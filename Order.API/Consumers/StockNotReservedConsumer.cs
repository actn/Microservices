using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class StockNotReservedConsumer:IConsumer<StockNotReserveEvent>
    {
        private readonly AppDbContext _db;
        private ILogger<StockNotReservedConsumer> _logger;

        public StockNotReservedConsumer(AppDbContext db, ILogger<StockNotReservedConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<StockNotReserveEvent> context)
        {
            var order = await _db.Orders.FindAsync(context.Message.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Fail;
                order.FailMessage = context.Message.Message;
                await _db.SaveChangesAsync();

                _logger.LogInformation($"Order (Id={context.Message.OrderId}) status changed: {order.Status}");
            }
            else
            {
                _logger.LogError($"Order (Id={context.Message.OrderId}) not found");
            }
        }
    }
}
