using BoardCreate.Common;
using BoardCreate.Models.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Web;

namespace BoardCreate.Service
{
    public class MessageService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CookieService _cookieService;

        public MessageService(IHttpContextAccessor httpContextAccessor, CookieService cookieService)
        {
            _httpContextAccessor = httpContextAccessor;
            _cookieService = cookieService;
        }

        public IActionResult SendMessage(string msgFlag)
        {
            string msg = "알 수 없는 요청입니다.";
            string url = "/";

            if (msgFlag.Equals("RegisterFailure"))
            {
                msg = "회원가입에 실패하였습니다.";
                url = "/User/Register";
            }
            else if (msgFlag.Equals("RegisterSuccess"))
            {
                msg = "회원가입에 성공하였습니다.";
                url = "/User/Index";
            }
            else if (msgFlag.Equals("LoginFailure"))
            {
                msg = "로그인에 실패하였습니다.";
                url = "/User/Index";
            }
            else if (msgFlag.Equals("LogoutSuccess"))
            {
                msg = "로그아웃 성공 하였습니다.";
                url = "/User/Index";
            }
            else if (msgFlag.Equals("LoginSuccess"))
            {
                var userId = _httpContextAccessor.HttpContext.Session.GetObject<string>("UserID");
                UserSessionModel userSession = null;
                if (!string.IsNullOrEmpty(userId))
                {
                    userSession = _httpContextAccessor.HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");
                }
                string nickName = userSession?.NickName ?? "error";

                msg = $"반갑습니다 {nickName} 님.";
                url = "/User/Index";
            }
            else if (msgFlag.Equals("LoginError"))
            {
                msg = "사용자 인증 오류 다시 로그인 해주세요.";
                url = "/User/Index";
            }
            else if (msgFlag.Equals("LoginPlease"))
            {
                msg = "로그인 해 주세요.";
                url = "/User/Index";
            }            
            /*
            else if (msgFlag.Equals("GetboardDetailModelFalse"))
            {
                msg = "게시글 가져오기 오류 다시 시도해 주세요.";
                url = "/User/Index";
            }
            */
            else if (msgFlag.Equals("BoardInsertFalse"))
            {
                int firstSectionIDX = GetSectionIDXsInCookieFirst();
                if (firstSectionIDX == -1)
                {
                    msg = "등록 오류 다시 등록해 주세요.";
                    url = "/User/Index";
                }
                msg = "등록 오류 다시 등록해 주세요.";
                url = $"/User/Board?SectionIDX={firstSectionIDX}";
            }
            else if (msgFlag.Equals("BoardInsertSuccess"))
            {
                int firstSectionIDX = GetSectionIDXsInCookieFirst();
                if (firstSectionIDX == -1)
                {
                    msg = "등록 성공 쿠키 확인 불가로 인한 홈 화면으로 이동.";
                    url = "/User/Index";
                }
                msg = "등록성공.";
                url = $"/User/Board?SectionIDX={firstSectionIDX}";
            }
            // URL 인코딩 적용
            string encodedMsg = HttpUtility.UrlEncode(msg);
            string encodedUrl = HttpUtility.UrlEncode(url);

            return new RedirectToActionResult("ShowMessage", "Message", new
            {
                msg = encodedMsg,
                url = encodedUrl
            });
        }


        public int GetSectionIDXsInCookieFirst()
        {
            // GetCookieDictionary 호출 및 값 가져오기
            var firstKey = _cookieService.GetCookieDictionary("userRecentVisit", 1) as string;

            // 값이 null이면 -1 반환, 그렇지 않으면 int로 변환
            if (string.IsNullOrEmpty(firstKey)) return -1;

            if (int.TryParse(firstKey, out int result)) return result; // 변환 성공 시 정수 반환

            return -1; 
        }
    }

}

