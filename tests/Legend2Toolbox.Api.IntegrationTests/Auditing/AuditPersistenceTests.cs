using System.Text.Json;
using Legend2Toolbox.Application.Behaviors;
using Legend2Toolbox.Application.Common.Interfaces;
using Legend2Toolbox.Application.Feature.Admin;
using Legend2Toolbox.Application.Feature.Scripts.MaterialFile;
using Legend2Toolbox.Domain.Entities.Audit;
using Legend2Toolbox.Domain.Entities.Cards;
using Legend2Toolbox.Domain.Entities.ScriptSets;
using Legend2Toolbox.Domain.Models;
using Legend2Toolbox.Infrastructure.Auditing;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Legend2Toolbox.Api.IntegrationTests.Auditing;

public class AuditPersistenceTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ExplicitTransaction_ShouldOnlyIncludeCommittedChanges(bool commit)
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("transactionuser");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
            await audit.ExecuteAsync(new AuditOperation("ScriptSet", "Create"), async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                db.ScriptSets.Add(ScriptSet.Create(user.Id, "first"));
                await db.SaveChangesAsync();
                db.ScriptSets.Add(ScriptSet.Create(user.Id, "second"));
                await db.SaveChangesAsync();
                if (commit) await transaction.CommitAsync();
                else await transaction.RollbackAsync();
                return commit ? Result.Success() : Result.Failure("rejected");
            });
        }
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        log.DataStatus.Should().Be(commit ? "Committed" : "RolledBack");
        log.Details.Should().HaveCount(commit ? 2 : 0);
        log.CaptureIncomplete.Should().BeFalse();
        await using var checkScope = host.App.Services.CreateAsyncScope();
        (await checkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>().ScriptSets.CountAsync())
            .Should().Be(commit ? 2 : 0);
    }

    [Fact]
    public async Task ImplicitRollback_ShouldNotLeaveSuccessfulDetails()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("rollbackuser");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await scope.ServiceProvider.GetRequiredService<IAuditService>().ExecuteAsync(
                new AuditOperation("ScriptSet", "Create"), async () =>
                {
                    await using var transaction = await db.Database.BeginTransactionAsync();
                    db.ScriptSets.Add(ScriptSet.Create(user.Id, "discarded"));
                    await db.SaveChangesAsync();
                    return Result.Failure("business rollback by disposal");
                });
        }
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        log.DataStatus.Should().Be("RolledBack");
        log.Details.Should().BeEmpty();
    }

    [Fact]
    public async Task FailureAfterCommit_ShouldPreserveBusinessExceptionAndCommittedDetails()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("notificationuser");
        var original = new InvalidOperationException("secret must not enter audit");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
            var caught = await Assert.ThrowsAsync<InvalidOperationException>(() => audit.ExecuteAsync<Result>(
                new AuditOperation("ScriptSet", "Create"), async () =>
                {
                    db.ScriptSets.Add(ScriptSet.Create(user.Id, "persisted"));
                    await db.SaveChangesAsync();
                    throw original;
                }));
            caught.Should().BeSameAs(original);
        }
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        log.Outcome.Should().Be("Failed");
        log.DataStatus.Should().Be("Committed");
        log.Details.Should().ContainSingle();
        JsonSerializer.Serialize(log).Should().NotContain(original.Message);
    }

    [Fact]
    public async Task SoftDeleteAndPhysicalCascade_ShouldKeepSnapshotsWithoutSecrets()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("deleteduser");
        var set = ScriptSet.Create(user.Id, "parent");
        var file = ScriptFile.Create(set.Id, "script.txt", "scripts/script.txt", ScriptFileType.Whole, "embedded-secret");
        var card = CardNumber.Create("owner", 30, 100, 50, "secret-cdk", user.Id, user.UserName!);
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AddRange(set, file, card);
            await db.SaveChangesAsync();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
            await audit.ExecuteAsync(new AuditOperation("CardNumber", "SoftDelete"), async () =>
            {
                card.Remove(user.UserName!);
                await db.SaveChangesAsync();
                return Result.Success();
            });
            db.ChangeTracker.Clear();
            await audit.ExecuteAsync(new AuditOperation("ScriptSet", "Delete"), async () =>
            {
                var entity = await db.ScriptSets.SingleAsync(x => x.Id == set.Id);
                db.ScriptSets.Remove(entity);
                await db.SaveChangesAsync();
                return Result.Success();
            });
            db.ChangeTracker.Clear();
            await audit.ExecuteAsync(new AuditOperation("User", "Delete"), async () =>
            {
                var entity = await db.Users.SingleAsync(x => x.Id == user.Id);
                db.Users.Remove(entity);
                await db.SaveChangesAsync();
                return Result.Success();
            });
        }
        var logs = await host.LogsAsync();
        logs.Should().HaveCount(3).And.OnlyContain(x => !x.CaptureIncomplete);
        logs.Single(x => x.Action == "SoftDelete").Details.Should().ContainSingle(x => x.ChangeType == "SoftDeleted");
        logs.Single(x => x.Module == "ScriptSet").Details.Should().Contain(x => x.EntityType == "ScriptFile" &&
            x.ChangeType == "CascadeDeleted" && x.OldValues.Contains("script.txt"));
        logs.Single(x => x.Module == "User").Details.Should().Contain(x => x.EntityType == "CardNumber",
            "cascade snapshots must include soft-deleted children hidden by query filters");
        JsonSerializer.Serialize(logs).Should().NotContain("embedded-secret").And.NotContain("secret-cdk");
    }

    [Fact]
    public async Task AuditDatabaseFailure_ShouldNotFailBusiness_AndRetryShouldBeIdempotent()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("retryuser");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE AuditLogs RENAME TO AuditLogsUnavailable");
            var response = await scope.ServiceProvider.GetRequiredService<IAuditService>().ExecuteAsync(
                new AuditOperation("ScriptSet", "Create"), async () =>
                {
                    db.ScriptSets.Add(ScriptSet.Create(user.Id, "still saved"));
                    await db.SaveChangesAsync();
                    return Result.Success();
                });
            response.IsSuccess.Should().BeTrue();
            (await db.ScriptSets.CountAsync()).Should().Be(1);
            Directory.GetFiles(host.SpoolDirectory, "*.json").Should().ContainSingle();
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE AuditLogsUnavailable RENAME TO AuditLogs");
        }
        var store = host.App.Services.GetRequiredService<AuditStore>();
        await store.RetryPendingAsync(CancellationToken.None);
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        await store.WriteAsync(log);
        await store.RetryPendingAsync(CancellationToken.None);
        (await host.LogsAsync()).Should().ContainSingle();
        Directory.GetFiles(host.SpoolDirectory, "*.json").Should().BeEmpty();
    }

    [Fact]
    public async Task NotesOnlyUpdate_ShouldRecordActualBeforeAndAfterValues()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("notesuser");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var card = CardNumber.Create("owner", 30, 100, 50, "secret-cdk", user.Id, user.UserName!, "before");
            db.CardNumbers.Add(card);
            await db.SaveChangesAsync();
            await scope.ServiceProvider.GetRequiredService<IAuditService>().ExecuteAsync(
                new AuditOperation("CardNumber", "Update"), async () =>
                {
                    card.Update(card.Owner, card.DurationInDays, card.FaceValue, card.Amount,
                        card.StartTime, "after", user.UserName!);
                    await db.SaveChangesAsync();
                    return Result.Success();
                });
        }
        var detail = (await host.LogsAsync()).Single().Details.Should().ContainSingle().Subject;
        detail.OldValues.Should().Be("{\"Notes\":\"before\"}");
        detail.NewValues.Should().Be("{\"Notes\":\"after\"}");
    }

    [Fact]
    public async Task Migration_ShouldAddOnlyAuditTables_AndRetainExistingUsers()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("migrationuser");
        await using var scope = host.App.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.ExecuteSqlRawAsync("DROP TABLE AuditLogDetails");
        await db.Database.ExecuteSqlRawAsync("DROP TABLE AuditLogs");
        var migrations = db.Database.GetMigrations().ToArray();
        var history = db.GetService<IHistoryRepository>();
        await db.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
        foreach (var migration in migrations.Where(x => !x.EndsWith("_AddAuditLogs")))
            await db.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(migration, "8.0.28")));
        await db.Database.MigrateAsync();
        (await db.Users.AnyAsync(x => x.Id == user.Id)).Should().BeTrue();
        db.AuditLogs.Add(new AuditLog { Module = "Test", Action = "Migration" });
        await db.SaveChangesAsync();
        (await db.AuditLogs.CountAsync()).Should().Be(1);
        (await db.Database.GetPendingMigrationsAsync()).Should().BeEmpty();
    }

    [Fact]
    public void ReadOperations_ShouldBeExcludedEvenWhenNamedCommandOrWritingInternalMetadata()
    {
        AuditOperations.Describe(new GetUserByNameCommand("someone")).Should().BeNull();
        AuditOperations.Describe(new GetMaterialFileContentQuery(Guid.NewGuid())).Should().BeNull();
        AuditOperations.Describe(new RefreshTokenCommand("secret-refresh-token")).Should().BeNull();
    }

    [Fact]
    public async Task CancellationAfterSave_ShouldStillPersistAuditAndPropagateCancellation()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("canceluser");
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var audit = scope.ServiceProvider.GetRequiredService<IAuditService>();
            using var cancelled = new CancellationTokenSource();
            await Assert.ThrowsAsync<OperationCanceledException>(() => audit.ExecuteAsync<Result>(
                new AuditOperation("ScriptSet", "Create"), async () =>
                {
                    db.ScriptSets.Add(ScriptSet.Create(user.Id, "saved before disconnect"));
                    await db.SaveChangesAsync();
                    cancelled.Cancel();
                    cancelled.Token.ThrowIfCancellationRequested();
                    return Result.Success();
                }));
        }
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        log.Outcome.Should().Be("Cancelled");
        log.DataStatus.Should().Be("Committed");
        log.Details.Should().ContainSingle();
    }

    [Theory]
    [InlineData("203.0.113.10", "198.51.100.10", "203.0.113.10")]
    [InlineData("10.0.0.2", "198.51.100.10", "198.51.100.10")]
    [InlineData("10.0.0.2", "198.51.100.10, 10.0.0.3", "198.51.100.10")]
    [InlineData("10.0.0.2", "192.0.2.99, 198.51.100.10", "198.51.100.10")]
    [InlineData("::ffff:203.0.113.10", "198.51.100.10", "203.0.113.10")]
    [InlineData("10.0.0.2", "invalid", "10.0.0.2")]
    public void IpResolution_ShouldTrustOnlyConfiguredProxyChain(string peer, string forwarded, string expected)
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
        context.Request.Headers["X-Forwarded-For"] = forwarded;
        var result = AuditIpResolver.Resolve(context, new AuditOptions { TrustedProxies = ["10.0.0.2", "10.0.0.3"] });
        result.ClientIp.Should().Be(expected);
        context.Connection.RemoteIpAddress.Should().Be(IPAddress.Parse(peer), "audit must not alter rate limiter inputs");
    }
}
