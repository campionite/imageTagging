using System.Diagnostics;
using QvcImageTagger.Predict.Models;
using Microsoft.AspNetCore.Mvc;

namespace QvcImageTagger.Predict.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Index()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
