using System;
using System.Collections.Generic;

namespace Accounting
{
    public class AppConfig
    {
        public string ActiveServerKey { get; set; } = string.Empty;
        public bool AllowEnvironmentFallback { get; set; }
        public Dictionary<string, OracleServerConfig> Servers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    }

    public class OracleServerConfig
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 1521;
        public string ServiceName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
