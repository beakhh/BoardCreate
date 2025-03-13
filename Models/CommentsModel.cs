using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models
{
    [Table("Comments", Schema = "Board")]
    public class CommentsModel
    {
        [Key]
        public int IDX { get; set; }
        public int BoardIDX { get; set; }
        public int WriterIDX { get; set; }
        public string? WriterID { get; set; }
        public int TargetedCommentsIDX { get; set; }
        public int? TargetedUserIDX { get; set; }
        public string? TargetedID { get; set; }
        public string CommentsContent { get; set; }
        public DateTime CommentsCreatedAt { get; set; }
        public DateTime CommentsUpdatedAt { get; set; }
        public int CommentsStatus { get; set; }
        public int? LikeStatus { get; set; }
        public int LikeCount { get; set; }
        public int BadCount { get; set; }
        public int ReplyCount { get; set; }

    }
}
