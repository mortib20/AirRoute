using ADSB.Input;
using ADSB.Output;
using ADSB.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net;

namespace AirRoute
{
    internal class Program
    {
        static async Task Main()
        {
            var builder = Host.CreateApplicationBuilder();

            builder.Services.AddSingleton<OutputServiceManager>();
            builder.Services.AddSingleton(typeof(TcpInput), new TcpInput(IPAddress.Any, 30004));
            builder.Services.AddHostedService<ADSBWorker>();

            builder.Logging.AddSimpleConsole();

            var app = builder.Build();

            await app.RunAsync();
        }
    }
}