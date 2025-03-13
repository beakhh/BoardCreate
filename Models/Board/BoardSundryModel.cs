using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.Board
{
    public class BoardSundryModel
    {
        public int SectionIDX { get; set; }
        public string SelectedTab { get; set; } = "전체";
        public int PageSize { get; set; } = 5;
        public int CurrentPage { get; set; } = 1;
        public int PageTotalCount { get; set; } = 1;
        public string UserID { get; set; }
        public int UserLevel { get; set; }
    }
}
