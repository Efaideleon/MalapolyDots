namespace Assets.Common
{
    public static class SoloSession
    {
        public const int FirstAiId = 1001;
        public static bool Active, Paused, StartRequested;
        public static int Opponents = 3;
        public static string Status = "";
        public static bool IsAiId(int id) => Active && id >= FirstAiId && id < FirstAiId + 5;

        public static void Reset()
        {
            Active = Paused = StartRequested = false;
            Status = "";
        }
    }
}
