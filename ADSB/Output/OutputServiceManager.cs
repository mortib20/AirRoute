using Microsoft.Extensions.Logging;

namespace ADSB.Output
{
    public class OutputServiceManager : IDisposable
    {
        // Fields
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly List<OutputService> _outputs;

        // Properties
        public IEnumerable<OutputService> Services => _outputs.AsEnumerable();
        public IEnumerable<OutputService> FailedServices => _outputs.FindAll(m => m.Failed).AsEnumerable();

        // Constructor
        public OutputServiceManager(ILogger<OutputServiceManager> logger, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = logger;
            _outputs = new();
        }


        // Methods
        public void AddService(string hostname, int port)
        {
            var service = new OutputService(_loggerFactory, new TcpOutput(hostname, port));

            if (_outputs.Contains(service))
            {
                _logger.LogInformation("{service} already existing", service);
                return;
            }

            _outputs.Add(service);
            _logger.LogInformation("{service} added", service);
        }

        public void RemoveService(OutputService service)
        {
            var found = _outputs.Find(s => s == service);

            if (found is null)
            {
                _logger.LogInformation("{service} not found", service);
                return;
            }

            _outputs.Remove(found);
            _logger.LogInformation("{service} removed", service);
        }

        public void StartAll(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Starting Services");
            _outputs.ForEach(o => o.Start(cancellationToken));
        }

        public void StopAll()
        {
            _logger.LogInformation("Stopping Services");
            _outputs.ForEach(o => o.Stop());
        }

        public void WriteAll(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _outputs.ForEach(o => o.Write(buffer, cancellationToken));
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            StopAll();
        }
    }
}
