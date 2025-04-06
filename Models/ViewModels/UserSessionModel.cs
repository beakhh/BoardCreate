namespace BoardCreate.Models.ViewModels
{
    public class UserSessionModel
    {
        public string NickName { get; set; }
        public string UserID { get; set; } = "guest";
        public int IDX { get; set; }
        public int UserLevel { get; set; }
    }
}