using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BoardCreate.Models.Member
{
    [Table("Member", Schema = "UserInfo")]
    public class MemberModel
    {
        [Key]
        public int IDX { get; set; }

        [Required(ErrorMessage = "아이디는 필수 입력 항목입니다.")]
        [RegularExpression("^[a-zA-Z0-9_]{4,20}$", ErrorMessage = "아이디는 4~20자의 영문자, 숫자, 언더바(_)만 허용됩니다.")]
        public string UserId { get; set; }
        public string? UserSalt { get; set; }

        [Required(ErrorMessage = "비밀번호는 필수 입력 항목입니다.")]
        [RegularExpression("^(?=.*[A-Za-z])(?=.*\\d)(?=.*[!@@#$%^&*])[A-Za-z\\d!@@#$%^&*]{7,20}$", ErrorMessage = "비밀번호는 7~20자이며, 문자, 숫자, 특수문자를 포함해야 합니다.")]
        public string UserPW { get; set; }

        [Required(ErrorMessage = "닉네임은 필수 입력 항목입니다.")]
        [RegularExpression("^[가-힣a-zA-Z0-9]{2,10}$", ErrorMessage = "닉네임은 한글, 영문자, 숫자로 2~10자여야 합니다.")]
        public string NickName { get; set; }

        public int Gender { get; set; }
        public string? Bio { get; set; }
        public string? Photo { get; set; }
        public int UserLevel { get; set; }
        public int ShareScope { get; set; }
        public bool UserDelete { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime LastDate { get; set; }
    }
}

