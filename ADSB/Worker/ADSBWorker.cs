using ADSB.Input;
using ADSB.Output;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Sockets;

namespace ADSB.Worker
{
    public class ADSBWorker : BackgroundService
    {
        // Constants
        private readonly TimeSpan STATUS_CHECK_INTERVAL = TimeSpan.FromSeconds(10);
        private const int BUFFER_SIZE = 2048;

        // Fields
        private readonly Timer _StatusCheckTimer;
        private readonly ILogger<ADSBWorker> _logger;
        private readonly TcpInput _input;
        private readonly OutputServiceManager _outputManager;

        // Constructor
        public ADSBWorker(ILogger<ADSBWorker> logger, TcpInput input, OutputServiceManager outputManager)
        {
            _StatusCheckTimer = new(StatusCheck, null, STATUS_CHECK_INTERVAL, STATUS_CHECK_INTERVAL);
            _logger = logger;
            _input = input;
            _outputManager = outputManager;
        }

        // Methods
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Starting");

            try
            {
                AddOutputServices(stoppingToken);
                await Listen(stoppingToken);
            }
            catch (OperationCanceledException) { }
            _logger.LogInformation("Stopped");
        }

        private async Task Listen(CancellationToken stoppingToken)
        {
            _input.HandleConnection += HandleClient;
            await _input.Listen(stoppingToken);

            _outputManager.StopAll();
        }

        private void AddOutputServices(CancellationToken stoppingToken)
        {
            _outputManager.AddService("feed.adsb.lol", 30004);
            _outputManager.AddService("feed.adsb.fi", 30004);
            _outputManager.AddService("feed.adsb.one", 64004);
            _outputManager.AddService("feed.planespotters.net", 30004);
            _outputManager.AddService("feed1.adsbexchange.com", 30004);

            _outputManager.StartAll(stoppingToken);
        }

        private async void HandleClient(TcpClient client, CancellationToken cancellationToken)
        {
            var stream = client.GetStream();
            try
            {
                _logger.LogInformation("{Client.Client.RemoteEndPoint} connected", client.Client.RemoteEndPoint);
                while (client.Connected)
                {
                    var buffer = new byte[BUFFER_SIZE];
                    var length = await stream.ReadAsync(buffer, cancellationToken);

                    if (length == 0)
                    {
                        break;
                    }

                    _outputManager.WriteAll(buffer.Take(length).ToArray(), cancellationToken);
                }

                _logger.LogInformation("{Client.Client.RemoteEndPoint} disconnected", client.Client.RemoteEndPoint);
            }
            catch (OperationCanceledException) { }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        private void StatusCheck(object? state)
        {
            foreach (var output in _outputManager.FailedServices)
            {
                output.Start();
            }
        }
    }
}
