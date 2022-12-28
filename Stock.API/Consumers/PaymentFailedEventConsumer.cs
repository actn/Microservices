using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class PaymentFailedEventConsumer : IConsumer<PaymentFailedEvent>
    {
        private readonly AppDbContext _db;
        private ILogger<PaymentFailedEventConsumer> _logger;

        public PaymentFailedEventConsumer(AppDbContext db, ILogger<PaymentFailedEventConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
        {
            foreach (var item in context.Message.OrderItems)
            {
                var stock = await _db.Stocks.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                if (stock != null)
                    stock.Count += item.Count;

                await _db.SaveChangesAsync();
            }

            _logger.LogInformation($"Stock reservation was released for buyer Id: {context.Message.BuyerId} ");
        }
    }
}
