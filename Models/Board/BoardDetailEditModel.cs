using BoardCreate.Models;
using BoardCreate.Models.ViewModels;

namespace BoardCreate.Models.Board
{
    public class BoardDetailEditModel
    {
        public BoardModel Board { get; set; }
        public List<SectionTabsModel> BoardTabs { get; set; }
        public UserSessionModel UserSession { get; set; }
    }
}
