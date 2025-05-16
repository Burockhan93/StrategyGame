
namespace Shared.Communication
{
    class Constants
    {
        public const int TICKS_PER_SEC = 30;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;

        public const int COUNTER_MAX = 150;

        public const int SEC_PER_GAMETICK = COUNTER_MAX / TICKS_PER_SEC;

        public const int SAVE_RATE = 10;

        public static int MinutesToGameTicks(int minutes)
        {
            return (minutes * (60 / SEC_PER_GAMETICK));
        }

        public static int HoursToGameTicks(int hours)
        {
            return 2;
            return ((hours * 3600) / SEC_PER_GAMETICK);
        }
    }
}
