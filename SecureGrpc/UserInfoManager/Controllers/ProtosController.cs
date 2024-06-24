using Microsoft.AspNetCore.Mvc;

namespace UserInfoManager.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProtosController(IWebHostEnvironment webHost) : ControllerBase
    {
        private readonly string _baseDirectory = webHost.ContentRootPath;

        [HttpGet("")]
        public ActionResult GetAll()
        {
            return Ok(Directory.GetFiles($"{_baseDirectory}/Protos").Select(Path.GetFileName));
        }

        [HttpGet("{protoName}")]
        public async Task<ActionResult> GetFileContent(string protoName)
        {
            var filePath = $"{_baseDirectory}/Protos/{protoName}";

            if (System.IO.File.Exists(filePath))
                return Content(await System.IO.File.ReadAllTextAsync(filePath));

            return NotFound();
        }
    }
}