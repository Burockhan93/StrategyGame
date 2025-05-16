
namespace Shared.Communication
{
    class Constants
    {
        public const int TICKS_PER_SEC = 30; // A second has 30 Ticks
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC; // a Milisecond has 0,03 Tick, A tick has 33 ms

        public const int COUNTER_MAX = 150;

        public const int SEC_PER_GAMETICK = COUNTER_MAX / TICKS_PER_SEC; // after this much time game will tick. About 4.5 sec

        public const int SAVE_RATE = 100;

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
