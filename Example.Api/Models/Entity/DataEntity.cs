namespace Example.Api.Models.Entity
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using Smart.Data.Mapper.Attributes;

    public class DataEntity
    {
        [PrimaryKey]
        public int Id { get; set; }

        [AllowNull]
        public string Name { get; set; }

        public bool Flag { get; set; }

        public DateTime DateTime { get; set; }
    }
}
