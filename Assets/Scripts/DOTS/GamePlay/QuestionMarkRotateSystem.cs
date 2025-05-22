using DOTS.GameSpaces;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTS.GamePlay
{
    [BurstCompile]
    public partial struct QuestionMarkRotateSystem : ISystem
    {
        private const float RadiansPerSecond = math.PI / 16;
        private float _accumulatedRadians;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            _accumulatedRadians = 0;
            state.RequireForUpdate<ChanceSpaceTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            _accumulatedRadians = (dt * RadiansPerSecond + _accumulatedRadians) % (2 * math.PI);
            var quaternionRotated = quaternion.RotateY(_accumulatedRadians);

            foreach (var (localTransform, _) in SystemAPI.Query<RefRW<LocalTransform>, RefRO<ChanceSpaceTag>>())
                localTransform.ValueRW.Rotation = quaternionRotated;
        }
    }
}
