using Microsoft.AspNetCore.Identity;

namespace GymInfrastructure.Models
{
    public class User : IdentityUser
    {
        public string Email { get; set; }
    }
}
