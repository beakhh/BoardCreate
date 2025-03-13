using BoardCreate.Repositories;
using BoardCreate.Common;
using BoardCreate.Models;
using Microsoft.Extensions.Logging;
using BoardCreate.Controllers;
using BoardCreate.Models.Board;

namespace BoardCreate.Service
{
    public class AdminService
    {
        private readonly ILogger<UserController> _logger;
        private readonly AdminRepository _adminRepository;

        public AdminService(MemberRepository repository, ILogger<UserController> logger, AdminRepository adminRepository)
        {
            _logger = logger;
            _adminRepository = adminRepository;
        }

        public async Task<bool> SectionCreateCheck(string SectionName, int SectionOrder)
        {
            bool SectionResult = await _adminRepository.SectionDuplicateCheck(SectionName);

            if (SectionResult == true) 
            {
                //동일한 이름일때 메시지
                return false;
            }
            else
            {
                int MaxSectionOrder = await _adminRepository.GetSectionOrderMax();
                if (MaxSectionOrder > SectionOrder) SectionOrder = MaxSectionOrder + 1;

                bool createSection = await _adminRepository.SectionCreate(SectionName, SectionOrder);
                if (createSection != true) return false; // 이 부분 필요한지 고민

                int MaxSectionIDX = await _adminRepository.GetSectionIDXMax();

                bool createSectionTab = await _adminRepository.SectionTabsCreate(MaxSectionIDX);
                if (createSectionTab == true) return true;

                return false;
            }

        }
        /*
        public async Task<List<BoardSectionsModel>> GetSectionListsAll()
        {
            return await _adminRepository.GetSectionListAll(); 
        }       
        */
        public async Task<List<BoardSectionsModel>> GetSectionListsAll(int statusTyle)
        {
            var result = await _adminRepository.GetSectionListAll(statusTyle);

            /*
            // 1. SectionOrder 기준 정렬
            result = result.OrderBy(x => x.SectionOrder).ToList();
            // 2. SectionOrder를 1부터 순차적으로 설정
            for (int i = 0; i < result.Count; i++)
            {
                result[i].SectionOrder = i + 1;
            }
            */

            return result;
        }
        public async Task<List<SectionTabsModel>> GetSectionTabsListsAllService(int SectionIdx)
        {
            var result = await _adminRepository.GetSectionTabsListAllRepository(SectionIdx);

            return result;
        }

        public async Task<bool> SectionOrderUpDownService(int SectionOrder, int type)
        {
            int SectionOrderResult = 3;

            // 아마 여기에 상태가 0인지 1인지 등등으로도 적용할 듯
            if (type == 0) SectionOrderResult = SectionOrder - 1;
            else if (type == 1) SectionOrderResult = SectionOrder + 1;

            if (SectionOrderResult == 3) return false;

            bool SectionOrderUpDownSuccess = await _adminRepository.SetSectionOrderUpDownUpdate(SectionOrder, SectionOrderResult);

            return SectionOrderUpDownSuccess;

        }

        public async Task<bool> SectionStatusUpdateService(int SliderIDX, int SliderValue, int SliderOrignValue)
        {
            int sliderV = 0;
            int MaxSectionOrder = -1;

            if (SliderValue >= 2)
            {
                sliderV = 1;
            }
            else
            {
                if (SliderOrignValue >= 2)
                {
                    sliderV = 2;
                    int MaxSOrder = await _adminRepository.GetSectionOrderMax();
                    MaxSectionOrder = MaxSOrder + 1;
                }
            }
            return await _adminRepository.SectionStatusUpdateRepository(SliderIDX, SliderValue, MaxSectionOrder, sliderV );
        }

        public async Task<bool> SectionNameUpdateService(int SectionIdx, string SectionNameValue)
        {
            return await _adminRepository.SectionNameUpdateRepository(SectionIdx, SectionNameValue);
        }

        public async Task<BoardSectionsModel> GetBoardSectionService(int SectionIDX)
        {
            var result = new BoardSectionsModel();
            result = await _adminRepository.GetBoardSectionRepository(SectionIDX);

            return result;
        }
        public async Task<bool> AdminTabsInsertService(int SectionIDX, string TabName)
        {
            return await _adminRepository.AdminTabsInsertRepository(SectionIDX, TabName);
        }
        public async Task<bool> AdminTabsStatusUpdateService(int CheckedIDX, int CheckedNumber)
        {
            return await _adminRepository.AdminTabsStatusUpdateRepository(CheckedIDX, CheckedNumber);
        }

    }


}

