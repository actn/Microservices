using EventSourcing.API.Commands;
using EventSourcing.API.EventStores;
using MediatR;

namespace EventSourcing.API.Handlers.Commands
{
    public class ChangeProductNameCommandHandler : IRequestHandler<ChangeProductNameCommand>
    {
        private readonly ProductStream stream;

        public ChangeProductNameCommandHandler(ProductStream stream)
        {
            this.stream = stream;
        }

        public async Task Handle(ChangeProductNameCommand request, CancellationToken cancellationToken)
        {
            stream.NameChanged(request.ChangeProductNameDto);
            await stream.SaveAsync();
        }
    }
}
