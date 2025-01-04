using System.ComponentModel.DataAnnotations;

namespace IdentityDemo.Models
{
    public class CreateRoleViewModel
    {
        [Required]
        [Display(Name = "Role")]
        public string RoleName { get; set; }
    }
}
