using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace FinTech.Infrastructure.Monitoring
{
    public class CustomTelemetryInitializer : ITelemetryInitializer
    {
        private readonly string _roleName;
        private readonly string _roleInstance;
        private readonly string _environment;

        public CustomTelemetryInitializer()
        {
            _roleName = Environment.GetEnvironmentVariable("WEBSITE_SITE_NAME") ?? "FinTech.WebAPI";
            _roleInstance = Environment.MachineName;
            _environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        }

        public void Initialize(ITelemetry telemetry)
        {
            telemetry.Context.Cloud.RoleName = _roleName;
            telemetry.Context.Cloud.RoleInstance = _roleInstance;
            
            if (string.IsNullOrEmpty(telemetry.Context.GetInternalContext().SdkVersion))
            {
                var version = typeof(CustomTelemetryInitializer).Assembly.GetName().Version;
                telemetry.Context.GetInternalContext().SdkVersion = $"fintech-webapi:{version}";
            }
            
            // Add custom properties to all telemetry
            telemetry.Context.GlobalProperties["Environment"] = _environment;
            telemetry.Context.GlobalProperties["Application"] = "FinTech.WebAPI";
        }
    }
}
