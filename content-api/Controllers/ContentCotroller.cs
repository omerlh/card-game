using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using static System.IO.File;
using ContentApi.Models;
using Microsoft.Extensions.Options;

namespace ContentApi.Controllers
{
    [Route("api/v1/[controller]")]
    [EnableCors("AllowAllOrigins")]
    public class ContentController : Controller
    {
        private readonly string baseDir;

        public ContentController(IOptions<ContentOptions> contentOptions)
        {
            baseDir = contentOptions.Value.BaseDirectory;   
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return Directory.GetDirectories(baseDir)
                .Where(folder => !folder.Contains(".git"))
                .Select(folder => folder.Split(Path.DirectorySeparatorChar).Last());
        }

        // GET api/values
        [HttpGet("{directoryName}")]
        public ActionResult Get(string directoryName)
        {
            var regex = new Regex("^[a-zA-Z1-9\\-].*?$");
            if (!regex.Match(directoryName).Success)
            {
                return BadRequest($"Invalid directory name {directoryName}");
            }
            return Ok(Directory.GetFiles(Path.Combine(baseDir, directoryName))
                .Select(folder => folder.Split(Path.DirectorySeparatorChar).Last()));
        }

        // GET api/values
        [HttpGet("{directoryName}/{fileName}")]
        public ActionResult Get(string directoryName, string fileName)
        {
            var regex = new Regex("^[a-zA-Z1-9\\-].*?$");
            if (!regex.Match(directoryName).Success || !regex.Match(fileName).Success)
            {
                return BadRequest($"Invalid directory name {directoryName}");
            }
            return File(ReadAllBytes(Path.Combine(baseDir, directoryName, fileName)), "image/png");
        }
    }
}
