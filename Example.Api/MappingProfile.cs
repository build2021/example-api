namespace Example.Api
{
    using AutoMapper;

    using Example.Api.Models.Api;
    using Example.Api.Models.Entity;

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<DataEntity, DataGetResponse>();
            CreateMap<DataPostRequest, DataEntity>();
        }
    }
}
