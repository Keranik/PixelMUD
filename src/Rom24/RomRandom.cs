namespace Rom24
{
    /* glibc TYPE_3 random()/srandom() (db.c number_mm when OLD_RAND is off). */
    public static class RomRandom
    {
        const int DEG_3 = 31;
        const int SEP_3 = 3;

        static readonly int[] state = new int[DEG_3];
        static int fptr;
        static int rptr;

        public static void init_mm()
        {
            uint seed = (uint)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() ^ Environment.ProcessId);
            srandom(seed);
        }

        public static void srandom(uint seed)
        {
            if (seed == 0)
                seed = 1;
            int word = (int)seed;
            state[0] = word;
            for (int i = 1; i < DEG_3; i++)
            {
                long hi = word / 127773;
                long lo = word % 127773;
                word = (int)(16807 * lo - 2836 * hi);
                if (word < 0)
                    word += 2147483647;
                state[i] = word;
            }
            fptr = SEP_3;
            rptr = 0;
            for (int i = DEG_3 * 10; i > 0; i--)
                random();
        }

        public static int random()
        {
            unchecked
            {
                uint val = (uint)state[fptr] + (uint)state[rptr];
                state[fptr] = (int)val;
                int result = (int)(val >> 1);
                fptr++;
                if (fptr >= DEG_3)
                {
                    fptr = 0;
                    rptr++;
                }
                else
                {
                    rptr++;
                    if (rptr >= DEG_3)
                        rptr = 0;
                }
                return result;
            }
        }

        public static long number_mm() => random() >> 6;

        public static int number_bits(int width) => (int)(number_mm() & ((1 << width) - 1));

        public static int number_fuzzy(int number)
        {
            switch (number_bits(2))
            {
                case 0: number -= 1; break;
                case 3: number += 1; break;
            }
            return Bit.UMAX(1, number);
        }

        public static int number_range(int from, int to)
        {
            if (from == 0 && to == 0) return 0;
            to = to - from + 1;
            if (to <= 1) return from;
            int power;
            for (power = 2; power < to; power <<= 1) { }
            int number;
            while ((number = (int)(number_mm() & (power - 1))) >= to) { }
            return from + number;
        }

        public static int number_percent()
        {
            int percent;
            while ((percent = (int)(number_mm() & (128 - 1))) > 99) { }
            return 1 + percent;
        }

        public static int number_door()
        {
            int door;
            while ((door = (int)(number_mm() & (8 - 1))) > 5) { }
            return door;
        }

        public static int dice(int number, int size)
        {
            switch (size)
            {
                case 0: return 0;
                case 1: return number;
            }
            int sum = 0;
            for (int i = 0; i < number; i++)
                sum += number_range(1, size);
            return sum;
        }

        public static int interpolate(int level, int value_00, int value_32)
            => value_00 + level * (value_32 - value_00) / 32;
    }
}
