using BoardCreate.Models.Board;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.ViewModels
{
    public class BoardSectionsForIndexModel
    {
        public List<BoardSectionsModel> BoardSectionList { get; set; }
        public List<BoardModel> BoardList { get; set; }


    }
}
