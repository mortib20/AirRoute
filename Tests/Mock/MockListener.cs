namespace Tests.Mock
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;

    namespace Tests.Mock
    {
        internal static class MockListener
        {
            private const string NAME = nameof(MockListener);
            private static TcpListener? _listener;
            public static IPAddress Address { get; set; } = IPAddress.Any;
            public static byte[] LastReceivedBytes { get; private set; } = new byte[1];
            public static string LastReceivedString => Encoding.UTF8.GetString(LastReceivedBytes);
            public static int Port { get; set; }

            public static Thread NewThread => new(ListenerThread);

            private static void ListenerThread(object? param)
            {
                if (param is not TimeSpan timeout)
                {
                    Debug.WriteLine("Param was not TimeSpan", NAME);
                    return;
                }

                Debug.WriteLine("Initialized", NAME);
                _listener = new(Address, Port);

                Debug.WriteLine($"Waiting {timeout:dd\\.hh\\:mm\\:ss}", NAME);
                Thread.Sleep(timeout);

                Debug.WriteLine($"Starting on {Address}:{Port}", NAME);
                _listener.Start();

                using var client = _listener.AcceptTcpClient();
                using var stream = client.GetStream();
                Debug.WriteLine("Client connected", NAME);

                int available = _listener.Server.Available;

                var buffer = new byte[2048];
                var length = stream.Read(buffer);

                if (length == 0)
                {
                    Debug.WriteLine("Received Length was ZERO", NAME);
                    return;
                }

                LastReceivedBytes = buffer.Take(length).ToArray();

                Debug.WriteLine($"Received Length: {length}", NAME);
                Debug.WriteLine($"Data: {LastReceivedString}", NAME);

                Debug.WriteLine("Close client connection", NAME);
                client.Close();

                Debug.WriteLine("Shutdown", NAME);
                _listener.Server.Close();
                _listener.Stop();
            }
        }
    }
}
