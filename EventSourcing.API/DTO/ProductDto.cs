namespace EventSourcing.API.DTO
{
    public class ProductDto:CreateProductDto
    {
        public Guid Id { get; set; }
    }
}
