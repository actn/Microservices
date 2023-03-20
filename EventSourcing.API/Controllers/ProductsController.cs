using EventSourcing.API.Commands;
using EventSourcing.API.DTO;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventSourcing.API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator mediator;

        public ProductsController(IMediator mediator)
        {
            this.mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductDto createProductDto,CancellationToken cancellationToken)
        {
            await mediator.Send(new CreateProductCommand() { CreateProductDto = createProductDto },cancellationToken);
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> ChangeName(ChangeProductNameDto changeProductNameDto, CancellationToken cancellationToken)
        {
            await mediator.Send(new ChangeProductNameCommand() { ChangeProductNameDto = changeProductNameDto }, cancellationToken);
            return NoContent();
        }

        [HttpPut]
        public async Task<IActionResult> ChangePrice(ChangeProductPriceDto changeProductPriceDto, CancellationToken cancellationToken)
        {
            await mediator.Send(new ChangeProductPriceCommand() { ChangeProductPriceDto = changeProductPriceDto }, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id,CancellationToken cancellationToken)
        {
            await mediator.Send(new DeleteProductCommand() { Id=id} , cancellationToken);
            return NoContent();
        }
    }
}
