using Microsoft.AspNetCore.Mvc;

namespace YourProject.Controllers
{
    public class SupportController : Controller
    {
        // /Support
        public IActionResult Index()
        {
            return View();
        }

        // /Support/Terms
        public IActionResult Terms()
        {
            return View();
        }

        // /Support/Privacy
        public IActionResult Privacy()
        {
            return View();
        }

        // /Support/Contact
        public IActionResult Contact()
        {
            return View();
        }
    }
}