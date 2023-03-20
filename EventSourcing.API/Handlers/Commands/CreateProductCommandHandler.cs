using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand>
    {
        private readonly ProductStream stream;

        public CreateProductCommandHandler(ProductStream stream)
        {
            this.stream = stream;
        }

        public async Task Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            stream.Created(request.CreateProductDto);
            await stream.SaveAsync();
        }
    }
}
