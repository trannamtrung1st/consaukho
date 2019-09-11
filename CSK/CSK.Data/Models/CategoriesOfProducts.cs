using System;
using System.Collections.Generic;

namespace CSK.Data.Models
{
    public partial class CategoriesOfProducts
    {
        public string CategoryId { get; set; }
        public string ProductId { get; set; }

        public virtual ProductCategories Category { get; set; }
        public virtual Products Product { get; set; }
    }
}
