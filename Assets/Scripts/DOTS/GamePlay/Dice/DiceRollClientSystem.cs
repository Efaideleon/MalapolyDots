using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace DOTS.GamePlay
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class DiceRollClientSystem : SystemBase
    {
        DiceRollPresenter presenter;
        uint lastSequence, settledSequence;

        protected override void OnUpdate()
        {
            using var commands = new EntityCommandBuffer(Allocator.Temp);
            bool inGame = !SystemAPI.QueryBuilder().WithAll<NetworkStreamInGame>().Build().IsEmptyIgnoreFilter;
            foreach (var (result, entity) in SystemAPI.Query<RefRO<DiceRollResultRpc>>()
                         .WithAll<ReceiveRpcCommandRequest>().WithEntityAccess())
            {
                var roll = result.ValueRO;
                if (inGame && roll.Sequence != lastSequence && roll.First >= 1 && roll.First <= 6 && roll.Second >= 1 && roll.Second <= 6)
                {
                    lastSequence = roll.Sequence;
                    if (presenter == null)
                    {
                        var prefab = Resources.Load<DiceRollPresenter>("Dice/DiceRollPresentation");
                        if (prefab != null)
                        {
                            presenter = Object.Instantiate(prefab);
                            presenter.name = "Dice Roll Presentation";
                            presenter.Settled += OnSettled;
                        }
                        else Debug.LogError("DiceRollPresentation prefab is missing. Run Tools > Malapoly > Dice > Build assets.");
                    }
                    if (presenter != null) presenter.Play(roll.First, roll.Second, roll.Sequence);
                    else settledSequence = roll.Sequence;
                }
                commands.DestroyEntity(entity);
            }
            if (inGame && settledSequence != 0)
            {
                var ack = commands.CreateEntity();
                commands.AddComponent(ack, new DiceSettledRpc { Sequence = settledSequence });
                commands.AddComponent<SendRpcCommandRequest>(ack);
                settledSequence = 0;
            }
            commands.Playback(EntityManager);
            if (!inGame)
            {
                presenter?.Hide();
                lastSequence = settledSequence = 0;
            }
        }

        void OnSettled(uint sequence) => settledSequence = sequence;
        protected override void OnDestroy()
        {
            if (presenter != null)
            {
                presenter.Settled -= OnSettled;
                Object.Destroy(presenter.gameObject);
            }
        }
    }
}
