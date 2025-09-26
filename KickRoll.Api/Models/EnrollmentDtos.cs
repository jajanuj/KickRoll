namespace KickRoll.Api.Models;

// Request DTOs (using camelCase for JSON)
public class EnrollmentRequest
{
    public string MemberId { get; set; } = default!;
}

// Response DTOs (will be serialized to camelCase JSON)
public class EnrollmentResponse
{
    public string Id { get; set; } = default!;
    public string MemberId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}

public class SessionEnrollmentResponse
{
    public string Id { get; set; } = default!;
    public string CourseId { get; set; } = default!;
    public string TeamId { get; set; } = default!;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public int Capacity { get; set; }
    public int EnrolledCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class MemberEnrollmentsResponse
{
    public string EnrollmentId { get; set; } = default!;
    public string SessionId { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public SessionEnrollmentResponse Session { get; set; } = default!;
}

// Custom exceptions
public class CapacityFullException : Exception
{
    public CapacityFullException() : base("Session capacity is full") { }
    public CapacityFullException(string message) : base(message) { }
}

public class EnrollmentWindowClosedException : Exception
{
    public EnrollmentWindowClosedException() : base("Enrollment window is closed") { }
    public EnrollmentWindowClosedException(string message) : base(message) { }
}

public class AlreadyEnrolledException : Exception
{
    public AlreadyEnrolledException() : base("Already enrolled in this session") { }
    public AlreadyEnrolledException(string message) : base(message) { }
}