using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class SaleOrderDetails
    {
        public string Id { get; set; }
        public string OrderId { get; set; }
        public string ProductId { get; set; }
        public int? Quantity { get; set; }
        public double? TotalAmount { get; set; }
        public double? FinalAmount { get; set; }

        public virtual SaleOrders Order { get; set; }
        public virtual Products Product { get; set; }
    }
}
