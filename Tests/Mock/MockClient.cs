using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace Tests.Mock
{
    public static class MockClient
    {
        private const string NAME = nameof(MockClient);
        public class MockClientParameters
        {
            public MockClientParameters(TimeSpan timeout, int amount)
            {
                Timeout = timeout;
                Amount = amount;
            }

            public TimeSpan Timeout { get; set; }
            public int Amount { get; set; }
        }

        private static TcpClient _client = new();
        public static string Address { get; set; } = "127.0.0.1";
        public static int Port { get; set; }
        public static string StringToWrite { get; set; } = "";
        public static byte[] BytesToWrite => Encoding.UTF8.GetBytes(StringToWrite);
        public static Exception LastException { get; private set; } = new();

        public static Thread NewThread => new(ClientThread);

        private static void ClientThread(object? param)
        {
            if (param is not MockClientParameters parameter)
            {
                Debug.WriteLine("Param was not TimeSpan", NAME);
                return;
            }

            Debug.WriteLine("Initialized", NAME);
            _client = new();

            Debug.WriteLine($"Waiting {parameter.Timeout:dd\\.hh\\:mm\\:ss}", NAME);
            Thread.Sleep(parameter.Timeout);

            Debug.WriteLine($"Connecting to {Address}:{Port}", NAME);
            try
            {
                _client.Connect(Address, Port);
            }
            catch (Exception ex)
            {
                LastException = ex;
                Debug.WriteLine($"Connection failed - {ex.Message}", NAME);
                return;
            }

            var stream = _client.GetStream();
            Debug.WriteLine($"Stream opened", NAME);

            for (var i = 0; i < parameter.Amount; i++)
            {
                try
                {
                    Debug.WriteLine($"Write -> {StringToWrite}", NAME);
                    stream.Write(BytesToWrite);
                }
                catch (Exception ex)
                {
                    LastException = ex;
                    Debug.WriteLine($"Failed to write - {ex.Message}", NAME);
                }

                //Thread.Sleep(10);
            }

            Debug.WriteLine("Closing stream", NAME);
            _client.Close();
        }
    }
}
