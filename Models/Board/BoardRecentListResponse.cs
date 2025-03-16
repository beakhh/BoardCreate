using BoardCreate.Models;
using BoardCreate.Models.ViewModels;

namespace BoardCreate.Models.Board
{
    public class BoardRecentListResponse
    {
        public int isAvailable { get; set; }
        public List<BoardModel> DataRecontListEvery { get; set; } = new();
        public List<BoardModel> DataRecontListMy { get; set; } = new();
    }
}
