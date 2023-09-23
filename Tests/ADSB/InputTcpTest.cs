using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using ADSB.Input.Enums;
using Tests.Mock;

namespace Tests.ADSB
{
    [TestClass]
    public class InputTcpTest
    {
        private int randomPort;
        private TcpInput input = new(IPAddress.Any, default);

        [TestInitialize]
        public void Initialize()
        {
            randomPort = MockRandom.RandomPort;
            MockClient.Port = randomPort;
            input = new(IPAddress.Any, randomPort);

            Assert.AreEqual("127.0.0.1", MockClient.Address);
            Assert.AreEqual(MockClient.Port, randomPort);
            Assert.AreEqual(InputStatus.Stopped, input.Status);
        }

        [TestMethod]
        public async void Listen()
        {
            CancellationTokenSource cSource = new();
            cSource.Cancel();
            input.HandleConnection += (_, _) =>
            {
                Assert.AreEqual(InputStatus.Listening, input.Status);
            };

            await input.Listen(cSource.Token);
            Assert.AreEqual(InputStatus.Stopped, input.Status);
        }

        [TestMethod]
        public async Task ClientConnecting()
        {
            var clientThread = MockClient.NewThread;
            CancellationTokenSource cSource = new();

            clientThread.Start(new MockClient.MockClientParameters(TimeSpan.FromSeconds(2), 1));

            input.HandleConnection += (client, token) =>
            {
                Assert.AreEqual(InputStatus.Listening, input.Status);
                Debug.WriteLine($"{client.Client.RemoteEndPoint} connected");
                var stream = client.GetStream();

                Debug.WriteLine($"Closing connection");
                stream.Close();
                cSource.Cancel();
            };

            await input.Listen(cSource.Token);

            Assert.AreEqual(InputStatus.Stopped, input.Status);

            clientThread.Join();
        }

        [TestMethod]
        public async Task ClientWrite()
        {
            var clientThread = MockClient.NewThread;
            CancellationTokenSource cSource = new();
            MockClient.StringToWrite = Guid.NewGuid().ToString();

            clientThread.Start(new MockClient.MockClientParameters(TimeSpan.Zero, 1));

            input.HandleConnection += (client, token) =>
            {
                Assert.AreEqual(InputStatus.Listening, input.Status);
                var stream = client.GetStream();

                Debug.WriteLine($"{client.Client.RemoteEndPoint} connected");
                var buffer = new byte[2048];

                Debug.WriteLine($"Reading");

                var length = stream.Read(buffer);
                Debug.WriteLine($"Received length -> {length}");

                var receivedBuffer = buffer.Take(length).ToArray();

                Debug.WriteLine($"Received string -> '{Encoding.UTF8.GetString(receivedBuffer)}'");

                Assert.AreEqual(Encoding.UTF8.GetString(MockClient.BytesToWrite), Encoding.UTF8.GetString(receivedBuffer));
                Assert.AreEqual(MockClient.BytesToWrite.Length, length);

                Debug.WriteLine($"Closing connection");
                stream.Close();
                cSource.Cancel();
            };

            await input.Listen(cSource.Token);

            Assert.AreEqual(InputStatus.Stopped, input.Status);

            clientThread.Join();
        }

        [TestMethod]
        public async Task ClientConnectMany()
        {
            for (int i = 0; i < 10; i++)
            {
                var clientThread = MockClient.NewThread;
                CancellationTokenSource cSource = new();

                clientThread.Start(new MockClient.MockClientParameters(TimeSpan.Zero, 1));

                MockClient.StringToWrite = Guid.NewGuid().ToString();

                input.HandleConnection += Handle;
                input.HandleConnection += (_, _) => cSource.Cancel();

                await input.Listen(cSource.Token);

                input.HandleConnection -= Handle;

                clientThread.Join();
                Debug.WriteLine("");
            }

            Assert.AreEqual(InputStatus.Stopped, input.Status);

            void Handle(TcpClient client, CancellationToken cancellationToken)
            {
                Assert.AreEqual(InputStatus.Listening, input.Status);
                var stream = client.GetStream();
                Debug.WriteLine($"{client.Client.RemoteEndPoint} connected");
                var buffer = new byte[2048];

                Debug.WriteLine($"Reading");

                var length = stream.Read(buffer);
                Debug.WriteLine($"Received length -> {length}");

                var receivedBuffer = buffer.Take(length).ToArray();

                Debug.WriteLine($"Received string -> '{Encoding.UTF8.GetString(receivedBuffer)}'");

                Assert.AreEqual(MockClient.StringToWrite, Encoding.UTF8.GetString(receivedBuffer));
                Assert.AreEqual(MockClient.BytesToWrite.Length, length);

                Debug.WriteLine($"Closing connection");
                stream.Close();
            }
        }


        [TestMethod]
        public async Task ClientWriteMany()
        {
            int writeAmount = MockRandom.RandomBetween(2, 50);
            MockClient.StringToWrite = Guid.NewGuid().ToString();
            var clientThread = MockClient.NewThread;
            CancellationTokenSource cSource = new();

            clientThread.Start(new MockClient.MockClientParameters(TimeSpan.FromSeconds(2), writeAmount));

            input.HandleConnection += (client, token) =>
            {
                Assert.AreEqual(InputStatus.Listening, input.Status);
                var stream = client.GetStream();

                while (client.Connected)
                {
                    if (client.Client.Available == 0)
                    {
                        break;
                    }
                    int available = client.Client.Available;

                    Debug.WriteLine($"Data available {client.Client.Available}");
                    var buffer = new byte[available];

                    var length = stream.Read(buffer);
                    Debug.WriteLine($"Data available after read {client.Client.Available}");

                    var receivedBuffer = buffer.Take(length).ToArray();
                    var receivedString = Encoding.UTF8.GetString(receivedBuffer);


                    Debug.WriteLine($"Received -> {receivedString}");
                    Assert.AreEqual(available, length);
                }

                cSource.Cancel();
            };

            await input.Listen(cSource.Token);

            clientThread.Join();
            Assert.AreEqual(InputStatus.Stopped, input.Status);
        }
    }
}
