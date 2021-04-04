namespace Example.Api.Models.Api
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public class DataPostRequest
    {
        public int Id { get; set; }

        [AllowNull]
        public string Name { get; set; }

        public bool Flag { get; set; }

        public DateTime DateTime { get; set; }
    }
}
