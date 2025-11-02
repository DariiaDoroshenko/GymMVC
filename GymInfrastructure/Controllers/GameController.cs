using Microsoft.AspNetCore.Mvc;

namespace GymInfrastructure.Controllers
{
    public class GameController : Controller
    {
        // GET: /Game/Canvas
        public IActionResult Canvas() => View();
    }
}
