using DOTS.DataComponents;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

namespace DOTS.Mediator
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct LoadPriceToQuadBuffer : ISystem
    {
        private const int QuetzalSign = 10;
        private const int Coma = 11;

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkId>();
            state.RequireForUpdate<QuadDataBuffer>();
            state.RequireForUpdate<NumberToUVOffset>();
        }

        public void OnUpdate(ref SystemState state)
        {
            // TODO: this is part of the property space tag.
            foreach (var (price, quadBuffer) in
                SystemAPI.Query<RefRO<GhostPriceComponent>, DynamicBuffer<QuadDataBuffer>>().WithChangeFilter<GhostPriceComponent>())
            {
                var numToUVOffsetMap = SystemAPI.GetSingleton<NumberToUVOffset>().Map;

                int digitCount = GetDigitCount(price.ValueRO.Value);
                int numOfExtraQuads = digitCount > 3 ? 2 : 1;
                int totalNumberOfQuads = digitCount + numOfExtraQuads;

                NativeArray<int> quadNums = new(totalNumberOfQuads, Allocator.Temp);
                GetQuadValues(price.ValueRO.Value, quadNums);

                quadBuffer.Clear();
                // Loading quad uv offsets for all properties.
                for (int i = 0; i < totalNumberOfQuads; i++)
                {
                    var quadUVOffset = numToUVOffsetMap[quadNums[i]];
                    quadBuffer.Add(new QuadDataBuffer { UVOffset = quadUVOffset });
                    UnityEngine.Debug.Log($"[LoadPriceToQuadBuffer] | adding to quadUVOffset {quadUVOffset}");
                    // TODO: this data is set in the server;
                    // But then it should be read in the client.
                    // There should be a way to send an rpc to the client to update the quad entities now
                }
            }
        }

        public static int GetComaPosition(int digitCount)
        {
            if (digitCount < 4) return -1;
            return digitCount - 2;
        }

        public static void GetQuadValues(int Number, NativeArray<int> result)
        {
            int dividend = Number;
            int digitCount = GetDigitCount(Number);
            int comaPosition = GetComaPosition(digitCount);

            for (int i = result.Length - 1; i >= 0; i--)
            {
                // Add `Q` to the first quad.
                if (i == 0)
                {
                    result[i] = QuetzalSign;
                    continue;
                }

                // Add `,`
                if (i == comaPosition)
                {
                    result[i] = Coma;
                    continue;
                }

                // Digit
                result[i] = dividend % 10;

                dividend /= 10;
            }
        }

        public static int GetDigitCount(int num)
        {
            if (num < 10) return 1;
            if (num < 100) return 2;
            if (num < 1_000) return 3;
            if (num < 10_000) return 4;
            if (num < 100_000) return 5;
            if (num < 1_000_000) return 6;
            return -1;
        }
    }
}
