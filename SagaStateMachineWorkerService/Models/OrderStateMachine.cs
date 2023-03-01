using AutoMapper;
using MassTransit;
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

        public State OrderCreated { get; private set; }

        public OrderStateMachine(IMapper mapper)
        {
            _mapper = mapper;
            InstanceState(x => x.CurrentState);

            Event(()=> OrderCreatedRequestEvent, y=>y.CorrelateBy((x,z)=>x.OrderId==z.Message.OrderId).SelectId(c=>NewId.NextGuid()));

            Initially(When(OrderCreatedRequestEvent)
                .Then(context =>
                {

                    _mapper.Map(context.Message, context.Saga);
                    //context.Saga.BuyerId = context.Message.BuyerId;

                    //context.Saga.OrderId = context.Message.OrderId;
                    //context.Saga.CreatedDate = DateTime.Now;

                    //context.Saga.CardName = context.Message.Payment.CardName;
                    //context.Saga.CardNumber = context.Message.Payment.CardNumber;
                    //context.Saga.CVV = context.Message.Payment.CVV;
                    //context.Saga.Expiration = context.Message.Payment.Expiration;
                    //context.Saga.TotalPrice = context.Message.Payment.TotalPrice;
                    context.Saga.CreatedDate = DateTime.Now;

                }).Then(context => { Console.WriteLine($"OrderCreatedRequestEvent before : {context.Saga}"); })
                   .TransitionTo(OrderCreated)
                   .Then(context => { Console.WriteLine($"OrderCreatedRequestEvent after : {context.Saga}"); })
            );
        }
    }
}
