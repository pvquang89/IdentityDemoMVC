using IdentityDemo.Models.Claims;

namespace IdentityDemo.Models
{
    public class UserClaimsViewModel
    {
        public UserClaimsViewModel()
        {
            Claims = new List<UserClaim>(); 
        }

        public string UserId { get; set; }
        public List<UserClaim> Claims { get; set; }
    }
}
