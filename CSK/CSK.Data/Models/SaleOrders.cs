using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class SaleOrders
    {
        public SaleOrders()
        {
            SaleOrderDetails = new HashSet<SaleOrderDetails>();
        }

        public string Id { get; set; }
        public double? TotalAmount { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhone { get; set; }
        public string ReceiverAddress { get; set; }
        public string ShipType { get; set; }
        public DateTime? ShipDate { get; set; }
        public string ShipTime { get; set; }
        public string PaymentType { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }
        public double? FinalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? OrderTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? CancledTime { get; set; }

        public virtual ICollection<SaleOrderDetails> SaleOrderDetails { get; set; }
    }
}
