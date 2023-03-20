using AutoMapper;
using EventSourcing.API.DTO;
using EventSourcing.Shared.Events;
using EventStore.ClientAPI;

namespace EventSourcing.API.EventStores
{
    public class ProductStream : AbstractStream
    {
        public static string StreamName => "ProductStream";
        private readonly IMapper _mapper;
        public ProductStream(IMapper mapper,IEventStoreConnection connection) : base(StreamName, connection)
        {
            _mapper = mapper;
        }


        public void Created(CreateProductDto createProductDto)
        {
            var createEvent = _mapper.Map<ProductCreatedEvent>(createProductDto);
            createEvent.Id=Guid.NewGuid();
            Events.AddLast(createEvent);

        }

        public void NameChanged(ChangeProductNameDto changeProductNameDto)
        {
            Events.AddLast(_mapper.Map<ProductNameChangedEvent>(changeProductNameDto));
        }

        public void PriceChanged(ChangeProductPriceDto changeProductPriceDto)
        {
            Events.AddLast(_mapper.Map<ProductPriceChangedEvent>(changeProductPriceDto));
        }

        public void Deleted(Guid id)
        {
            Events.AddLast(new ProductDeletedEvent() {Id=id });
        }

    }
}
