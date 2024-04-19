using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.IO;

namespace BackEnd.Controllers
{
    public class FallbackController : Controller
    {
        public IActionResult Index()
        {
            return PhysicalFile(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html"), MediaTypeNames.Text.Html);
        }
    }
}
