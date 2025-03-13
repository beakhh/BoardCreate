using BoardCreate.Models.Board;

namespace BoardCreate.Models.ViewModels
{
    public class BoardViewModel
    {
        public List<SectionTabsModel> BoardTabs { get; set; }
        public List<BoardModel> BoardLists { get; set; }
        public BoardSundryModel BoardSundry { get; set; }
        public int SectionIDX { get; set; }
        public string SelectedTab { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int PageTotalCount { get; set; }
        
    }
}