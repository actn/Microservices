using EventSourcing.API.DTO;
using EventSourcing.Shared.Events;

namespace EventSourcing.API.Profiles
{
    public class MappingProfiles:AutoMapper.Profile
    {
        public MappingProfiles()
        {
            CreateMap<CreateProductDto,ProductCreatedEvent>().ReverseMap();
            CreateMap<ChangeProductNameDto,ProductNameChangedEvent>().ForMember(x=>x.ChangedName,y=>y.MapFrom(z=>z.Name)).ReverseMap();
            CreateMap<ChangeProductPriceDto,ProductPriceChangedEvent>().ForMember(x=>x.ChangedPrice,y=>y.MapFrom(z=>z.Price)).ReverseMap();
        }
    }
}
