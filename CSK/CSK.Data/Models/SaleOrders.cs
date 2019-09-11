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
        public string CustomerId { get; set; }
        public double? FinalAmount { get; set; }
        public string Status { get; set; }
        public DateTime? OrderTime { get; set; }
        public DateTime? FinishedTime { get; set; }
        public DateTime? AcceptedTime { get; set; }
        public DateTime? CancledTime { get; set; }
        public string PaymentType { get; set; }
        public string ShipAddress { get; set; }

        public virtual Customers Customer { get; set; }
        public virtual ICollection<SaleOrderDetails> SaleOrderDetails { get; set; }
    }
}
