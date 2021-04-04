namespace Example.Api.Infrastructure.Filters
{
    public sealed class TimeLoggingOptions
    {
        public string Key { get; } = "_TimeLogging";

        public long Threshold { get; set; }
    }
}
