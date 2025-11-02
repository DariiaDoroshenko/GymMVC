using System;
using System.Collections.Generic;

namespace GymDomain.Model;

public partial class Client : Entity
{
   

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? PasswordHash { get; set; }

    public string? Phone { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? PhotoUrl { get; set; }

    public string IdentityUserId { get; set; } = null!;


    public virtual ICollection<TrainingRegistration> TrainingRegistrations { get; set; } = new List<TrainingRegistration>();
}
