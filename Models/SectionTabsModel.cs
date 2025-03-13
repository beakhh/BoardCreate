using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models
{
    [Table("SectionTabs", Schema = "Board")]
    public class SectionTabsModel
    {
        [Key]
        public int IDX { get; set; }
        public string TabName { get; set; }
        public int SectionIDX { get; set; }
        public int TabStatus { get; set; }

    }
}
