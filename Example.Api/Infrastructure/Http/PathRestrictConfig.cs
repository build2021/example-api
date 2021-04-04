namespace Example.Api.Infrastructure.Http
{
    using System.Net;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification = "Ignore")]
    public class PathRestrictConfig
    {
        public string? Path { get; set; }

        public IPNetwork[]? Networks { get; set; }
    }
}
