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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
            if (!System.IO.File.Exists(localFile))
                return NotFound();
            var extension = localFile.Substring(localFile.LastIndexOf('.') + 1);
            return File(new FileStream(localFile, FileMode.Open)
                , MimeTypeMap.GetMimeType(extension));
        }

        [Authorize]
        [HttpPost("")]
        public IActionResult Create(string path, IFormFile file)
        {
            path = App.Instance.DataPath + path;
            using (var stream = new FileStream(path + "/" + file.FileName, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            return Ok();
        }

        [Authorize]
        [HttpGet("list")]
        public IActionResult List()
        {
            var dataPath = App.Instance.DataPath;
            var system = List(dataPath);
            return Ok(system);
        }

        [Authorize]
        [HttpPost("folder")]
        public IActionResult CreateFolder(string path)
        {
            path = App.Instance.DataPath + '/' + path;

            if (System.IO.Directory.Exists(path))
                return BadRequest(new
                {
                    message = "Đã tồn tại"
                });
            System.IO.Directory.CreateDirectory(path);
            return Ok();
        }

        [Authorize]
        [HttpDelete("folder")]
        public IActionResult DeleteFolder(string path)
        {
            path = App.Instance.DataPath + '/' + path;
            System.IO.Directory.Delete(path, true);
            return Ok();
        }


        [Authorize]
        [HttpDelete("")]
        public IActionResult DeleteFile(string path)
        {
            path = App.Instance.DataPath + '/' + path;
            System.IO.File.Delete(path);
            return Ok();
        }

        [Authorize]
        [HttpPatch("folder/name")]
        public IActionResult RenameFolder(string old_path, string new_name)
        {
            var path = App.Instance.DataPath + '/' + old_path;
            var fInfo = new DirectoryInfo(path);
            if (System.IO.Directory.Exists(fInfo.Parent.FullName + "/" + new_name))
                return BadRequest(new
                {
                    message = "Đã tồn tại"
                });
            System.IO.Directory.Move(path, fInfo.Parent.FullName + "/" + new_name);

            return Ok();
        }

        [Authorize]
        [HttpPatch("name")]
        public IActionResult RenameFile(string old_path, string new_name)
        {
            var path = App.Instance.DataPath + '/' + old_path;
            var fInfo = new FileInfo(path);
            if (System.IO.File.Exists(fInfo.DirectoryName + "/" + new_name))
                return BadRequest(new
                {
                    message = "Đã tồn tại"
                });
            System.IO.File.Move(path, fInfo.DirectoryName + "/" + new_name);
            return Ok();
        }

        private object List(string dataPath)
        {
            var rInfo = new DirectoryInfo(dataPath);
            var folders = System.IO.Directory.GetDirectories(dataPath);
            var files = System.IO.Directory.GetFiles(dataPath);

            var list = new List<object>();

            foreach (var fd in folders)
            {
                var f = fd.Replace('\\', '/');
                list.Add(List(f));
            }

            foreach (var fi in files)
            {
                var f = fi.Replace('\\', '/');
                var fInfo = new FileInfo(f);
                list.Add(new
                {
                    name = fInfo.Name,
                    type = "file",
                    path = f,
                    size = fInfo.Length,
                    extension = fInfo.Extension,
                });
            }

            return new
            {
                name = rInfo.Name,
                type = "folder",
                path = dataPath,
                items = list
            };
        }

    }

}
