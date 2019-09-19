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
using TNT.Core.Helpers.Cryptography;
using TNT.Core.Helpers.Data;
using TNT.Core.Helpers.General;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSK.Admin.Controllers
{
    [Route("api/sale-orders")]
    [ApiController]
    public class SaleOrdersController : BaseController
    {
        public SaleOrdersController(CSKContext context) : base(context)
        {
        }

        private object ToOrderResponse(SaleOrders order)
        {
            return new
            {
                id = order.Id,
                final_amount = order.FinalAmount,
                order_time = Helper.ToVNStyleDateString(order.OrderTime),
                accepted_time = Helper.ToVNStyleDateString(order.AcceptedTime),
                cancled_time = Helper.ToVNStyleDateString(order.CancledTime),
                finished_time = Helper.ToVNStyleDateString(order.FinishedTime),
                payment_type = order.PaymentType,
                details = order.SaleOrderDetails.Select(oD => new
                {
                    id = oD.Id,
                    product_name = oD.Product.Name,
                    product_id = oD.ProductId,
                    quantity = oD.Quantity,
                    total_amount = oD.TotalAmount,
                    final_amount = oD.FinalAmount
                }).ToList(),
                customer_name = order.CustomerName,
                customer_phone = order.CustomerPhone,
                customer_email = order.CustomerEmail,
                customer_address = order.CustomerAddress,
                receiver_name = order.ReceiverName,
                receiver_phone = order.ReceiverPhone,
                receiver_address = order.ReceiverAddress,
                ship_type = order.ShipType,
                ship_date = Helper.ToVNStyleDateString(order.ShipDate, false),
                ship_time = order.ShipTime,
                total_amount = order.TotalAmount,
                status = order.Status,
                message = order.Message,
                note = order.Note
            };
        }

        [Authorize]
        [HttpPatch("{id}/status")]
        public IActionResult ProcessOrderStatus(string id, string value)
        {
            try
            {
                var order = _context.SaleOrders.FirstOrDefault(o => o.Id == id);
                if (order == null)
                    return NotFound(new
                    {
                        message = "Không tìm thấy"
                    });
                var changed = false;
                if (value.Equals("Accepted"))
                {
                    changed = true;
                    order.AcceptedTime = DateTime.UtcNow;
                }
                else if (value.Equals("Finished"))
                {
                    changed = true;
                    order.FinishedTime = DateTime.UtcNow;
                }
                else if (value.Equals("Cancled"))
                {
                    changed = true;
                    order.CancledTime = DateTime.UtcNow;
                }
                if (changed)
                {
                    order.Status = value;
                    _context.SaveChanges();
                    return NoContent();
                }
                return BadRequest(new
                {
                    message = "Không hợp lệ"
                });
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

        [HttpPost("cart")]
        public IActionResult CheckCart(CartViewModel model)
        {
            try
            {
                var result = ValidateCart(model);
                if (result.Any())
                    return BadRequest(new
                    {
                        messages = result
                    });
                var order = CalculateOrder(model);
                return Ok(ToOrderResponse(order));
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
        public IActionResult Get([FromQuery]string[] fields,
            [FromQuery]string[] sorts,
            [FromQuery]int page = 1, [FromQuery]int limit = 50)
        {
            try
            {
                var ordQuery = _context.SaleOrders.AsQueryable();
                if (sorts != null && sorts.Length > 0)
                {
                    foreach (var s in sorts)
                    {
                        var isAsc = s[0] == 'a';
                        var sStr = s.Substring(1);
                        switch (sStr)
                        {
                            case "order_time":
                                if (isAsc)
                                    ordQuery = ordQuery.OrderBy(o => o.OrderTime);
                                else
                                    ordQuery = ordQuery.OrderByDescending(o => o.OrderTime);
                                break;
                        }
                    }
                }
                var ord = ordQuery.Skip((page - 1) * limit).Take(limit).ToList();
                if (fields == null || fields.Length == 0)
                    fields = new string[] { "info" };
                var list = new List<IDictionary<string, object>>();
                foreach (var o in ord)
                {
                    var obj = new Dictionary<string, object>();
                    list.Add(obj);

                    foreach (var f in fields)
                    {
                        switch (f)
                        {
                            case "info":
                                obj["accepted_time"] = Helper.ToVNStyleDateString(o.AcceptedTime);
                                obj["cancled_time"] = Helper.ToVNStyleDateString(o.CancledTime);
                                obj["final_amount"] = o.FinalAmount;
                                obj["id"] = o.Id;
                                obj["order_time"] = Helper.ToVNStyleDateString(o.OrderTime);
                                obj["finished_time"] = Helper.ToVNStyleDateString(o.FinishedTime);
                                obj["payment_type"] = o.PaymentType;
                                obj["status"] = o.Status;
                                obj["total_amount"] = o.TotalAmount;
                                break;
                            case "cinfo":
                                obj["customer_name"] = o.CustomerName;
                                obj["customer_phone"] = o.CustomerName;
                                obj["customer_email"] = o.CustomerName;
                                obj["customer_address"] = o.CustomerName;
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
                    data = e,
                });
            }
        }


        [HttpGet("count")]
        public IActionResult Count()
        {
            try
            {
                var count = _context.SaleOrders.Count();
                return Ok(count);
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


        [HttpGet("{id}")]
        public IActionResult GetById(string id)
        {
            try
            {
                var ord = _context.SaleOrders.FirstOrDefault(o => o.Id == id);
                if (ord == null)
                    return NotFound(new
                    {
                        message = "Không tìm thấy"
                    });

                return Ok(ToOrderResponse(ord));
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

        [HttpPost("")]
        public IActionResult Create(CreateSaleOrderViewModel model)
        {
            try
            {
                var result = ValidateCreate(model);
                if (result.Any())
                    return BadRequest(new
                    {
                        messages = result
                    });
                var order = CalculateOrder(model);

                order.CustomerAddress = model.customer_address;
                order.CustomerEmail = model.customer_email;
                order.CustomerName = model.customer_name;
                order.CustomerPhone = model.customer_phone;
                order.ReceiverAddress = model.receiver_address;
                order.ReceiverName = model.receiver_name;
                order.ReceiverPhone = model.receiver_phone;
                order.ShipType = model.ship_type;
                order.ShipDate = model.ship_date;
                order.ShipTime = model.ship_time;
                order.PaymentType = model.payment_type;
                order.Message = model.message;
                order.Note = model.note;

                _context.SaleOrders.Add(order);
                _context.SaveChanges();

                var mailResult = Gmail.SendEmail("Có đơn hàng mới",
                    $"<p><b>{order.CustomerName}</b> đã đặt 1 đơn hàng</p>" +
                    $"<p>SĐT: <b>{order.CustomerPhone}</b></p>" +
                    $"<p>Email: <b>{order.CustomerEmail}</b></p>" +
                    $"<p><a href='{App.Instance.BaseAddress}admin/order/{order.Id}'>Xem chi tiết</a></p>",
                    App.Instance.OwnerMails);

                return Ok(ToOrderResponse(order));
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
        private static readonly Random _rand = new Random();
        private SaleOrders CalculateOrder(CartViewModel model)
        {
            var order = new SaleOrders();
            order.AcceptedTime = null;
            order.CancledTime = null;

            order.Id = _rand.RandomStringFrom(RandomExtension.Uppers_Digits, 10);
            order.OrderTime = DateTime.UtcNow;
            order.FinishedTime = null;
            order.Status = "New";

            var details = new List<SaleOrderDetails>();
            foreach (var kv in model.details)
            {
                var d = kv.Value;
                var pro = _context.Products.FirstOrDefault(p => p.Id == d.product_id && p.Active == true);
                var oD = new SaleOrderDetails();
                oD.Id = Guid.NewGuid().ToString();
                oD.OrderId = order.Id;
                oD.ProductId = pro.Id;
                oD.Quantity = d.quantity;
                oD.TotalAmount = d.quantity * pro.UnitPrice;
                if (pro.DiscountAmount != null)
                    oD.FinalAmount = oD.TotalAmount - pro.DiscountAmount;
                else if (pro.DiscountPercent != null)
                    oD.FinalAmount = oD.TotalAmount - (oD.TotalAmount * pro.DiscountPercent / 100.0);
                else oD.FinalAmount = oD.TotalAmount;
                details.Add(oD);
            }
            order.SaleOrderDetails = details;

            order.TotalAmount = details.Sum(oD => oD.TotalAmount);
            order.FinalAmount = details.Sum(oD => oD.FinalAmount);
            return order;
        }

        private IEnumerable<string> ValidateCart(CartViewModel model)
        {
            var listMess = new List<string>();

            if (!model.details.Any())
                listMess.Add("Không có hàng trong giỏ");
            else
            {
                foreach (var kv in model.details)
                {
                    var d = kv.Value;
                    var pro = _context.Products.FirstOrDefault(p => p.Active == true && p.Id == d.product_id);
                    if (pro == null)
                        listMess.Add("Một vài sản phẩm đã bị gỡ bỏ");
                    else
                    {
                        if (d.quantity > pro.InStockAmount)
                            listMess.Add($"Không đủ số lượng sẵn có của {pro.Name}");
                    }
                }
            }

            return listMess;
        }

        private IEnumerable<string> ValidateCreate(CreateSaleOrderViewModel model)
        {
            var listMess = new List<string>();
            if (string.IsNullOrWhiteSpace(model.customer_name))
                listMess.Add("Thiếu tên người mua");

            if (string.IsNullOrWhiteSpace(model.customer_phone) &&
                string.IsNullOrWhiteSpace(model.customer_email))
                listMess.Add("Vui lòng để lại thông tin liên lạc");

            if ((!model.payment_type.Equals("cod") && !model.payment_type.Equals("transfer"))
                || (string.IsNullOrWhiteSpace(model.ship_type))
                || model.ship_date == null || string.IsNullOrWhiteSpace(model.ship_time))
                listMess.Add("Có lỗi xảy ra");

            var checkCart = ValidateCart(model);
            listMess.AddRange(checkCart);
            return listMess;
        }

    }

    public class NewSaleOrderDetailViewModel
    {
        public string product_id { get; set; }
        public int? quantity { get; set; }
    }

    public class CartViewModel
    {
        public IDictionary<string, NewSaleOrderDetailViewModel> details { get; set; }
    }

    public class CreateSaleOrderViewModel : CartViewModel
    {
        public string customer_name { get; set; }
        public string customer_phone { get; set; }
        public string customer_email { get; set; }
        public string customer_address { get; set; }
        public string receiver_name { get; set; }
        public string receiver_phone { get; set; }
        public string receiver_address { get; set; }
        public string ship_type { get; set; }
        public DateTime? ship_date { get; set; }
        public string ship_time { get; set; }
        public string payment_type { get; set; }
        public string message { get; set; }
        public string note { get; set; }

    }


}
