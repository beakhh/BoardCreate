using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models
{
    [Table("UserPreferences", Schema = "UserInfo")]
    public class UserPreferencesModel
    {
        [Key]
        public int IDX { get; set; }
        public string UserID { get; set; }
        public int BoardIDX { get; set; }
        public DateTime FirstVisitedDate { get; set; }
        public DateTime LastVisitedDate { get; set; }
        public int? LikeStatus { get; set; } // NULL 처리 가능
        public int FavoriteStatus { get; set; }
    }

}
