using System;
using System.Collections.Generic;

namespace GymDomain.Model;

public partial class Training : Entity
{


    public int TrainerId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateOnly Date { get; set; }

    public TimeOnly StartTime { get; set; }

    public int? DurationMinutes { get; set; }

    public decimal? Price { get; set; }

    public int? MaxClients { get; set; }

    public string? PhotoUrl { get; set; }

    public bool? IsCanceled { get; set; }

    public virtual Trainer Trainer { get; set; } = null!;

    public virtual ICollection<TrainingRegistration> TrainingRegistrations { get; set; } = new List<TrainingRegistration>();
  
}
