using ADSB.Output.Enums;
using System.Text;
using Tests.Mock;
using Tests.Mock.Tests.Mock;

namespace Tests.ADSB
{
    [TestClass]
    public class OutputTcpTest
    {
        private int randomPort;
        private TcpOutput output = new("0.0.0.0", 0);
        private Thread listenerThread = MockListener.NewThread;

        [TestInitialize]
        public void Initialize()
        {
            randomPort = MockRandom.RandomPort;
            MockListener.Address = System.Net.IPAddress.Any;
            MockListener.Port = randomPort;
            listenerThread = MockListener.NewThread;

            output = new("localhost", randomPort);

            Assert.AreEqual("localhost", output.Hostname);
            Assert.AreEqual(randomPort, output.Port);
            Assert.AreEqual($"localhost:{randomPort}", output.ToString());
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);
        }

        [TestMethod]
        public void Connection()
        {
            listenerThread.Start(TimeSpan.Zero);

            var result = output.ConnectAsync().Result;
            Assert.AreEqual(true, result);
            Assert.AreEqual(OutputStatus.Connected, output.Status);

            output.Disconnect();
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);

            listenerThread.Join();
        }

        [TestMethod]
        public void DelayedConnection()
        {
            listenerThread.Start(TimeSpan.FromSeconds(4));

            var result = output.ConnectAsync().Result;
            Assert.AreEqual(true, result);
            Assert.AreEqual(OutputStatus.Connected, output.Status);

            output.Disconnect();
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);

            listenerThread.Join();
        }

        [TestMethod]
        public void ConnectionFailed()
        {
            var result = output.ConnectAsync().Result;
            Assert.AreEqual(false, result);
            Assert.AreEqual(OutputStatus.Failed, output.Status);

            output.Disconnect();
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);
        }

        [TestMethod]
        public async Task Write()
        {
            var messageString = "Hello World";
            listenerThread.Start(TimeSpan.Zero);

            var result = output.ConnectAsync().Result;
            Assert.AreEqual(true, result);
            Assert.AreEqual(OutputStatus.Connected, output.Status);

            var buffer = Encoding.UTF8.GetBytes(messageString);
            await output.WriteAsync(buffer);

            output.Disconnect();
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);

            listenerThread.Join();
            Assert.AreEqual(messageString, MockListener.LastReceivedString);
        }

        [TestMethod]
        public async Task DelayedWrite()
        {
            var messageString = "Hello World";
            listenerThread.Start(TimeSpan.FromSeconds(4));

            var result = output.ConnectAsync().Result;
            Assert.AreEqual(true, result);
            Assert.AreEqual(OutputStatus.Connected, output.Status);

            var buffer = Encoding.UTF8.GetBytes(messageString);
            await output.WriteAsync(buffer);

            output.Disconnect();
            Assert.AreEqual(OutputStatus.Disconnected, output.Status);

            listenerThread.Join();
            Assert.AreEqual(messageString, MockListener.LastReceivedString);
        }
    }
}
