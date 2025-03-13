using Microsoft.AspNetCore.Mvc;
using BoardCreate.Models;
using BoardCreate.Service;
using BoardCreate.Repositories;
using Microsoft.Extensions.Logging;
using BoardCreate.Models.ViewModels;
using BoardCreate.Models.Board;
using Microsoft.AspNetCore.Authorization;

namespace BoardCreate.Controllers
{

    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {

        private readonly ILogger<UserController> _logger;
        private readonly AdminService _adminService;
        private readonly AdminRepository _adminRepository;

        public AdminController(MemberRepository repository, ILogger<UserController> logger, AdminService adminService, AdminRepository adminRepository)
        {
            _adminRepository = adminRepository;
            _adminService = adminService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<IActionResult> AdminMain()
        {

            /* 세션이 없을때 보내기
            String? SessionUserID = HttpContext.Session.GetString("admin");
            if (SessionUserID == null || SessionUserID != "admin")
            {
                HttpContext.Session.Clear();
                return _messageService.SendMessage("LoginError");
            }
            */

            string manager = HttpContext.Session.GetString("manager");
            if (manager == null) HttpContext.Session.SetString("manager", "admin");


            var sectionListsStatusValid = new List<BoardSectionsModel>();
            int statusTyleValid = 0;
            var GetSectionListsValid = await _adminService.GetSectionListsAll(statusTyleValid);
            sectionListsStatusValid.AddRange(GetSectionListsValid); // 조건 없이 전체 데이터를 listA에 추가

            int maxIdx = GetSectionListsValid.Max(x => x.IDX);
            //ViewBag.MaxIdx = maxIdx;

            var sectionListsStatusInValid = new List<BoardSectionsModel>();
            int statusTyleInvalid = 1;
            var GetSectionListsInvalid = await _adminService.GetSectionListsAll(statusTyleInvalid);
            sectionListsStatusInValid.AddRange(GetSectionListsInvalid);

            int SectionIdx = -1;
            var BoardTabs = new List<SectionTabsModel>();
            var GetSectionTabs = await _adminService.GetSectionTabsListsAllService(SectionIdx);
            BoardTabs.AddRange(GetSectionTabs);

            /*
            // 특정 조건으로 BoardTabs 추가 (예: SectionStatus가 2인 경우)
            BoardTabs.AddRange(SectionLists.Where(section => section.SectionStatus == 2));
            */

            var SectionLists = new ViewModelLists
            {
                SectionListsStatusValid = sectionListsStatusValid,
                SectionListsStatusInvalid = sectionListsStatusInValid,
                BoardTabs = BoardTabs
            };

            return View(SectionLists);
        }
        [HttpPost]
        public async Task<IActionResult> SectionCreate(string SectionName, int SectionOrder)
        {
            try
            {
                bool result = await _adminService.SectionCreateCheck(SectionName, SectionOrder);
                return Json(new { isAvailable = result }); // JSON 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionCreate: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }
        [HttpPost]
        public async Task<IActionResult> SectionOrderUpDown(int SectionOrder, int Type)
        {
            try
            {
                bool result = await _adminService.SectionOrderUpDownService(SectionOrder, Type);
                return Json(new { isAvailable = result }); // JSON 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionOrderUpDown: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }
        [HttpPost]
        public async Task<IActionResult> SectionStatusUpdate(int SliderIDX, int SliderValue, int SliderOrignValue)
        {
            try
            {
                bool result = await _adminService.SectionStatusUpdateService(SliderIDX, SliderValue, SliderOrignValue);
                return Json(new { isAvailable = result }); // JSON 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionStatusUpdate: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }
        [HttpPost]
        public async Task<IActionResult> SectionNameUpdate(int SectionIdx, string SectionNameValue)
        {
            try
            {
                bool result = await _adminService.SectionNameUpdateService(SectionIdx, SectionNameValue);
                return Json(new { isAvailable = result }); // JSON 반환
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionNameUpdate: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }

        [HttpGet]
        public IActionResult GoToHome()
        {
            HttpContext.Session.Remove("manager");
            return RedirectToAction("Index", "User");
        }
        [HttpGet]
        public async Task<IActionResult> AdminMainDetail(int SectionIDX)
        {

            var boardSection = new BoardSectionsModel();
            boardSection = await _adminService.GetBoardSectionService(SectionIDX);

            var boardTabs = new List<SectionTabsModel>();
            var GetSectionTabs = await _adminService.GetSectionTabsListsAllService(SectionIDX);
            boardTabs.AddRange(GetSectionTabs);

            var model = new BoardDetailsViewModel
            {
                BoardSections= boardSection,
                BoardTabs = boardTabs
            };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> AdminTabsInsertSingle(int SectionIDX, string TabName)
        {
            try
            {
                bool result = await _adminService.AdminTabsInsertService(SectionIDX, TabName);
                return Json(new {isAvailable = result});
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionCreate: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }
        [HttpPost]
        public async Task<IActionResult> AdminTabsStatusUpdate(int CheckedIDX, int CheckedNumber)
        {
            try
            {
                bool result = await _adminService.AdminTabsStatusUpdateService(CheckedIDX, CheckedNumber);
                return Json(new {isAvailable = result});
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in SectionCreate: " + ex.Message);
                return Json(new { isAvailable = false, error = "서버 오류가 발생했습니다." }); // 오류 시 JSON 반환
            }
        }


    }
}
