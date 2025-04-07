using BoardCreate.Models.Board;

namespace BoardCreate.Models.ViewModels
{
    public class ViewModelLists
    {
        public List<BoardSectionsModel> SectionListsStatusValid { get; set; }
        public List<BoardSectionsModel> SectionListsStatusInvalid { get; set; }
        public List<SectionTabsModel> BoardTabs { get; set; }
        public UserSessionModel UserSession { get; set; }
    }
}