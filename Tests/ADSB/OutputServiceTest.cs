using ADSB.Output.Enums;
using System.Net;
using System.Text;
using Tests.Mock;
using Tests.Mock.Tests.Mock;

namespace Tests.ADSB
{
    [TestClass]
    public class ADSBOutputServiceTest
    {
        private int randomPort;
        private OutputService service = new(MockLogger.Factory, new TcpOutput("127.0.0.1", 0));

        [TestInitialize]
        public void Initialize()
        {
            randomPort = MockRandom.RandomPort;
            MockListener.Address = IPAddress.Any;
            MockListener.Port = randomPort;
            service = new(MockLogger.Factory, new TcpOutput("127.0.0.1", randomPort));

            Assert.AreEqual(OutputServiceStatus.Stopped, service.Status);
            Assert.AreEqual(true, service.Stopped);
        }

        [TestMethod]
        public void Start()
        {
            var buffer = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());

            var listenerThread = MockListener.NewThread;
            listenerThread.Start(TimeSpan.Zero);

            service.Start();
            Assert.AreEqual(OutputServiceStatus.Started, service.Status);
            Assert.AreEqual(true, service.Started);

            service.Write(buffer);
            Assert.AreEqual(OutputServiceStatus.Started, service.Status);
            Assert.AreEqual(true, service.Started);

            service.Stop();
            Assert.AreEqual(OutputServiceStatus.Stopped, service.Status);
            Assert.AreEqual(true, service.Stopped);

            listenerThread.Join();

            Assert.AreEqual(MockListener.LastReceivedBytes.Length, buffer.Length);
        }

        [TestMethod]
        public void StartFail()
        {
            var buffer = Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());

            service.Start();
            Assert.AreEqual(OutputServiceStatus.Stopped, service.Status);
            Assert.AreEqual(true, service.Stopped);

            service.Write(buffer);
            Assert.AreEqual(OutputServiceStatus.Stopped, service.Status);
            Assert.AreEqual(true, service.Stopped);

            service.Stop();
            Assert.AreEqual(OutputServiceStatus.Stopped, service.Status);
            Assert.AreEqual(true, service.Stopped);
        }
    }
}
