using Microsoft.Extensions.Logging;

namespace Tests.Mock
{
    public static class MockLogger
    {
        public static ILoggerFactory Factory { get; private set; }

        static MockLogger()
        {
            Factory = LoggerFactory.Create(c =>
            {
                c.SetMinimumLevel(LogLevel.Trace);
            });
        }
    }
}
