using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class Products
    {
        public Products()
        {
            CategoriesOfProducts = new HashSet<CategoriesOfProducts>();
            SaleOrderDetails = new HashSet<SaleOrderDetails>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public int? InStockAmount { get; set; }
        public bool? IsInStockAmountVisible { get; set; }
        public string UnitName { get; set; }
        public double? UnitPrice { get; set; }
        public string ImageUrls { get; set; }
        public bool? IsVisible { get; set; }
        public double? DiscountAmount { get; set; }
        public double? DiscountPercent { get; set; }
        public bool? Active { get; set; }

        public virtual ICollection<CategoriesOfProducts> CategoriesOfProducts { get; set; }
        public virtual ICollection<SaleOrderDetails> SaleOrderDetails { get; set; }
    }
}
