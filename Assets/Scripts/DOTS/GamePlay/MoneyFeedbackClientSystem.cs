using System.Collections.Generic;
using Assets.Scripts.DOTS.Characters;
using DOTS.GamePlay.CameraSystems;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MoneyFeedbackClientSystem : SystemBase
    {
        struct Shot
        {
            public int Player, Delta;
            public MoneyChangeReason Reason;
            public bool FocusCamera => Reason != MoneyChangeReason.Purchase &&
                                       !(Reason == MoneyChangeReason.Building && Delta < 0);
        }
        readonly Queue<Shot> shots = new();
        Shot current;
        bool presenting;
        bool confettiPlayed;
        float elapsed, waiting;
        MoneyFeedbackOverlay overlay;
        PurchaseConfetti confetti;
        CinemachineCameraManager cameraManager;

        protected override void OnUpdate()
        {
            using var commands = new EntityCommandBuffer(Allocator.Temp);
            bool inGame = !SystemAPI.QueryBuilder().WithAll<NetworkStreamInGame>().Build().IsEmptyIgnoreFilter;
            foreach (var (message, entity) in SystemAPI.Query<RefRO<MoneyFeedbackRpc>>()
                         .WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                if (inGame)
                {
                    var rpc = message.ValueRO;
                    Enqueue(rpc.FirstPlayerId, rpc.FirstDelta, rpc.Reason);
                    Enqueue(rpc.SecondPlayerId, rpc.SecondDelta, rpc.Reason);
                }
                commands.DestroyEntity(entity);
            }
            commands.Playback(EntityManager);
            if (!inGame) { Clear(); return; }
            var manager = CinemachineCameraManager.Instance;
            if (manager == null) { Clear(); return; }
            cameraManager = manager;
            if (overlay == null) overlay = manager.GetComponent<MoneyFeedbackOverlay>() ?? manager.gameObject.AddComponent<MoneyFeedbackOverlay>();

            if (!presenting)
            {
                if (shots.Count == 0) return;
                current = shots.Dequeue();
                presenting = true;
                confettiPlayed = false;
                elapsed = waiting = 0;
            }
            bool found = false;
            Vector3 position = default;
            foreach (var (owner, transform) in SystemAPI.Query<RefRO<GhostOwner>, RefRO<LocalToWorld>>().WithAll<GhostMoneyComponet>())
                if (owner.ValueRO.NetworkId == current.Player)
                {
                    position = transform.ValueRO.Position;
                    found = true;
                    break;
                }
            if (!found)
            {
                waiting += UnityEngine.Time.unscaledDeltaTime;
                if (waiting > 1f) FinishShot();
                return;
            }
            if (!confettiPlayed && current.Reason == MoneyChangeReason.Purchase && current.Delta < 0)
            {
                if (confetti == null)
                    confetti = manager.GetComponent<PurchaseConfetti>() ?? manager.gameObject.AddComponent<PurchaseConfetti>();
                confettiPlayed = confetti.Play();
            }
            if (elapsed == 0)
            {
                if (!overlay.Show(current.Delta, current.Reason)) return;
                if (current.FocusCamera) manager.ShowMoneyShot(position);
            }
            if (current.FocusCamera) manager.UpdateMoneyShot(position);
            elapsed += UnityEngine.Time.unscaledDeltaTime;
            overlay.Track(position + Vector3.up * manager.MoneyHeadHeight, elapsed / manager.MoneyShotDuration);
            if (elapsed >= manager.MoneyShotDuration) FinishShot();
        }

        void Enqueue(int player, int delta, MoneyChangeReason reason)
        {
            if (player > 0 && delta != 0)
                shots.Enqueue(new Shot { Player = player, Delta = delta, Reason = reason });
        }

        void FinishShot()
        {
            presenting = false;
            overlay?.Hide();
            if (shots.Count == 0 || !shots.Peek().FocusCamera) cameraManager?.EndMoneyShot();
        }

        void Clear()
        {
            shots.Clear();
            confetti?.Clear();
            presenting = false;
            overlay?.Hide();
            cameraManager?.EndMoneyShot();
        }

        protected override void OnDestroy()
        {
            Clear();
            if (overlay != null) Object.Destroy(overlay);
            if (confetti != null) Object.Destroy(confetti);
        }
    }
}
