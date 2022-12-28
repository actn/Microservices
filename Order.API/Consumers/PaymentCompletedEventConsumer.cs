using MassTransit;
using Order.API.Models;
using Shared;

namespace Order.API.Consumers
{
    public class PaymentCompletedEventConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly AppDbContext _db;
        private ILogger<PaymentCompletedEventConsumer> _logger;

        public PaymentCompletedEventConsumer(AppDbContext db, ILogger<PaymentCompletedEventConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var order = await _db.Orders.FindAsync(context.Message.OrderId);
            if (order != null) {
                order.Status = OrderStatus.Complete;
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
