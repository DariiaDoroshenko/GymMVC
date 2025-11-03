using Microsoft.AspNetCore.Mvc;

namespace GymInfrastructure.Controllers
{
    public class ToDoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
