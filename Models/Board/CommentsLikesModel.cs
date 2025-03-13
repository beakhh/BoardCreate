using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.Board
{
    [Table("CommentLikes", Schema = "Board")]
    public class CommentsLikesModel
    {
        [Key]
        public int IDX { get; set; }
        public int CommentsIDX { get; set; }
        public string UserID { get; set; }
        public int LikeStatus { get; set; }
        public DateTime LikeUpdateAt { get; set; }

        public int ProgressStatus { get; set; }
        public int LikeCount { get; set; }
        public int BadCount { get; set; }
    }
}
