using Microsoft.AspNetCore.Mvc;

namespace BoardCreate.Controllers
{
    public class MessageController : Controller
    {
        public IActionResult ShowMessage(string msg, string url)
        {
            ViewBag.Message = msg;
            ViewBag.RedirectUrl = url;
            return View("Message"); // 폴더랑 컨트롤러랑 이름이 같으면 컴터가 알아서 찾음
        }
    }
}
