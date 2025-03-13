using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.Board
{
    [Table("Board", Schema = "Board")]
    public class BoardModel
    {
        [Key]
        public int IDX { get; set; }
        public int SectionIDX { get; set; }
        public string Tab { get; set; }
        public string UserID { get; set; }
        public string NickName { get; set; }
        public string Title { get; set; }
        public string Contents { get; set; }
        public int BoardPrivate { get; set; }
        public int ViewCount { get; set; }
        public DateTime BoardCreatedAt { get; set; }
        public DateTime? BoardUpdatedAt { get; set; }
        public int BoardStatus { get; set; }
        public int UserIDX { get; set; }
        public string SectionName { get; set; }
        public int BoardLikeCount { get; set; }
        public int UserExists { get; set; } = 0;
        public int AdjustedIDX { get; set; }
    }
}
