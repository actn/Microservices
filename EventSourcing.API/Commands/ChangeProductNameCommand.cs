using EventSourcing.API.DTO;
using MediatR;
namespace EventSourcing.API.Commands
{
    public class ChangeProductNameCommand : IRequest
    {
        public ChangeProductNameDto ChangeProductNameDto {get; set;}
    }
}
