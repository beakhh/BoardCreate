using BoardCreate.Repositories;
using BoardCreate.Common;
using BoardCreate.Models;
using Microsoft.Extensions.Logging;
using BoardCreate.Controllers;
using BoardCreate.Models.Board;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using BoardCreate.Models.Member;
using BoardCreate.Models.ViewModels;

namespace BoardCreate.Service
{
    public class UserService
    {
        private readonly MemberRepository _memberRepository;
        private readonly ILogger<UserController> _logger;
        private readonly BoardRepository _boardRepository;
        private readonly CookieService _cookieService;

        public UserService(MemberRepository memberRepository, ILogger<UserController> logger, BoardRepository boardRepository, CookieService cookieService)
        {
            _memberRepository = memberRepository;
            _logger = logger;
            _boardRepository = boardRepository;
            _cookieService = cookieService;

        }
        
        public async Task<MemberModel> LoginCheck(string UserId, string UserPW, bool IdSave)
        {
            int type = 0;
            var user = await _memberRepository.GetUserByIdData(UserId, type);
            if(user == null) return null;

            string decryptedPassword = PasswordHelper.GenerateSaltedHash(UserPW, user.UserSalt); // static이라 직접 소환해야함
            if (decryptedPassword != user.UserPW) return null;

            return user;
        }

        #region Board
        public async Task<List<BoardSectionsModel>> GetSectionListService()
        {
            var result = await _boardRepository.GetSectionListRepository();
            return result;
        }
        public async Task<List<SectionTabsModel>> GetSectionTabsListService(int SectionIDX, int UserLevel)
        {
            var result = await _boardRepository.GetSectionTabsLIstRepository(SectionIDX, UserLevel);
            return result;
        }
        public async Task<BoardSectionsModel> GetSectionService(int SectionIDX)
        {
            BoardSectionsModel result = await _boardRepository.GetSectionRepository(SectionIDX);
            return result;
        }

        public async Task<bool> SetBoardService(BoardModel boardModel)
        {
            var result = await _boardRepository.SetBoardRepository(boardModel);
            return result;
        }

        public async Task<BoardViewModel> GetBoardListService(int UserLevel, BoardSundryModel boardSundryModel)
        {
            int boardListCount = await _boardRepository.GetBoardListCountRepository(boardSundryModel.SectionIDX, UserLevel, boardSundryModel.SelectedTab);

            int totalCount = (int)Math.Ceiling((double)boardListCount / (double)boardSundryModel.PageSize);
            if (boardSundryModel.PageSize % 5 != 0) boardSundryModel.PageSize = 5;
            if (boardSundryModel.CurrentPage > totalCount) boardSundryModel.CurrentPage = 1;

            boardSundryModel.PageTotalCount = totalCount;

            //Tuple 사용
            //var BoardAllList = await _boardRepository.GetBoardListRepository(SectionIDX, UserLevel, BoardType, PageSize, CurrentPage, boardListCount);
            var BoardAllList = await _boardRepository.GetBoardListRepository( UserLevel, boardListCount, boardSundryModel);

            var returnModel = new BoardViewModel
            {
                BoardLists = BoardAllList,
                BoardSundry = boardSundryModel
            };

            return returnModel;
        }

        public int BoardDetailViewCountUpdateService(int? nonCookieResult, int BoardIDX, int SectionIDX)
        {
            int viewCountCheck = 0;
            var firstKey = _cookieService.GetCookieDictionary("userRecentVisit", 1)?.ToString();

            if (nonCookieResult == -1)
            {
                if (firstKey == null)
                {
                    bool cookieCheck = CookieCheck();
                    if (!cookieCheck) viewCountCheck = 1;
                }
                else
                {
                    _cookieService.SetCookieDictionary("userRecentVisit", 1, TimeSpan.FromDays(1), SectionIDX, BoardIDX);
                    viewCountCheck = 0;
                }
            }
            else if (firstKey == null) viewCountCheck = 1;

            return viewCountCheck;
        }

        public void UpdateUserRecontBoardDetail(int SectionIDX, int BoardIDX)
        {
            bool cookieCheck = CookieCheck();
            if(cookieCheck == true)
            {
                var userRecent = _cookieService.GetCookie<List<string>>("userRecentBoardDetail") ?? null;
                string combined = $"{SectionIDX}_{BoardIDX}";

                if (userRecent == null)
                {
                    userRecent = new List<string> { combined }; // 바로 현재 값을 추가
                    _cookieService.SetCookie("userRecentBoardDetail", userRecent, TimeSpan.FromDays(1));
                }

                if (userRecent.Contains(combined)) userRecent.Remove(combined);
                userRecent.Insert(0, combined);

                _cookieService.SetCookie("userRecentBoardDetail", userRecent, TimeSpan.FromDays(1));
            }
        }

        public bool CookieCheck()
        {
            _cookieService.SetCookie("cookieCheck", 0, TimeSpan.FromDays(1));
            int result = _cookieService.GetCookie<int>("cookieCheck");
            if (result != 0) return false;
            _cookieService.DeleteCookie("cookieCheck");
            return true;
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


        public async Task<BoardDetailModel> GetBoardDetailService(int BoardIDX, int ViewCountCheck, string userId)
        {
            BoardDetailModel boardDetailModel = new BoardDetailModel();
            UserPreferencesModel userPreferencesModel = new UserPreferencesModel(); 
            int viewCountCheck = ViewCountCheck;

            if (userId != null)
            {
                bool Duplicatecheck = await _memberRepository.UserPreferencesDuplicateCehck(userId , BoardIDX);
                viewCountCheck = 1;
                if (!Duplicatecheck)
                {
                    bool result = await _memberRepository.SetBoardDetailRepository(userId, BoardIDX);
                    viewCountCheck = 0;
                }
                int type = 0;
                userPreferencesModel = await _memberRepository.GetUserPreferences(userId, BoardIDX, type);
            }

            boardDetailModel = await _boardRepository.GetBoardDetailRepository(BoardIDX, viewCountCheck);
            if(boardDetailModel == null) return null;

            boardDetailModel.UserPreferences = userPreferencesModel;

            return boardDetailModel;
        }

        public async Task<bool> UpdateUserPreferencesLikeStatusService(int BoardIDX, int UpdateType, string UserID)
        {
            bool result = false;
            if (UpdateType == 0 || UpdateType == 1)
            {
                result = await _boardRepository.UpdateUserPreferencesLikeStatusRepository(BoardIDX, UpdateType, UserID);
            }


            return result;
        }
        public async Task<int> CommentsInsertService(CommentsModel commentsModel, int CommentsType)
        {
            int result = await _boardRepository.CommentsInsertRepository(commentsModel, CommentsType);
            return result;
        }

        public async Task<List<CommentsModel>> CommentsReplyDynamicSelectService(int BoardIDX,int TargetedCommentsIDX, int CurrentComments, int CurrentCommentsPlus)
        {
            var commentsListModel = await _boardRepository.CommentsReplyDynamicSelectRepository(BoardIDX, TargetedCommentsIDX, CurrentComments, CurrentCommentsPlus);

            if (commentsListModel == null || !commentsListModel.Any()) commentsListModel = null;

            return commentsListModel;
        }
       


        public async Task<CommentsLikesModel> UpdateCommentsLikesService(int CommentsIdx, string UserID, int Likestype, int CurrentLikeStatus)
        {

            //int duplicateLikeStstusCheck = await _boardRepository.GetLikeStatusRepository(CommentsIdx, UserID);

            var commentsLikesModel = await _boardRepository.UpdateCommentsLikesRepository(CommentsIdx, UserID, Likestype, CurrentLikeStatus);
            /*
            if (commentsLikesModel != null)
            {
                commentsLikesModel.LikeStatus = duplicateLikeStstusCheck;
            }
            */
            return commentsLikesModel;
        }




        public async Task<BoardRecentListResponse> GetBoardRecentListService()
        {
            List<BoardModel>? recontList_Every = await _boardRepository.GetBoardRecentListAllMyRepository();
            List<BoardModel>? recontList_My = new();

            var userRecent = _cookieService.GetCookie<List<string>>("userRecentBoardDetail") ?? null;
            if (userRecent != null)
            {
                recontList_My = await _boardRepository.GetBoardRecentListMyRepository(userRecent);
            }

            int isAvailable = 0;
            if (recontList_Every?.Any() == true) isAvailable += 1;
            if (recontList_My?.Any() == true) isAvailable += 2;

            return new BoardRecentListResponse
            {
                isAvailable = isAvailable,
                DataRecontListEvery = recontList_Every ,
                DataRecontListMy = recontList_My
            };
        }

        #endregion










    }
}