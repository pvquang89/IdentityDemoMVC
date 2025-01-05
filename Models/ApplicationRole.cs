using Microsoft.AspNetCore.Identity;

namespace IdentityDemo.Models
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
    }
}
