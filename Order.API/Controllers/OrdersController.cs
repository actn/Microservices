using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;
using Shared.Events;
using Shared.Interfaces;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ISendEndpointProvider _sendEndpointProvider;

        public OrdersController(AppDbContext db, ISendEndpointProvider sendEnpointProvider)
        {
            _db = db;
            _sendEndpointProvider = sendEnpointProvider;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateDto orderCreate)
        {
            var newOrder = new Models.Order
            {
                BuyerId=orderCreate.BuyerId,
                Status=Models.OrderStatus.Suspend,
                Address= new Models.Address { Line=orderCreate.addresss.Line, District=orderCreate.addresss.District, Province= orderCreate.addresss.Province },
                CreatedDate =DateTime.Now,
                FailMessage=""
            };

            orderCreate.orderItems.ForEach(item =>
            {
                newOrder.Items.Add(new Models.OrderItem { Price = item.Price, ProductId = item.ProductId, Count = item.Count });
            });

            await _db.AddAsync(newOrder);

            await _db.SaveChangesAsync();

            OrderCreatedRequestEvent orderCreateRequestdEvent = new OrderCreatedRequestEvent()
            {
                BuyerId=orderCreate.BuyerId,
                OrderId=newOrder.Id,
                Payment = new PaymentMessage { CardName=orderCreate.payment.CardName, CardNumber=orderCreate.payment.CardNumber, 
                CVV=orderCreate.payment.CVV, Expiration=orderCreate.payment.Expiration, TotalPrice= orderCreate.orderItems.Sum(p=> p.Count*p.Price)}
            };

            orderCreate.orderItems.ForEach(item => {
                orderCreateRequestdEvent.OrderItems.Add(new OrderItemMessage { ProductId = item.ProductId, Count = item.Count });
            });

            var sendEndpoint = await _sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettingsConst.OrderSaga}"));
            await sendEndpoint.Send<IOrderCreatedRequestEvent>(orderCreateRequestdEvent);
            return Ok();
        }
    }
}
