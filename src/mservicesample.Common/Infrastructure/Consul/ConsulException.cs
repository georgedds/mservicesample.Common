using System;

namespace mservicesample.Common.Infrastructure.Consul
{
    public class ConsulException: Exception
    {
        public string ServiceName { get;}
        
        public ConsulException(string serviceName) : this(string.Empty, serviceName)
        {
        }

        public ConsulException(string message, string serviceName) : base(message)
        {
            ServiceName = serviceName;
        }
    }
}
