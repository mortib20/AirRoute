using System.Net;
using System.Net.Sockets;
using ADSB.Input.Enums;

namespace ADSB.Input
{
    public class TcpInput : IDisposable
    {
        // Events
        public event Action<TcpClient, CancellationToken>? HandleConnection;

        // Fields
        private readonly TcpListener _listener;
        public readonly IPAddress IpAddress;
        public readonly int Port;

        // Properties
        public InputStatus Status { get; private set; }
        public bool Listening => Status == InputStatus.Listening;
        public bool Stopped => Status == InputStatus.Stopped;
        public bool Failed => Status == InputStatus.Failed;

        // Constructor
        public TcpInput(IPAddress ipAddress, int port)
        {
            _listener = new(ipAddress, port);
            IpAddress = ipAddress;
            Port = port;
        }

        // Methods
        public async ValueTask Listen(CancellationToken cancellationToken = default)
        {
            Start();
            await HandleConnections(cancellationToken);
            Stop();
        }

        private void Start()
        {
            _listener.Start();
            Status = InputStatus.Listening;
        }

        private void Stop()
        {
            _listener.Stop();
            Status = InputStatus.Stopped;
        }

        private async ValueTask HandleConnections(CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken);

                HandleConnection?.Invoke(client, cancellationToken);
            }
        }

        public override string ToString()
        {
            return $"{IpAddress}:{Port}";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            Stop();
        }
    }
}
