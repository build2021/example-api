namespace Example.Api.Metrics
{
    using Prometheus;

    public static class ApiMetrics
    {
        private static readonly Counter DataGetCounter =
            Prometheus.Metrics.CreateCounter("api_data_get", "Data get request count.");

        public static void IncrementDataGet() => DataGetCounter.Inc();

        private static readonly Counter DataPostCounter =
            Prometheus.Metrics.CreateCounter("api_data_post", "Data post request count.");

        public static void IncrementDataPost() => DataPostCounter.Inc();
    }
}
