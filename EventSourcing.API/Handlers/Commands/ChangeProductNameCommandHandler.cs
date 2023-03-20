using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class ChangeProductPriceCommandHandler : IRequestHandler<ChangeProductPriceCommand>
    {
        private readonly ProductStream stream;

        public ChangeProductPriceCommandHandler(ProductStream stream)
        {
            this.stream = stream;
        }

        public async Task Handle(ChangeProductPriceCommand request, CancellationToken cancellationToken)
        {
            stream.PriceChanged(request.ChangeProductPriceDto);
            await stream.SaveAsync();
        }
    }
}
