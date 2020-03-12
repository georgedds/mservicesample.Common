using System;

namespace mservicesample.Common.Infrastructure.Consul
{
    public class ConsulServiceOptions
    {
        public Uri ServiceDiscoveryAddress { get; set; }
        public Uri ServiceAddress { get; set; }
        public string ServiceName { get; set; }
        //public string ServiceId { get; set; }
        public bool Enabled { get; set; }
        public bool PingEnabled { get; set; }
        public int PingInterval { get; set; }
        public int RemoveAfterInterval { get; set; }
        public int RequestRetries { get; set; }
        //public bool SkipLocalhostDockerDnsReplace { get; set; }
        public string HealthCheckEndPoint { get; set; }

    }
}
