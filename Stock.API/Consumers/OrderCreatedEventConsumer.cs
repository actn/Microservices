using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<IOrderCreatedEvent>
    {
        private readonly AppDbContext _db;
        private ILogger<OrderCreatedEventConsumer> _logger;
        private readonly ISendEndpointProvider sendEndpointProvider;
        private readonly IPublishEndpoint publishEndpoint;

        public OrderCreatedEventConsumer(AppDbContext db, ILogger<OrderCreatedEventConsumer> logger, ISendEndpointProvider sendEndpointProvider, IPublishEndpoint publishEndpoint)
        {
            _db = db;
            _logger = logger;
            this.sendEndpointProvider = sendEndpointProvider;
            this.publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<IOrderCreatedEvent> context)
        {
            var stokResult = new List<bool>();

            foreach (var item in context.Message.OrderItems)
            {
                stokResult.Add(await _db.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }
            if (stokResult.All(x => x.Equals(true)))
            {
                foreach (var item in context.Message.OrderItems)
                {
                    var stock = await _db.Stocks.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                    if (stock != null)
                        stock.Count -= item.Count;

                    await _db.SaveChangesAsync();
                }

                _logger.LogInformation($"Stock was reserved for CorrelationId Id: {context.Message.CorrelationId} ");

                var sendEnpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

                StockReserveEvent stockReserveEvent = new StockReserveEvent(context.Message.CorrelationId)
                {
                    OrderItems = context.Message.OrderItems

                };

                await publishEndpoint.Publish(stockReserveEvent);
            }
            else
            {
                await publishEndpoint.Publish(new StockNotReserveEvent(context.Message.CorrelationId)
                {
                    Reason = "Not enough stock"
                });
                _logger.LogInformation($"Stock not enough for CorrelationId Id: {context.Message.CorrelationId} ");
            }
        }
    }
}
