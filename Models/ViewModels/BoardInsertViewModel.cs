using BoardCreate.Models.Board;

namespace BoardCreate.Models.ViewModels
{
    public class BoardInsertViewModel
    {
        public string SessionNickName { get; set; }
        public string SessionUserID { get; set; }
        public int SessionLevel { get; set; }
        public BoardSectionsModel BoardSections { get; set; }
        public List<SectionTabsModel> BoardTabs { get; set; }
        public List<BoardModel> BoardLists { get; set; }
    }
}