using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using Tests.Mock;
using Tests.Mock.Tests.Mock;

namespace Tests.ADSB
{
    [TestClass]
    public class ADSBOutputManagerTest
    {
        private int randomPort;
        private OutputServiceManager manager = new(MockLogger.Factory.CreateLogger<OutputServiceManager>(), MockLogger.Factory);

        [TestInitialize]
        public void Initialize()
        {
            randomPort = MockRandom.RandomPort;
            MockListener.Address = IPAddress.Any;
            MockListener.Port = randomPort;
            var loggerFactory = MockLogger.Factory;
            manager = new(loggerFactory.CreateLogger<OutputServiceManager>(), MockLogger.Factory);

            manager.AddService("127.0.0.1", randomPort);
        }

        [TestMethod]
        public void WriteAll()
        {
            var buffer = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            var listenerThread = MockListener.NewThread;

            listenerThread.Start(TimeSpan.Zero);

            manager.AddService("127.0.0.1", randomPort);

            manager.StartAll();

            manager.WriteAll(buffer);

            manager.StopAll();

            listenerThread.Join();

            Assert.AreEqual(MockListener.LastReceivedBytes.Length, buffer.Length);
        }
    }
}
