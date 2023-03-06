using AutoMapper;
using MassTransit;
using Shared;
using Shared.Events;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SagaStateMachineWorkerService.Models
{
    public class OrderStateMachine:MassTransitStateMachine<OrderStateInstance>
    {
        public IMapper _mapper;

        public Event<IOrderCreatedRequestEvent> OrderCreatedRequestEvent { get; set; }
        public Event<IStockReservedEvent> StockReservedEvent { get; set; }
        public Event<IPaymentCompletedEvent> PaymentCompletedEvent { get; set; }

        public State OrderCreated { get; private set; }
        public State StockReserved { get; private set; }
        public State PaymentCompleted { get; private set; }

        public OrderStateMachine(IMapper mapper)
        {
            _mapper = mapper;
            InstanceState(x => x.CurrentState);

            Event(()=> OrderCreatedRequestEvent, y=>y.CorrelateBy((x,z)=>x.OrderId==z.Message.OrderId).SelectId(c=>NewId.NextGuid()));

            Event(() => StockReservedEvent, y => y.CorrelateById(z => z.Message.CorrelationId));

            Event(() => PaymentCompletedEvent, y => y.CorrelateById(z => z.Message.CorrelationId));

            Initially(When(OrderCreatedRequestEvent)
                .Then(context =>
                {
                    _mapper.Map(context.Message, context.Saga);
                    context.Saga.CreatedDate = DateTime.Now;

                }).Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before : {context.Saga}"); })
                    .Publish(context=> new OrderCreatedEvent(context.Saga.CorrelationId) { OrderItems=context.Message.OrderItems})
                   .TransitionTo(OrderCreated)
                   .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after : {context.Saga}"); })
            );


            During(OrderCreated, When(StockReservedEvent)
                                 .TransitionTo(StockReserved)
                                 .Send(new Uri($"queue:{RabbitMQSettingsConst.PaymentStockReservedRequestQueueName}"), context => new StockReservedeRequestPayment(context.Saga.CorrelationId)
                                 {
                                     OrderItems = context.Message.OrderItems,
                                     Payment = _mapper.Map(context.Saga, new PaymentMessage()),
                                     BuyerId=context.Saga.BuyerId
                                 }).Then(context => { Console.WriteLine($"StockReservedEvent after : {context.Saga}"); })
            );

            During(StockReserved, When(PaymentCompletedEvent)
                                    .TransitionTo(PaymentCompleted)
                                    .Publish(context => new OrderRequestCompletedEvent() { OrderId = context.Saga.OrderId })
                                    .Then(context => { Console.WriteLine($"PaymentCompletedEvent After : {context.Saga}"); })
                                    .Finalize());
        }
    }
}
