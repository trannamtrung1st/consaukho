using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CSK.Admin.Pages
{
    public class ProductsModel : PageModel
    {
        public string Search { get; set; }
        public string CateId { get; set; }
        public void OnGet(string search = "", string cate_id = "")
        {
            Search = search;
            CateId = cate_id;
        }
    }
}