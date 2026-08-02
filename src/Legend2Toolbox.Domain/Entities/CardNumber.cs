using Legend2Toolbox.Domain.Common;
using System.Buffers;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace Legend2Toolbox.Domain.Entities;

public class CardNumber : AuditableEntity
{
    public string Owner { get; private set; } = string.Empty;
    public DateTimeOffset StartTime { get; private set; }
    public int DurationInDays { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public double FaceValue { get; private set; }
    public decimal Amount { get; private set; }
    public string Cdk { get; private set; } = string.Empty;
    public string Notes { get; private set; } = string.Empty;
    public bool IsExpiredNotificationSent { get; private set; }
    public DateTimeOffset? LastCheckedForConnection { get; private set; }

    public Guid UserId { get; private set; }

    public bool IsExpired => DateTimeOffset.UtcNow > EndTime;

    private CardNumber() { }
    public static CardNumber Create(string customerName, int durationInDays, double faceValue, decimal amount, string cdk, Guid userId, string createdBy, string? notes = null)
    {
        var startTime = DateTimeOffset.UtcNow;
        return new CardNumber
        {
            Id = Guid.NewGuid(),
            Owner = customerName,
            StartTime = startTime,
            DurationInDays = durationInDays,
            EndTime = startTime.AddDays(durationInDays),
            FaceValue = faceValue,
            Amount = amount,
            Cdk = cdk,
            Notes = notes ?? string.Empty,
            UserId = userId,
            CreatedOn = DateTimeOffset.UtcNow,
            CreatedBy = createdBy,
            IsExpiredNotificationSent = false
        };
    }
    public void Update(string owner, int durationInDays, double faceValue, decimal amount, DateTimeOffset startTime, string? notes, string modifiedBy)
    {
        Owner = owner;
        DurationInDays = durationInDays;
        FaceValue = faceValue;
        Amount = amount;
        StartTime = startTime;
        EndTime = startTime.AddDays(durationInDays);
        Notes = notes ?? string.Empty;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    public void Remove(string modifiedBy)
    {
        DurationInDays = 0;
        EndTime = StartTime.AddDays(0);
        IsDeleted = true;
        LastModifiedBy = modifiedBy;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    public void UpdateCdk(string newCdk)
    {
        Cdk = newCdk;
        LastModifiedOn = DateTimeOffset.UtcNow;
    }

    public bool IsActive()
    {
        var now = DateTimeOffset.UtcNow;
        return StartTime <= now && EndTime > now;
    }
    public TimeSpan GetRemainingTime()
    {
        return IsExpired ? TimeSpan.Zero : EndTime - DateTimeOffset.UtcNow;
    }
}
