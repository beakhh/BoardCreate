//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.SignalR;
//using System.Threading.Tasks;

//[Route("Notification")]
//[ApiController]
//public class NotificationController : ControllerBase
//{
//    private readonly IHubContext<NotificationHub> _hubContext;

//    public NotificationController(IHubContext<NotificationHub> hubContext)
//    {
//        _hubContext = hubContext;
//    }

//    // ✅ 댓글이 달릴 때 게시글 작성자에게 실시간 알림 전송
//    [HttpPost("SendCommentNotification")]
//    public async Task<IActionResult> SendCommentNotification([FromBody] CommentNotificationRequest request)
//    {
//        // ✅ 게시글 작성자가 현재 접속 중인지 확인
//        if (NotificationHub.IsUserOnline(request.PostOwnerId))
//        {
//            await _hubContext.Clients.User(request.PostOwnerId).SendAsync("ReceiveNotification",
//                $"💬 새로운 댓글이 달렸습니다: {request.CommentContent}");
//            return Ok(new { success = true, message = "작성자가 접속 중이므로 알림을 전송했습니다." });
//        }
//        else
//        {
//            return Ok(new { success = false, message = "작성자가 현재 접속 중이 아닙니다." });
//        }
//    }
//}

//// ✅ 댓글 알림 요청 모델
//public class CommentNotificationRequest
//{
//    public string PostOwnerId { get; set; }  // ✅ 게시글 작성자 ID
//    public string CommentContent { get; set; }  // ✅ 댓글 내용
//}
