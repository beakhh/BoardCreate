using BoardCreate.Models;
using BoardCreate.Models.ViewModels;

namespace BoardCreate.Models.Board
{
    public class BoardDetailModel
    {
        public BoardModel Board { get; set; }
        public List<CommentsModel> CommentsList { get; set; }
        public UserPreferencesModel UserPreferences { get; set; }
        public UserSessionModel UserSession { get; set; }
    }
}
