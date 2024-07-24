using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ServiceListener
{
    public partial class ServiceListener : ServiceBase
    {
        private TelemetryClient _telemetryClient;
        private string _serviceName;
        private string _instanceName;
        private int _intervalInSecs;

        public ServiceListener(TelemetryClient tc)
        {
            _telemetryClient = tc;
            InitializeComponent();

            _serviceName = Environment.GetEnvironmentVariable("ServiceMonitorServiceName");
            _instanceName = Environment.GetEnvironmentVariable("ServiceMonitorInstanceName");
            _intervalInSecs = Convert.ToInt32(Environment.GetEnvironmentVariable("ServiceMonitorIntervalInSecs"));
        }

        protected override void OnStart(string[] args)
        {
            _telemetryClient.TrackTrace("Starting to listen to service:" + _serviceName);

            // Set up a timer that triggers every minute.
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = 60000 // 60 seconds
            };
            timer.Elapsed += new ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, ElapsedEventArgs args)
        {
            _telemetryClient.TrackTrace("Starting to listen to service:" + _serviceName);
            try
            {


                while (true)
                {
                    using (_telemetryClient.StartOperation<RequestTelemetry>($"monitoring {_instanceName} service"))
                    {
                        if (!string.IsNullOrWhiteSpace(_serviceName))
                        {
                            var status = CheckStatus(_serviceName);
                            _telemetryClient.TrackEvent(_instanceName + ": " + status);
                        }
                        else
                        {
                            _telemetryClient.TrackTrace("service=not defined");
                        }
                    }

                    Thread.Sleep(_intervalInSecs);
                }
            }
            catch (Exception ex)
            {
                _telemetryClient.TrackException(ex);
                _telemetryClient.TrackTrace("ex=" + ex.Message);
            }
        }

        protected override void OnStop()
        {
            _telemetryClient.TrackTrace("Stopping listen to service:" + _serviceName);
        }
        string CheckStatus(string serviceName)
        {
            ServiceController sc = new ServiceController(serviceName);
            switch (sc.Status)
            {
                case ServiceControllerStatus.Running:
                    return "Running";
                case ServiceControllerStatus.Stopped:
                    return "Stopped";
                case ServiceControllerStatus.Paused:
                    return "Paused";
                case ServiceControllerStatus.StopPending:
                    return "Stopping";
                case ServiceControllerStatus.StartPending:
                    return "Starting";
                default:
                    return "Status Changing";
            }
        }
    }
}
