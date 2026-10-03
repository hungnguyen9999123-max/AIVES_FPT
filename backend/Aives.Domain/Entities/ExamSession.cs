using System;
using System.Collections.Generic;

using Aives.Domain.Enums;

namespace Aives.Domain.Entities;

public partial class ExamSession
{
    public int SessionId { get; set; }

    public int ExamId { get; set; }

    public int ScheduledBy { get; set; }

    public string SessionName { get; set; } = null!;

    public string? Room { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public SessionStatus? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Exam Exam { get; set; } = null!;

    public virtual ICollection<Result> Results { get; set; } = new List<Result>();

    public virtual User ScheduledByNavigation { get; set; } = null!;

    public virtual ICollection<SessionEnrollment> SessionEnrollments { get; set; } = new List<SessionEnrollment>();
}
