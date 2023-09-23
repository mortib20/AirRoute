using ADSB.Output.Enums;
using Microsoft.Extensions.Logging;

namespace ADSB.Output
{
    public class OutputService
    {
        // Fields
        private readonly ILogger _logger;
        private readonly TcpOutput Output;

        // Properies
        public OutputServiceStatus Status { get; private set; }
        public bool Started => Status == OutputServiceStatus.Started;
        public bool Stopped => Status == OutputServiceStatus.Stopped;
        public bool Failed => Status == OutputServiceStatus.Failed;

        // Constructor
        public OutputService(ILoggerFactory loggerFactory, TcpOutput output)
        {
            _logger = loggerFactory.CreateLogger($"{nameof(OutputService)} {output}");
            Output = output;
            Status = OutputServiceStatus.Stopped;
        }

        // Methods
        public async void Write(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (!Output.Connected)
            {
                return;
            }

            try
            {
                await Output.WriteAsync(buffer, cancellationToken);
            }
            catch (IOException)
            {
                Status = OutputServiceStatus.Failed;
                throw;
            }
        }

        public async void Start(CancellationToken cancellationToken = default)
        {
            if (Started)
            {
                return;
            }

            Status = await Output.ConnectAsync(cancellationToken) ? OutputServiceStatus.Started : OutputServiceStatus.Failed;

            if (Started)
            {
                _logger.LogInformation("Started");
            }
            else
            {
                _logger.LogInformation("Failed to start");
            }
        }

        public void Stop()
        {
            if (Stopped)
            {
                return;
            }

            _logger.LogInformation("Stopping");
            Status = OutputServiceStatus.Stopped;
            Output.Disconnect();
        }

        public override string ToString()
        {
            return Output.ToString();
        }
    }
}
