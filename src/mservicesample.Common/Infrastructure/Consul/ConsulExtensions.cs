using System;
using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using mservicesample.Common.Infrastructure.Fabio;
using mservicesample.Common.Services;

namespace mservicesample.Common.Infrastructure.Consul
{
    public  static  class ConsulExtensions
    {

        private static readonly string ConsulConfigSection = "consul";
        private static readonly string FabioConfigSection = "fabio";

        public static IServiceCollection AddConsul(this IServiceCollection services)
        {
            IConfiguration configuration;
            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            var options = configuration.GetOptions<ConsulServiceOptions>(ConsulConfigSection);
            services.Configure<ConsulServiceOptions>(configuration.GetSection(ConsulConfigSection));
            services.Configure<FabioServiceOptions>(configuration.GetSection(FabioConfigSection));
            //services.AddTransient<IConsulServicesRegistry, ConsulServicesRegistry>();
            //services.AddTransient<ConsulServiceDiscoveryMessageHandler>();
            //services.AddHttpClient<IConsulHttpClient, ConsulHttpClient>()
            //    .AddHttpMessageHandler<ConsulServiceDiscoveryMessageHandler>();

            services.AddSingleton<IConsulClient, ConsulClient>();
            return services.AddSingleton<IConsulClient>(c => new ConsulClient(cfg =>
            {
                if (!string.IsNullOrEmpty(options.ServiceDiscoveryAddress.Host))
                {
                    cfg.Address = options.ServiceDiscoveryAddress;
                }
            }));
        }

        //Returns unique service ID used for removing the service from registry.
        public static string UseConsul(this IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var consulOptions = scope.ServiceProvider.GetService<IOptions<ConsulServiceOptions>>();
                var fabioServiceOptions = scope.ServiceProvider.GetService<IOptions<FabioServiceOptions>>();
                var enabled = consulOptions.Value.Enabled;
                var consulEnabled = Environment.GetEnvironmentVariable("CONSUL_ENABLED")?.ToLowerInvariant();
                if (!string.IsNullOrWhiteSpace(consulEnabled))
                {
                    enabled = consulEnabled == "true" || consulEnabled == "1";
                }

                if (!enabled)
                {
                    return string.Empty;
                }


                var address = consulOptions.Value.ServiceAddress.Host;
                if (string.IsNullOrWhiteSpace(address))
                {
                    throw new ArgumentException("Consul address can not be empty.",
                        nameof(consulOptions.Value.ServiceAddress));
                }

                var uniqueId = scope.ServiceProvider.GetService<IServiceId>().Id;
                var client = scope.ServiceProvider.GetService<IConsulClient>();
                

                var serviceName = consulOptions.Value.ServiceName;
                var serviceId = $"{serviceName}:{uniqueId}";
                var port = consulOptions.Value.ServiceAddress.Port;
                
                var helthCheckEndPoint = consulOptions.Value.HealthCheckEndPoint;
                var pingInterval = consulOptions.Value.PingInterval <= 0 ? 5 : consulOptions.Value.PingInterval;
                var removeAfterInterval = consulOptions.Value.RemoveAfterInterval <= 0 ? 10 : consulOptions.Value.RemoveAfterInterval;

                var registration = new AgentServiceRegistration
                {
                    Name = serviceName,
                    ID = serviceId,
                    Address = address,
                    Port = port,
                    Tags = fabioServiceOptions.Value.Enabled ? GetFabioTags(serviceName, fabioServiceOptions.Value.ServiceName) : null
                };
                if (consulOptions.Value.PingEnabled || fabioServiceOptions.Value.Enabled)
                {
                    var scheme = address.StartsWith("http", StringComparison.InvariantCultureIgnoreCase)
                        ? string.Empty
                        : "http://";
                    var check = new AgentServiceCheck
                    {
                        Interval = TimeSpan.FromSeconds(pingInterval),
                        DeregisterCriticalServiceAfter = TimeSpan.FromSeconds(removeAfterInterval),
                        HTTP = $"{scheme}{address}{(port > 0 ? $":{port}" : string.Empty)}/{helthCheckEndPoint}"
                    };
                    registration.Checks = new[] { check };
                }
                //add service to consul
                client.Agent.ServiceRegister(registration);

                return serviceId;
            }
        }

        private static string[] GetFabioTags(string consulService, string fabioService)
        {
            var service = (string.IsNullOrWhiteSpace(fabioService) ? consulService : fabioService)
                .ToLowerInvariant();

            return new[] {$"urlprefix-/{service} strip=/{service}"};
        }
       
    }
}
