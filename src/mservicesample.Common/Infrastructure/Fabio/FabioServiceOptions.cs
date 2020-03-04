using System;

namespace mservicesample.Common.Infrastructure.Fabio
{
    public class FabioServiceOptions
    {
        public bool Enabled { get; set; }
        public Uri FabioAddress { get; set; }
        public string ServiceName { get; set; }
        public int RequestRetries { get; set; }
    }
}
