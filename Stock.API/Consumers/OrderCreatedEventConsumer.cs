using MassTransit;
using Microsoft.EntityFrameworkCore;
using Shared;
using Stock.API.Models;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
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

        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            var stokResult = new List<bool>();

            foreach (var item in context.Message.orderMessage)
            {
                stokResult.Add(await _db.Stocks.AnyAsync(x => x.ProductId == item.ProductId && x.Count > item.Count));
            }
            if (stokResult.All(x=>x.Equals(true)))
            {
                foreach (var item in context.Message.orderMessage)
                {
                    var stock = await _db.Stocks.FirstOrDefaultAsync(p => p.ProductId == item.ProductId);
                    if (stock != null)
                        stock.Count -= item.Count;

                    await _db.SaveChangesAsync();
                }

                _logger.LogInformation($"Stock was reserved for buyer Id: {context.Message.BuyerId} ");

                var sendEnpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.StockReservedEventQueueName}"));

                StockReserveEvent stockReserveEvent = new StockReserveEvent()
                {
                    Payment = context.Message.paymentMessage,
                    BuyerId = context.Message.BuyerId,
                    OrderId =context.Message.OrderId,
                    OrderItems = context.Message.orderMessage
                    
                };

                await sendEnpoint.Send(stockReserveEvent);
            }
            else
            {
                await publishEndpoint.Publish(new StockNotReserveEvent()
                {
                    OrderId= context.Message.OrderId,
                    Message ="Not enough stock"
                });
                _logger.LogInformation($"Stock not enough for buyer Id: {context.Message.BuyerId} ");
            }

        }
    }
}
