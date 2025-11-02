using System;
using System.Collections.Generic;

namespace GymDomain.Model;

public partial class TrainingRegistration : Entity
{
 

    public int ClientId { get; set; }

    public int TrainingId { get; set; }

    public DateTime? RegistrationDate { get; set; }

    public string? Status { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual Training Training { get; set; } = null!;
}
