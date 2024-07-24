using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ServiceProcess;
using System;
using System.IO;
using System.Configuration;
using System.Diagnostics;

namespace ServiceListener
{
    internal static class Program
    {
        private static string _instrumentationkey;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            _instrumentationkey = Environment.GetEnvironmentVariable("ServiceMonitorInstrumentationKey");


            IServiceCollection services = new ServiceCollection();

            services.AddLogging(loggingBuilder => loggingBuilder.AddFilter<Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider>("Category", LogLevel.Information));
            services.AddApplicationInsightsTelemetryWorkerService((ApplicationInsightsServiceOptions options) => options.ConnectionString = "InstrumentationKey="+_instrumentationkey);

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new ServiceListener(telemetryClient)
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
