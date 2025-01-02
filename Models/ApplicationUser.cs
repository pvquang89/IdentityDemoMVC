using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    //class mở rộng của IdentityUser
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
