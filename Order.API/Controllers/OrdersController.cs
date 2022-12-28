using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.API.DTOs;
using Order.API.Models;
using Shared;

namespace Order.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPublishEndpoint _pubEP;

        public OrdersController(AppDbContext db, IPublishEndpoint publishEnpoint)
        {
            _db = db;
            _pubEP = publishEnpoint;
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

            OrderCreatedEvent orderCreatedEvent = new OrderCreatedEvent()
            {
                BuyerId=orderCreate.BuyerId,
                OrderId=newOrder.Id,
                paymentMessage = new PaymentMessage { CardName=orderCreate.payment.CardName, CardNumber=orderCreate.payment.CardNumber, 
                CVV=orderCreate.payment.CVV, Expiration=orderCreate.payment.Expiration, TotalPrice= orderCreate.orderItems.Sum(p=> p.Count*p.Price)}
            };

            orderCreate.orderItems.ForEach(item => {
                orderCreatedEvent.orderMessage.Add(new OrderItemMessage { ProductId = item.ProductId, Count = item.Count });
            });
            await _pubEP.Publish(orderCreatedEvent);
            return Ok();
        }
    }
}
