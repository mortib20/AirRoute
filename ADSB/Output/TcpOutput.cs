using ADSB.Output.Enums;
using System.Net;
using System.Net.Sockets;

namespace ADSB.Output
{
    public class TcpOutput : IDisposable
    {
        // Fields
        private TcpClient _client;

        public readonly DnsEndPoint RemoteEndPoint;
        public readonly string Hostname;
        public readonly int Port;

        // Properties
        public OutputStatus Status { get; private set; }
        public bool Connected => Status == OutputStatus.Connected;
        public bool Disconnected => Status == OutputStatus.Disconnected;
        public bool Failed => Status == OutputStatus.Failed;

        // Constructor
        public TcpOutput(string hostname, int port)
        {
            RemoteEndPoint = new(hostname, port);
            Hostname = hostname;
            Port = port;
            _client = new();
        }

        // Methods
        public async ValueTask WriteAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            try
            {
                await _client.GetStream().WriteAsync(buffer, cancellationToken);
            }
            catch (Exception)
            {
                Status = OutputStatus.Failed;
                throw;
            }
        }

        public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default)
        {
            _client = new();

            try
            {
                await _client.ConnectAsync(RemoteEndPoint.Host, RemoteEndPoint.Port, cancellationToken);
                Status = _client.Connected ? OutputStatus.Connected : OutputStatus.Disconnected;
                return true;
            }
            catch (Exception)
            {
                Status = OutputStatus.Failed;
            }

            return false;
        }

        public void Disconnect()
        {
            _client.Close();
            _client.Dispose();
            Status = OutputStatus.Disconnected;
        }

        public override string ToString()
        {
            return $"{Hostname}:{Port}";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (!dispose)
            {
                return;
            }

            Disconnect();
        }
    }
}