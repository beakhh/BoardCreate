using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BoardCreate.Models;
using BoardCreate.Service;
using BoardCreate.Repositories;
using BoardCreate.Common;
using BoardCreate.Models.ViewModels;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using BoardCreate.Models.Board;
using System.Net;
using System.Net.Sockets;
using BoardCreate.Models.Member;
using System.Linq;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Collections.Specialized;
using System.Collections;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Azure;

namespace BoardCreate.Controllers
{
    public class UserController : Controller
    {
        private readonly ILogger<UserController> _logger;
        private readonly MemberRepository _memberRepository;
        private readonly MessageService _messageService;
        private readonly UserService _userService;
        private readonly CookieService _cookieService;

        public UserController(ILogger<UserController> logger, UserService userService, MemberRepository memberRepository, MessageService messageService, CookieService cookieService)
        {
            _logger = logger;
            _memberRepository = memberRepository;
            _userService = userService;
            _messageService = messageService;
            _cookieService = cookieService;
        }
        /*
        #region Cookie
        public bool SetCookie(string variable, object value, TimeSpan? expireTime)
        {
            if (string.IsNullOrEmpty(variable) || value == null) return false; // 쿠키 이름이나 값이 없으면 실패 반환

            var cookieOptions = new Microsoft.AspNetCore.Http.CookieOptions
            {
                HttpOnly = true, // 보안 설정
                Secure = true,   // HTTPS에서만 사용
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Strict, // SameSite 설정
                Expires = expireTime.HasValue
                    ? DateTime.Now.Add(expireTime.Value)
                    : DateTime.Now.AddDays(1) // 기본값: 1일
            };

            try
            {
                string serializedValue;

                // value is 는 패턴 매칭
                if (value is string stringValue)
                {
                    serializedValue = stringValue; // 문자열은 그대로 사용
                }
                else if (value is int intValue)
                {
                    serializedValue = intValue.ToString(); // 정수를 문자열로 변환
                }
                else if (value is List<string> listStringValue)
                {
                    serializedValue = JsonSerializer.Serialize(listStringValue); // 문자열 리스트를 JSON으로 직렬화
                }
                else if (value is List<int> listIntValue)
                {
                    serializedValue = JsonSerializer.Serialize(listIntValue); // 정수 리스트를 JSON으로 직렬화
                }
                else
                {
                    return false; // 지원하지 않는 데이터 타입이면 실패 반환
                }

                // 쿠키 저장
                Response.Cookies.Append(variable, serializedValue, cookieOptions);
                return true; // 성공 반환
            }
            catch
            {
                return false; // 예외 발생 시 실패 반환
            }
        }
        private T GetCookie<T>(string key)
        {
            if (!Request.Cookies.TryGetValue(key, out string cookieValue)) return default; // 쿠키가 없으면 기본값 반환
            try
            {
                if (typeof(T) == typeof(int))
                {
                    if (int.TryParse(cookieValue, out int intValue))
                    {
                        return (T)(object)intValue; // int 변환
                    }
                }
                else if (typeof(T) == typeof(List<string>))
                {
                    return JsonSerializer.Deserialize<T>(cookieValue); // List<string> 변환
                }
                else if (typeof(T) == typeof(List<int>))
                {
                    return JsonSerializer.Deserialize<T>(cookieValue); // List<int> 변환
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)cookieValue; // string 그대로 반환
                }
            }
            catch
            {
                // 변환 실패 시 기본값 반환
                return default;
            }
            return default; // 지원하지 않는 데이터 타입인 경우
        }

        public bool DeleteCookie(string key)
        {
            if (string.IsNullOrEmpty(key)) return false; // key가 없으면 에러    

            Response.Cookies.Delete(key);
            return true;
        }


        #endregion
        */
        #region HomeController 였던곳
        public async Task<IActionResult> Index()
        {
            // 여기에 조건 추가
            string manager = HttpContext.Session.GetString("manager");
            if (manager != null) HttpContext.Session.Remove("manager");

            var sectionListsStatusValid = new List<BoardSectionsModel>();
            var GetSectionLists = await _userService.GetSectionListService();

            sectionListsStatusValid.AddRange(GetSectionLists);

            var SectionLists = new ViewModelLists
            {
                SectionListsStatusValid = sectionListsStatusValid,
                UserSession = new UserSessionModel()
            };

            var userId = HttpContext.Session.GetObject<string>("UserID");

            if (!string.IsNullOrEmpty(userId))  SectionLists.UserSession = HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");

            return View(SectionLists);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRecentViewLists()
        {
            var response = await _userService.GetBoardRecentListService();

            return Json(response);
        }




        #endregion

        #region 회원 정보 관리 MemberController

        [HttpGet]
        public IActionResult Register()
        {
            return View("Member/Register");
        }

        [HttpPost]
        public async Task<IActionResult> Register(MemberModel model)
        {
            //int selectUserID = await _memberRepository.SelectUserID(model.UserId); 사용하지 않음

            bool UserIDandUserNickCheck = await _memberRepository.SelectIdNickCheckAsync(model.UserId, model.NickName);

            if (UserIDandUserNickCheck == true)
            {
                if (ModelState.IsValid)
                {
                    string salt = PasswordHelper.GenerateSalt();
                    string hashedPassword = PasswordHelper.GenerateSaltedHash(model.UserPW, salt);

                    model.UserSalt = salt;
                    model.UserPW = hashedPassword;

                    await _memberRepository.AddMember(model);

                    return _messageService.SendMessage("RegisterSuccess");
                }
                else
                {
                    foreach (var state in ModelState)
                    {
                        foreach (var error in state.Value.Errors)
                        {
                            //_logger.LogError($"Validation error in {state.Key}: {error.ErrorMessage}");
                        }
                    }
                    //_logger.LogInformation("Model is invalid.");
                    return View(model);
                }
            }
            else
            {
                return _messageService.SendMessage("RegisterFailure"); ;
            }
        }
        /*
        [HttpPost]
        public async Task<IActionResult> CheckUserIdOrNickName(string userValue, int type)
        {
            bool isAvailable;

            isAvailable = await _memberRepository.Check_Id_Nick_duplication(userValue, type);

            return Json(new { isAvailable });
        }
        */
        [HttpPost]
        public async Task<IActionResult> CheckUserIdOrNickName(string userValue, int type)
        {
            try
            {
                bool isAvailable = await _memberRepository.Check_Id_Nick_duplication(userValue, type);
                return Json(new { isAvailable });
            }
            catch (Exception ex)
            {
                // 예외 메시지를 로깅
                Console.WriteLine("Error in CheckUserIdOrNickName: " + ex.Message);
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }
        /*
        // 회원 목록 조회
        public async Task<IActionResult> Index()
        {
            var member = await _memberRepository.GetAllMembersAsync();
            return View(member);
        }
        */
        [HttpPost]
        public async Task<IActionResult> Login(string UserId, string UserPW, bool IdSave)
        {
            var user = await _userService.LoginCheck(UserId, UserPW, IdSave);

            if (user == null) return _messageService.SendMessage("LoginFailure");

            //HttpContext.Session.SetString("NickName", user.NickName);

            var userSession = new UserSessionModel
            {
                NickName = user.NickName,
                UserID = user.UserId,
                IDX = user.IDX,
                UserLevel = user.UserLevel
            };

            HttpContext.Session.SetObject("UserID", user.UserId);
            HttpContext.Session.SetObject($"UserSession_{user.UserId}", userSession);

            // 사용자 ID를 `Claims`에 추가하여 `Identity.Name`에 저장 (SignalR에서 사용 가능)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserId), // `Context.User.Identity.Name`으로 설정
                new Claim(ClaimTypes.Role, user.UserLevel.ToString()) // 필요하면 역할(Role)도 추가 가능
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 로그인 처리 (쿠키 기반 인증)
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            return _messageService.SendMessage("LoginSuccess");

        }
        [HttpGet]
        public IActionResult Logout()
        {
            var userId = HttpContext.Session.GetObject<string>("UserID");
            // 특정 사용자 세션 삭제
            HttpContext.Session.Remove($"UserSession_{userId}");
            // UserID 세션 삭제
            HttpContext.Session.Remove("UserID");
            // 로그아웃 후 리다이렉트

            return _messageService.SendMessage("LogoutSuccess");
        }

        #endregion

        #region BoardController 였던 곳

        [HttpGet]
        public async Task<IActionResult> Board(BoardSundryModel boardSundryModel)
        {
            var userId = HttpContext.Session.GetObject<string>("UserID");

            UserSessionModel userSession = null;
            string session_UserID = "Guest"; 

            if (!string.IsNullOrEmpty(userId))
            {
                userSession = HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");
                boardSundryModel.UserID = userId;
                session_UserID = userSession.UserID;
            }

            var firstKey = _cookieService.GetCookieDictionary("userRecentVisit", 1)?.ToString();

            string localIP = GetIPAddress();
            if (firstKey == null)
            {
                bool cookieCheck = CookieCheck();
                if (cookieCheck)
                {
                    bool cookieChec1k = _cookieService.SetCookieDictionary("userRecentVisit", 0, TimeSpan.FromDays(1), boardSundryModel.SectionIDX);
                }
                else
                {
                    HttpContext.Session.SetObject($"nonCookieResult_{localIP}", 0);
                }
            }
            else
            {
                bool cookieChec1k = _cookieService.SetCookieDictionary("userRecentVisit", 0, TimeSpan.FromDays(1), boardSundryModel.SectionIDX);
            }

            int UserLevel = userSession?.UserLevel ?? 8;
             
            var TabsAllList = await _userService.GetSectionTabsListService(boardSundryModel.SectionIDX, UserLevel);
            var BoardTabs = new List<SectionTabsModel>();
            BoardTabs.AddRange(TabsAllList);

            var validTabNames = TabsAllList.Select(t => t.TabName).ToList();
            if (!validTabNames.Contains(boardSundryModel.SelectedTab)) boardSundryModel.SelectedTab = "전체";

            BoardViewModel ModelCollect = await _userService.GetBoardListService(UserLevel, boardSundryModel);

            ModelCollect.BoardTabs = BoardTabs;

            ModelCollect.UserID = session_UserID;


            return View("Board/Board", ModelCollect);
        }

       
        [HttpGet]
        public async Task<IActionResult> BoardInsert(int SectionIDX)
        {
            // 나중에 이 값이 널이면 로그인 화면으로 ㄱㄱ
            //string? SessionNickName = HttpContext.Session.GetString("NickName") ?? "";

            var userId = HttpContext.Session.GetObject<string>("UserID");
            UserSessionModel userSession = null;
            if (!string.IsNullOrEmpty(userId))
            {
                userSession = HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");
            }

            string SessionNickName = userSession?.NickName ?? "noting";
            string SessionUserID = userSession?.UserID ?? "noting";
            int SessionLevel = userSession?.UserLevel ?? 8;

            if (SessionNickName == "noting") return _messageService.SendMessage("LoginPlease");
            if (SessionLevel == 8) return _messageService.SendMessage("LoginError");

            ViewBag.SessionNickName = SessionNickName;

            BoardSectionsModel boardSectionsModel = await _userService.GetSectionService(SectionIDX);

            int UserLevel = userSession?.UserLevel ?? -1;
            if (UserLevel == -1) return _messageService.SendMessage("LoginPlease");

            var TabsAllList = await _userService.GetSectionTabsListService(SectionIDX, UserLevel);

            var BoardTabs = new List<SectionTabsModel>();
            BoardTabs.AddRange(TabsAllList); // 나중에 NickName으로 Board처럼 탭상태가 0만 오게 처리

            var sModels = new BoardInsertViewModel
            {
                SessionNickName = SessionNickName,
                SessionUserID = SessionUserID,
                SessionLevel = SessionLevel,
                BoardSections = boardSectionsModel,
                BoardTabs = BoardTabs
            };
            return View("Board/BoardInsert", sModels);
        }
        [HttpPost]
        public async Task<IActionResult> BoardInsert(BoardModel boardModel)
        {
            bool InsertBoardResult = await _userService.SetBoardService(boardModel);
            if (!InsertBoardResult) return _messageService.SendMessage("BoardInsertFalse");

            /*
            int SectionLists = boardModel.SectionIDX;
             중요 밑에선 뷰로 가는거라 SectionLists이 인트라서 오류 그래서 컨트롤러를 타는 RedirectToAction를 이용
            return View("Board/Board", SectionLists);
             밑처럼 하면 값도 보낼 수 있음 컨트롤러에
            return RedirectToAction("Board", new { SectionIDX = boardModel.SectionIDX });
            */

            return _messageService.SendMessage("BoardInsertSuccess");
        }
        [HttpGet]
        public async Task<IActionResult> BoardDetail(int BoardIDX, int SectionIDX, int ViewCount = 0)
        {
            int viewCountCheck = 0;

            string localIP = GetIPAddress();
            int? nonCookieResult = HttpContext.Session.GetObject<int?>($"nonCookieResult_{localIP}") ?? -1;

            viewCountCheck = _userService.BoardDetailViewCountUpdateService(nonCookieResult, BoardIDX, SectionIDX);
            /*
            var firstKey = _cookieService.GetCookieDictionary("userRecentVisit", 1)?.ToString();
            if (nonCookieResult == -1)
            {
                if (firstKey == null)
                {
                    bool cookieCheck1 = CookieCheck();

                    if (!cookieCheck1)
                    {
                        viewCountCheck = 1;
                        HttpContext.Session.SetObject($"nonCookieResult_{localIP}", 0);
                        /*
                        bool cookieCheck1 = CookieCheck();
                        if (cookieCheck1)
                        {
                            _cookieService.SetCookieDictionary("userRecentVisit", 0, TimeSpan.FromDays(1), SectionIDX);
                            _cookieService.SetCookieDictionary("userRecentVisit", 1, TimeSpan.FromDays(1), SectionIDX, BoardIDX);
                        }
                        else
                        {
                            ViewCountCheck = 1;
                        }
                        /*
                    }
                }
                else
                {
                    bool cookieCheck = _cookieService.SetCookieDictionary("userRecentVisit", 1, TimeSpan.FromDays(1), SectionIDX, BoardIDX);
                }
            }
            else if(firstKey == null) viewCountCheck = 1;
            */
            _userService.UpdateUserRecontBoardDetail(SectionIDX, BoardIDX);

            var firstKeyValue = _cookieService.GetCookieDictionary("userRecentVisit", 3) as dynamic; // dynamic 공부
            if (firstKeyValue != null)
            {
                var key = firstKeyValue.GetType().GetProperty("Key")?.GetValue(firstKeyValue)?.ToString();
                var value = firstKeyValue.GetType().GetProperty("Value")?.GetValue(firstKeyValue)?.ToString();

                // Value가 List<int>인지 확인
                if (!string.IsNullOrEmpty(value))
                {
                    bool valueCheck = value?.Contains(BoardIDX.ToString());
                    if (valueCheck) viewCountCheck = 1;
                }
            }
            else viewCountCheck = 1;
            
            var userId = HttpContext.Session.GetObject<string?>("UserID");

            // 여기 기준 20을 변수로 받으면 좋을 듯
            if (ViewCount >20 || userId == null) viewCountCheck = 1;

            BoardDetailModel boardDetailModel = new BoardDetailModel();

            UserSessionModel userSessionModel = new UserSessionModel();

            /*
            if (userId != null)
            {
                userSessionModel = HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");
            }
            */
            boardDetailModel = await _userService.GetBoardDetailService(BoardIDX, viewCountCheck, userId);

            if (userId != null)
            {
                boardDetailModel.UserSession = HttpContext.Session.GetObject<UserSessionModel>($"UserSession_{userId}");
            }
            else
            {
                boardDetailModel.UserSession = userSessionModel;
            }

            //if(boardDetailModel == null) return _messageService.SendMessage("GetboardDetailModelFalse");

            if (boardDetailModel == null)
            {
                TempData["GetboardDetailModelFalseAlertMessage"] = "해당 게시글이 존재하지 않습니다.";
                return RedirectToAction("Board", new { SectionIDX }); // 탭 이름까지 나중에 ㄱㄱ
            }


            return View("Board/BoardDetail", boardDetailModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserPreferencesLikeStatus(int BoardIDX, int UpdateType, string UserID)
        {
            try
            {
                bool isAvailable = await _userService.UpdateUserPreferencesLikeStatusService(BoardIDX, UpdateType, UserID);
                return Json(new { isAvailable });
            }
            catch (Exception ex)
            {
                // 예외 메시지를 로깅
                Console.WriteLine("Error in UpdateUserPreferencesLikeStatus: " + ex.Message);
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CommentsInsert(CommentsModel commentsModel, int CommentsType)
        {
            try
            {
                int result = await _userService.CommentsInsertService(commentsModel, CommentsType);

                if (result == null)  return NotFound(new { message = "데이터를 찾을 수 없습니다." });

                var response = new
                {
                    isAvailable = true,
                    data = result
                };
                return Json(response);
            }
            catch (Exception ex)
            {
                // 예외 로그 기록 (ex.Message는 서버에만 남김)
                _logger.LogError($"Error CommentsInsert: {ex.Message}");

                // 클라이언트에 일반적인 메시지 반환
                return StatusCode(500, new { message = "요청을 처리하는 중 문제가 발생했습니다. 다시 시도해주세요." });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CommentsReplyDynamicSelect(int BoardIDX,int TargetedCommentsIDX, int CurrentComments, int CurrentCommentsPlus)
        {
            try
            {
                var commentsListModel = await _userService.CommentsReplyDynamicSelectService(BoardIDX, TargetedCommentsIDX, CurrentComments, CurrentCommentsPlus);

                if (commentsListModel == null || !commentsListModel.Any())
                {
                    return Json(new { isAvailable = false, data = new List<CommentsModel>() });
                }

                var response = new
                {
                    isAvailable = true, // 여기선 항상 true
                    data = commentsListModel
                };
                return Json(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error CommentsDynamicSelect: {ex.Message}");

                return Json(new { isAvailable = false, message = "요청을 처리하는 중 문제가 발생했습니다. 다시 시도해주세요." });
            }
        }
        

        public async Task<IActionResult> UpdateCommentsLikes(int CommentsIdx, string UserID, int Likestype, int CurrentLikeStatus)
        {
            try
            {
                var commentsLikesModel = await _userService.UpdateCommentsLikesService(CommentsIdx, UserID, Likestype , CurrentLikeStatus);

                var response = new
                {
                    data = commentsLikesModel
                };
                return Json(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateCommentsLikes: " + ex.Message);
                return StatusCode(500, "서버 오류가 발생했습니다.");
            }
        }


        static string GetIPAddress()
        {
            try
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork) return ip.ToString();
                }
                throw new Exception("IPv4 주소를 찾을 수 없습니다.");
            }
            catch (Exception ex)
            {
                return $"에러 발생: {ex.Message}";
            }
        }
        /*
        public bool UpdateUserRecontBoardDetail(int SectionIdx, int BoardIDX) // 최근기록 쿠키저장
        {
            var userRecent = _cookieService.GetCookie<List<string>>("userRecentBoardDetail") ?? null;
            string combined = $"{SectionIdx}_{BoardIDX}";

            if (userRecent == null)
            {
                userRecent = new List<string> { combined }; // 바로 현재 값을 추가
                _cookieService.SetCookie("userRecentBoardDetail", userRecent, TimeSpan.FromDays(1));
                return true; // 초기화 후 바로 반환
            }

            if (userRecent.Contains(combined)) userRecent.Remove(combined);
            userRecent.Insert(0, combined);

            return _cookieService.SetCookie("userRecentBoardDetail", userRecent, TimeSpan.FromDays(1));
        }
        
        */
        public bool CookieCheck()
        {
            _cookieService.SetCookie("cookieCheck",0, TimeSpan.FromDays(1));
            int result = _cookieService.GetCookie<int>("cookieCheck");
            if (result != 0) return false;
            _cookieService.DeleteCookie("cookieCheck");
            return true; 
        }



        #endregion




        public IActionResult Privacy()
        {


            return Content("쿠키 이름(key)을 입력하세요.");
        }

        /*

        [HttpGet]
        public IActionResult SetCookie1()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = false, // HTTPS 환경이 아니라면 false
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(1) // UTC 기준으로 만료 시간 설정
            };

            Response.Cookies.Append("asdf", "HelloWorld2", cookieOptions);

            return Content("쿠키가 설정되었습니다.");
        }

        // 쿠키 확인 메서드
        [HttpGet]
        public IActionResult GetCookie1()
        {
            if (Request.Cookies.TryGetValue("asdf", out string cookieValue))
            {
                return Content($"쿠키 값: {cookieValue}");
            }

            return Content("쿠키를 찾을 수 없습니다.");
        }

        */




        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
