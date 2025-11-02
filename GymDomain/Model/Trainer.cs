using System;
using System.Collections.Generic;

namespace GymDomain.Model;

public partial class Trainer : Entity
{


    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? Specialization { get; set; }

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public string? PhotoUrl { get; set; }
    public string? IdentityUserId { get; set; }


    public virtual ICollection<Training> Training { get; set; } = new List<Training>();
}
