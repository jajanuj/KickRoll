using Google.Cloud.Firestore;
using KickRoll.Api.Models;

namespace KickRoll.Api.Services;

public interface IAuditService
{
    Task LogAsync(string action, string targetType, string targetId, string actorUserId, Dictionary<string, object>? payload = null, string? ip = null);
    Task<List<AuditLogResponse>> GetAuditLogsAsync(string? actorUserId = null, string? targetType = null, string? targetId = null, DateTime? from = null, DateTime? to = null, int limit = 100);
}

public class AuditService : IAuditService
{
    private readonly FirestoreDb _firestore;
    private readonly ILogger<AuditService> _logger;

    public AuditService(FirestoreDb firestore, ILogger<AuditService> logger)
    {
        _firestore = firestore;
        _logger = logger;
    }

    public async Task LogAsync(string action, string targetType, string targetId, string actorUserId, Dictionary<string, object>? payload = null, string? ip = null)
    {
        try
        {
            var auditLog = new AuditLog
            {
                ActorUserId = actorUserId,
                Action = action,
                TargetType = targetType,
                TargetId = targetId,
                Payload = payload,
                At = Timestamp.GetCurrentTimestamp(),
                Ip = ip
            };

            var logRef = _firestore.Collection("auditLogs").Document();
            auditLog.LogId = logRef.Id;
            
            await logRef.SetAsync(auditLog);
            
            _logger.LogInformation($"Audit log created: {action} on {targetType}/{targetId} by {actorUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to create audit log for action {action}");
            // Don't throw - audit logging shouldn't fail the main operation
        }
    }

    public async Task<List<AuditLogResponse>> GetAuditLogsAsync(string? actorUserId = null, string? targetType = null, string? targetId = null, DateTime? from = null, DateTime? to = null, int limit = 100)
    {
        try
        {
            Query query = _firestore.Collection("auditLogs");

            if (!string.IsNullOrEmpty(actorUserId))
                query = query.WhereEqualTo("ActorUserId", actorUserId);

            if (!string.IsNullOrEmpty(targetType))
                query = query.WhereEqualTo("TargetType", targetType);

            if (!string.IsNullOrEmpty(targetId))
                query = query.WhereEqualTo("TargetId", targetId);

            if (from.HasValue)
                query = query.WhereGreaterThanOrEqualTo("At", Timestamp.FromDateTime(from.Value.ToUniversalTime()));

            if (to.HasValue)
                query = query.WhereLessThanOrEqualTo("At", Timestamp.FromDateTime(to.Value.ToUniversalTime()));

            query = query.OrderByDescending("At").Limit(limit);

            var snapshot = await query.GetSnapshotAsync();
            var logs = new List<AuditLogResponse>();

            foreach (var doc in snapshot.Documents)
            {
                var log = doc.ConvertTo<AuditLog>();
                logs.Add(new AuditLogResponse
                {
                    LogId = log.LogId,
                    ActorUserId = log.ActorUserId,
                    Action = log.Action,
                    TargetType = log.TargetType,
                    TargetId = log.TargetId,
                    Payload = log.Payload,
                    At = log.At.ToDateTime(),
                    Ip = log.Ip
                });
            }

            return logs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve audit logs");
            return new List<AuditLogResponse>();
        }
    }
}