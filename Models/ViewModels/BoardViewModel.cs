using BoardCreate.Models.Board;

namespace BoardCreate.Models.ViewModels
{
    public class BoardViewModel
    {
        public List<SectionTabsModel> BoardTabs { get; set; }
        public List<BoardModel> BoardLists { get; set; }
        public BoardSundryModel BoardSundry { get; set; }
        public string UserID { get; set; } = "Guest";

    }
}