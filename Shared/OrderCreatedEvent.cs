using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared
{
    public class OrderCreatedEvent
    {
        public int OrderId { get; set; }
        public string BuyerId { get; set; }
        public PaymentMessage paymentMessage { get; set; }
        public List<OrderItemMessage> orderMessage { get; set; } = new List<OrderItemMessage>();
    }
}
