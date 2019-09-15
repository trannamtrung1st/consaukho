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
    [Route("api/brand-info")]
    [ApiController]
    public class BrandInfoController : BaseController
    {
        public BrandInfoController(CSKContext context) : base(context)
        {
        }

        [HttpGet("")]
        public IActionResult Get(
            [FromQuery]string[] fields)
        {
            try
            {
                var brand = _context.BrandInfo.FirstOrDefault();
                if (brand == null)
                {
                    brand = _context.BrandInfo.Add(new BrandInfo
                    {
                        Id = Guid.NewGuid().ToString()
                    }).Entity;
                    _context.SaveChanges();
                }
                var obj = new Dictionary<string, object>();

                foreach (var f in fields)
                {
                    switch (f)
                    {
                        case "layout":
                            if (brand.HomePageLayout != null)
                            {
                                try
                                {
                                    obj["layout"] = JsonConvert.DeserializeObject(brand.HomePageLayout);
                                }
                                catch (Exception e)
                                {
                                }
                            }
                            if (brand.HomePageLayoutBackup != null)
                            {
                                try
                                {
                                    obj["layout_backup"] = JsonConvert.DeserializeObject(brand.HomePageLayoutBackup);
                                }
                                catch (Exception e)
                                {
                                }
                            }
                            break;
                    }
                }
                return Ok(obj);
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
        [HttpPut("layout")]
        public IActionResult Update(IDictionary<string, string> data)
        {
            try
            {
                var brand = _context.BrandInfo.FirstOrDefault();
                brand.HomePageLayoutBackup = brand.HomePageLayout;
                brand.HomePageLayout = data["layout"];
                _context.SaveChanges();
                return Ok();
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

}
