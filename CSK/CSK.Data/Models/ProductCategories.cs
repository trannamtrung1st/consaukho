using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class ProductCategories
    {
        public ProductCategories()
        {
            CategoriesOfProducts = new HashSet<CategoriesOfProducts>();
        }

        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool? Active { get; set; }
        public string ImageUrl { get; set; }

        public virtual ICollection<CategoriesOfProducts> CategoriesOfProducts { get; set; }
    }
}
