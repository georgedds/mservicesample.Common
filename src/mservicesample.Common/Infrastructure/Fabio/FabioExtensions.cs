using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace mservicesample.Common.Infrastructure.Fabio
{
    public static  class FabioExtensions
    {
        private static readonly string FabioConfigSection = "fabio";
        public static IServiceCollection AddFabio(this IServiceCollection services)
        {
            using (var serviceProvider = services.BuildServiceProvider())
            {
                var configuration = serviceProvider.GetService<IConfiguration>();
                services.Configure<FabioServiceOptions>(configuration.GetSection(FabioConfigSection));
            }
            return services;
        }
    }
}
