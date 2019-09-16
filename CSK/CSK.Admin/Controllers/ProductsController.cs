using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using CSK.Data;
using CSK.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using TNT.Core.Helpers.Data;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSK.Admin.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : BaseController
    {
        public ProductsController(CSKContext context) : base(context)
        {
        }

        [Authorize]
        [HttpPost("")]
        public IActionResult Create([FromForm]CreateProductViewModel model)
        {
            try
            {
                var id = Guid.NewGuid().ToString();
                var categories = new List<CategoriesOfProducts>();
                if (model.categories_id != null)
                    foreach (var c in model.categories_id)
                        categories.Add(new CategoriesOfProducts()
                        {
                            CategoryId = c,
                            ProductId = id
                        });

                var urls = new List<string>();
                if (model.image_urls != null)
                    foreach (var l in model.image_urls.Split('\n'))
                        if (!string.IsNullOrWhiteSpace(l))
                            urls.Add(l);
                var pro = new Products()
                {
                    Active = true,
                    Code = model.code,
                    Description = model.description,
                    Id = id,
                    ImageUrls = JsonConvert.SerializeObject(urls),
                    InStockAmount = model.in_stock_amount,
                    IsInStockAmountVisible = model.is_in_stock_amount_visible == true ? true : false,
                    IsVisible = model.is_visible == true ? true : false,
                    Name = model.name,
                    UnitName = model.unit_name,
                    UnitPrice = model.unit_price,
                    DiscountAmount = model.discount_amount,
                    DiscountPercent = model.discount_percent,
                    CategoriesOfProducts = categories,
                    CreatedDate = DateTime.UtcNow
                };
                _context.Products.Add(pro);
                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {

                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e
                });
            }
        }

        [Authorize]
        [HttpPost("{id}")]
        public IActionResult Edit(string id,
            [FromForm]EditProductViewModel model)
        {
            try
            {
                var entity = _context.Products.FirstOrDefault(p => p.Id == id && p.Active == true);
                if (entity == null)
                    return NotFound(new
                    {
                        message = "Không tìm thấy"
                    });

                var categories = new List<CategoriesOfProducts>();
                if (model.categories_id != null)
                    foreach (var c in model.categories_id)
                        categories.Add(new CategoriesOfProducts()
                        {
                            CategoryId = c,
                            ProductId = id
                        });

                var urls = new List<string>();
                if (model.image_urls != null)
                    foreach (var l in model.image_urls.Split('\n'))
                        if (!string.IsNullOrWhiteSpace(l))
                            urls.Add(l);

                entity.Code = model.code;
                entity.Description = model.description;
                entity.ImageUrls = JsonConvert.SerializeObject(urls);
                entity.InStockAmount = model.in_stock_amount;
                entity.IsInStockAmountVisible = model.is_in_stock_amount_visible == true ? true : false;
                entity.IsVisible = model.is_visible == true ? true : false;
                entity.Name = model.name;
                entity.UnitName = model.unit_name;
                entity.UnitPrice = model.unit_price;
                entity.DiscountAmount = model.discount_amount;
                entity.DiscountPercent = model.discount_percent;

                entity.CategoriesOfProducts.Clear();
                entity.CategoriesOfProducts = categories;

                _context.SaveChanges();
                return NoContent();
            }
            catch (Exception e)
            {

                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e,
                });
            }
        }

        [HttpGet("")]
        public IActionResult Get([FromQuery]string[] fields, bool? visible,
            bool? popular,
            [FromQuery]string[] ids,
            [FromQuery]string[] sorts,
            string cate_id,
            double? from_price,
            double? to_price,
            string search,
            int page = 0, int limit = 0)
        {
            try
            {
                var query = _context.Products.Where(p => p.Active == true);
                if (ids != null && ids.Length > 0)
                    query = query.Where(p => ids.Contains(p.Id));
                if (popular == true)
                    query = query.OrderByDescending(p => p.SaleOrderDetails.Count);
                if (visible == true)
                    query = query.Where(p => p.IsVisible == true);
                if (limit > 0)
                    query = query.Skip(page * limit).Take(limit);
                if (sorts != null)
                    foreach (var s in sorts)
                    {
                        var isAsc = s[0] == 'a';
                        var field = s.Substring(1);
                        switch (field)
                        {
                            case "name":
                                if (isAsc)
                                    query = query.OrderBy(p => p.Name);
                                else
                                    query = query.OrderByDescending(p => p.Name);
                                break;
                            case "price":
                                if (isAsc)
                                    query = query.OrderBy(p => p.UnitPrice);
                                else
                                    query = query.OrderByDescending(p => p.UnitPrice);
                                break;
                        }
                    }
                if (cate_id != null)
                    query = query.Where(p => p.CategoriesOfProducts.Any(cp => cp.CategoryId == cate_id));
                if (from_price != null)
                    query = query.Where(p => p.UnitPrice >= from_price);
                if (to_price != null)
                    query = query.Where(p => p.UnitPrice <= to_price);
                if (search != null)
                {
                    query = query.Where(p => p.Name.Contains(search) ||
                        p.CategoriesOfProducts.Any(pC => pC.Category.Name.Contains(search)));
                }
                query = query.OrderByDescending(p => p.CreatedDate);

                var pros = query.ToList();
                if (fields == null || fields.Length == 0)
                    fields = new string[] { "info" };
                var list = new List<IDictionary<string, object>>();
                foreach (var p in pros)
                {
                    var obj = new Dictionary<string, object>();
                    list.Add(obj);

                    foreach (var f in fields)
                    {
                        switch (f)
                        {
                            case "info":
                                obj["active"] = p.Active;
                                obj["code"] = p.Code;
                                obj["description"] = p.Description;
                                obj["id"] = p.Id;
                                obj["image_urls"] = p.ImageUrls != null ?
                                    JsonConvert.DeserializeObject<List<string>>(p.ImageUrls) : null;
                                obj["in_stock_amount"] = p.InStockAmount;
                                obj["is_in_stock_amount_visible"] = p.IsInStockAmountVisible;
                                obj["is_available"] = p.InStockAmount > 0;
                                obj["is_visible"] = p.IsVisible;
                                obj["name"] = p.Name;
                                obj["unit_name"] = p.UnitName;
                                obj["unit_price"] = p.UnitPrice;
                                obj["discount_amount"] = p.DiscountAmount;
                                obj["discount_percent"] = p.DiscountPercent;
                                obj["created_date"] = Helper.ToVNStyleDateString(p.CreatedDate);
                                break;
                            case "cinfo":
                                obj["cinfo"] = p.CategoriesOfProducts.Select(c => new
                                {
                                    id = c.CategoryId,
                                    name = c.Category.Name
                                }).ToList();
                                break;
                        }
                    }

                }

                return Ok(list);
            }
            catch (Exception e)
            {
                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e
                });
            }

        }

        [HttpGet("{id}")]
        public IActionResult GetById(string id, bool? related)
        {
            try
            {
                var pro = _context.Products
                    .FirstOrDefault(p => p.Id == id && p.Active == true);

                if (pro == null)
                    return NotFound(new
                    {
                        message = "Không tìm thấy"
                    });

                dynamic relatedPro = null;
                if (related == true)
                {
                    var cates = pro.CategoriesOfProducts.Select(c => c.CategoryId);
                    relatedPro = _context.Products.Where(p =>
                        p.Active == true && p.Id != pro.Id &&
                        p.CategoriesOfProducts.Any(pC => cates.Contains(pC.CategoryId)))
                            .Select(p => new
                            {
                                active = p.Active,
                                code = p.Code,
                                description = p.Description,
                                id = p.Id,
                                image_urls = p.ImageUrls != null ?
                                    JsonConvert.DeserializeObject<IEnumerable<string>>(p.ImageUrls) : null,
                                in_stock_amount = p.InStockAmount,
                                is_in_stock_amount_visible = p.IsInStockAmountVisible,
                                is_available = p.InStockAmount > 0,
                                is_visible = p.IsVisible,
                                name = p.Name,
                                unit_name = p.UnitName,
                                unit_price = p.UnitPrice,
                                discount_amount = p.DiscountAmount,
                                discount_percent = p.DiscountPercent,
                                created_date = Helper.ToVNStyleDateString(p.CreatedDate)
                            }).ToList();
                }

                return Ok(new
                {
                    active = pro.Active,
                    cinfo = pro.CategoriesOfProducts.Select(c => new
                    {
                        id = c.CategoryId,
                        name = c.Category.Name
                    }).ToList(),
                    code = pro.Code,
                    description = pro.Description,
                    id = pro.Id,
                    image_urls = pro.ImageUrls != null ?
                        JsonConvert.DeserializeObject<IEnumerable<string>>(pro.ImageUrls) : null,
                    in_stock_amount = pro.InStockAmount,
                    is_in_stock_amount_visible = pro.IsInStockAmountVisible,
                    is_available = pro.InStockAmount > 0,
                    is_visible = pro.IsVisible,
                    name = pro.Name,
                    unit_name = pro.UnitName,
                    unit_price = pro.UnitPrice,
                    discount_amount = pro.DiscountAmount,
                    discount_percent = pro.DiscountPercent,
                    created_date = Helper.ToVNStyleDateString(pro.CreatedDate),
                    related = relatedPro
                });
            }
            catch (Exception e)
            {
                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e
                });
            }

        }

        [Authorize]
        [HttpDelete("{id}")]
        public IActionResult Delete(string id)
        {
            try
            {
                var cate = _context.Products.FirstOrDefault(c => c.Id == id);
                if (cate == null)
                    return NotFound(new { message = "Không tìm thấy" });

                cate.Active = false;
                _context.SaveChanges();

                return NoContent();
            }
            catch (Exception e)
            {
                return Error(new
                {
                    message = "Có lỗi xảy ra. Vui lòng thử lại.",
                    data = e
                });
            }
        }
    }

    public class CreateProductViewModel
    {
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public int? in_stock_amount { get; set; }
        public bool? is_in_stock_amount_visible { get; set; }
        public string unit_name { get; set; }
        public double? unit_price { get; set; }
        public double? discount_amount { get; set; }
        public double? discount_percent { get; set; }
        public string image_urls { get; set; }
        public bool? is_visible { get; set; }
        public IEnumerable<string> categories_id { get; set; }

    }


    public class EditProductViewModel
    {
        public string name { get; set; }
        public string code { get; set; }
        public string description { get; set; }
        public int? in_stock_amount { get; set; }
        public bool? is_in_stock_amount_visible { get; set; }
        public string unit_name { get; set; }
        public double? unit_price { get; set; }
        public double? discount_amount { get; set; }
        public double? discount_percent { get; set; }
        public string image_urls { get; set; }
        public bool? is_visible { get; set; }
        public IEnumerable<string> categories_id { get; set; }
    }

}
