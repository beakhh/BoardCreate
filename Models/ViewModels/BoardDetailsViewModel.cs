using BoardCreate.Models.Board;

namespace BoardCreate.Models.ViewModels
{
    public class BoardDetailsViewModel
    {
        
        public BoardSectionsModel BoardSections { get; set; }
        public List<SectionTabsModel> BoardTabs { get; set; }
        public List<BoardModel> BoardLists { get; set; }
    }
}