using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class Customers
    {
        public Customers()
        {
            SaleOrders = new HashSet<SaleOrders>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public virtual ICollection<SaleOrders> SaleOrders { get; set; }
    }
}
