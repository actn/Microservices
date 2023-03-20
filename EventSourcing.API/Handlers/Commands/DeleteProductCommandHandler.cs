using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand>
    {
        private readonly ProductStream stream;

        public DeleteProductCommandHandler(ProductStream stream)
        {
            this.stream = stream;
        }

        public async Task Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            stream.Deleted(request.Id);
            await stream.SaveAsync();
        }
    }
}
