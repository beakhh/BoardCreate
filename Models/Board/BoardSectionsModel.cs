using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.Board
{
    [Table("BoardSections", Schema = "Board")]
    public class BoardSectionsModel
    {
        [Key]
        public int IDX { get; set; }
        public string SectionName { get; set; }
        public int SectionStatus { get; set; }
        public int SectionOrder { get; set; }
        public DateTime SectionStartDate { get; set; }
        public DateTime? SectionEndDate { get; set; }
        public int BoardIDX { get; set; }  = 0;
        public string Title { get; set; } = string.Empty;
        public DateTime BoardCreatedAt { get; set; }
        public int CommentCount { get; set; } = 0;

    }
}
