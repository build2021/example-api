namespace Example.Api.Settings
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Ignore")]
    public class ServerSetting
    {
        public bool EnableSwagger { get; set; }

        public string[]? AllowSwagger { get; set; }

        public bool EnableMetrics { get; set; }

        public string[]? AllowMetrics { get; set; }

        public bool EnableProfiler { get; set; }

        public int LongTimeThreshold { get; set; }
    }
}
