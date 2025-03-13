//using BoardCreate.Service;
//using BoardCreate.Repositories;
//using Microsoft.AspNetCore.Mvc;
//using BoardCreate.Models;
//using BoardCreate.Common;

//namespace BoardCreate.Controllers
//{
//    public class Member : Controller
//    {

//        private readonly MemberRepository _memberRepository;
//        private readonly MemberService _memberService;
//        private readonly MessageService _messageService;
//        private readonly ILogger<Member> _logger;

//        public Member(MemberRepository memberRepository, ILogger<Member> logger, MemberService memberService, MessageService messageService)
//        {
//            _memberRepository = memberRepository;
//            _logger = logger;
//            _memberService = memberService;
//            _messageService = messageService;
//        }

//        // 회원가입 하러 가기
//        [HttpGet]
//        public IActionResult Register()
//        {
//            return View();
//        }
//        /*
//        // 회원가입 하러 가기
//        [HttpGet]
//        public IActionResult Register()
//        {
//            var model = new MemberModel
//            {
//                UserId = null,
//                UserPW = null,
//                NickName = null
//            };
//            TryValidateModel(model);

//            return View(model);
//        }
//        */

//        [HttpPost]
//        public async Task<IActionResult> Register(MemberModel model)
//        {
//            //int selectUserID = await _memberRepository.SelectUserID(model.UserId); 사용하지 않음

//            bool UserIDandUserNickCheck = await _memberRepository.SelectIdNickCheckAsync(model.UserId, model.NickName);

//            if (UserIDandUserNickCheck == true)
//            {
//                if (ModelState.IsValid)
//                {
//                    string salt = PasswordHelper.GenerateSalt();
//                    string hashedPassword = PasswordHelper.GenerateSaltedHash(model.UserPW, salt);

//                    model.UserSalt = salt;
//                    model.UserPW = hashedPassword;

//                    await _memberRepository.AddMember(model);
//                    //회원가입 성공시 메시지 보내야함
//                    return RedirectToAction("Index", "Home");
//                }
//                else 
//                {
//                    foreach (var state in ModelState)
//                    {
//                        foreach (var error in state.Value.Errors)
//                        {
//                            _logger.LogError($"Validation error in {state.Key}: {error.ErrorMessage}");
//                        }
//                    }
//                    _logger.LogInformation("Model is invalid.");
//                    return View(model);
//                }
//            }
//            else
//            {
//                return _messageService.SendMessage("RegisterFailure"); ;
//            }
//        }

//        /*
//        [HttpPost]
//        public async Task<IActionResult> CheckUserIdOrNickName(string userValue, int type)
//        {
//            bool isAvailable;

//            isAvailable = await _memberRepository.Check_Id_Nick_duplication(userValue, type);

//            return Json(new { isAvailable });
//        }
//        */
//        // 아이디 비밀번호 중복확인 ajax이용
//        [HttpPost]
//        public async Task<IActionResult> CheckUserIdOrNickName(string userValue, int type)
//        {
//            try
//            {
//                bool isAvailable = await _memberRepository.Check_Id_Nick_duplication(userValue, type);
//                return Json(new { isAvailable });
//            }
//            catch (Exception ex)
//            {
//                // 예외 메시지를 로깅
//                Console.WriteLine("Error in CheckUserIdOrNickName: " + ex.Message);
//                return StatusCode(500, "서버 오류가 발생했습니다.");
//            }
//        }
//        /*
//        // 회원 목록 조회
//        public async Task<IActionResult> Index()
//        {
//            var member = await _memberRepository.GetAllMembersAsync();
//            return View(member);
//        }
//        */
        
//        [HttpPost]
//        public async Task<IActionResult> Login(string UserId, string UserPW, bool IdSave)
//        {
//            var user = await _memberService.LoginCheck(UserId, UserPW, IdSave);

//            if (user == null)
//            {
//                return _messageService.SendMessage("LoginFailure"); 
//            }

//            HttpContext.Session.SetString("NickName", user.NickName);
//            return _messageService.SendMessage("LoginSuccess");

//        }


//    }
//}
