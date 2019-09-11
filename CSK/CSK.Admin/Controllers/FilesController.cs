using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using CSK.Data;
using CSK.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CSK.Admin.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesController : ControllerBase
    {

        [HttpGet("")]
        public IActionResult Get(string path)
        {
            var localFile = App.Instance.DataPath + path;
            var extension = localFile.Substring(localFile.LastIndexOf('.') + 1);
            return File(new FileStream(localFile, FileMode.Open)
                , MimeTypeMap.GetMimeType(extension));
        }

    }

}
