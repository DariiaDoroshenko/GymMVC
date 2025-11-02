using Microsoft.AspNetCore.Identity;

namespace GymDomain.Models
{
    public class User : IdentityUser
    {
        public string Email { get; set; }
    }
}
