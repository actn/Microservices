using EventSourcing.API.DTO;
using MediatR;

namespace EventSourcing.API.Commands
{
    public class CreateProductCommand:IRequest
    {
        public CreateProductDto CreateProductDto { get; set; }
    }
}
