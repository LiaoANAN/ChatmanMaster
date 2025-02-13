using Microsoft.AspNetCore.Mvc;

namespace Chatman.Controllers
{
    public class ChatController : WebController
    {
        public IActionResult PrivateRoom()
        {
            return View();
        }
    }
}
