namespace Tests.Mock
{
    public static class MockRandom
    {
        private static readonly Random _random = new();
        public static int RandomPort => _random.Next(30000, ushort.MaxValue - 1);
        public static int RandomBetween(int min, int max) => _random.Next(min, max);
    }
}
