using System.Text.Json;
using Legend2Toolbox.Application.Common.Models;
using Legend2Toolbox.Domain.Entities.Audit;

namespace Legend2Toolbox.Api.IntegrationTests.Auditing;

public class AuditEndpointTests
{
    [Fact]
    public async Task Login_ShouldRecordEveryAttempt_WithTimeIpAndNoCredentials()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("loginuser");
        host.Client.DefaultRequestHeaders.Add("X-Forwarded-For", "198.51.100.99");
        var before = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var success = await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.UserName!, AuditTestHost.Password));
        success.StatusCode.Should().Be(HttpStatusCode.OK);
        var response = await success.Content.ReadFromJsonAsync<AuthResponse>();
        var failure = await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.UserName!, "wrong-secret"));
        failure.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var logs = await host.LogsAsync();
        logs.Should().HaveCount(2);
        logs.Should().OnlyContain(x => x.Action == "Login" && x.SubjectUserId == user.Id &&
            x.ClientIp == "203.0.113.25" && x.OccurredAtUnixMs >= before && !x.CaptureIncomplete);
        logs.Single(x => x.Outcome == "Succeeded").ActorUserId.Should().Be(user.Id);
        logs.Single(x => x.Outcome == "Failed").ActorUserId.Should().BeNull();
        var json = JsonSerializer.Serialize(logs);
        json.Should().NotContain(AuditTestHost.Password).And.NotContain("wrong-secret")
            .And.NotContain(response!.AccessToken).And.NotContain(response.RefreshToken);
        using var client = await host.AsUserAsync(user);
        var mine = await client.GetFromJsonAsync<PagedResult<AuditLog>>("/api/audit/?action=Login");
        mine!.TotalCount.Should().Be(2);
        foreach (var loginLog in mine.Items)
        {
            var detailResponse = await client.GetAsync($"/api/audit/{loginLog.Id}");
            detailResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
            loginLog.Details.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task PersonalListAndDetail_ShouldNeverExposeOtherUsersRecords()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var alice = await host.CreateUserAsync("alice");
        var bob = await host.CreateUserAsync("bob");
        var admin = await host.CreateUserAsync("auditadmin", "SuperAdmin");
        var aliceRecord = new AuditLog
        {
            ActorUserId = alice.Id, Module = "ScriptSet", Action = "Update",
            Details = [new AuditLogDetail { Sequence = 1, OldValues = "{\"name\":\"before\"}" }]
        };
        var bobRecord = new AuditLog { ActorUserId = bob.Id, Module = "User", Action = "Update" };
        var targetedAtAlice = new AuditLog { ActorUserId = admin.Id, TargetId = alice.Id.ToString(), Module = "User", Action = "Update" };
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AuditLogs.AddRange(aliceRecord, bobRecord, targetedAtAlice);
            await db.SaveChangesAsync();
        }
        using var client = await host.AsUserAsync(alice);
        var mine = await client.GetFromJsonAsync<PagedResult<AuditLog>>("/api/audit/");
        mine!.Items.Should().ContainSingle(x => x.Id == aliceRecord.Id);
        mine.Items.Single().Details.Should().BeEmpty();
        var forgedFilter = await client.GetFromJsonAsync<PagedResult<AuditLog>>($"/api/audit/?userId={bob.Id}");
        forgedFilter!.TotalCount.Should().Be(0);
        (await client.GetAsync($"/api/audit/{aliceRecord.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync($"/api/audit/{bobRecord.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync($"/api/audit/{targetedAtAlice.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync("/api/admin/audit/")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        (await client.GetAsync($"/api/admin/audit/{bobRecord.Id}")).StatusCode.Should().Be(HttpStatusCode.Forbidden);
        using var adminClient = await host.AsUserAsync(admin);
        var all = await adminClient.GetFromJsonAsync<PagedResult<AuditLog>>("/api/admin/audit/");
        all!.TotalCount.Should().Be(3);
        var byId = await adminClient.GetFromJsonAsync<PagedResult<AuditLog>>($"/api/admin/audit/?userId={alice.Id:D}");
        byId!.Items.Should().ContainSingle(x => x.Id == aliceRecord.Id);
        (await adminClient.GetAsync($"/api/admin/audit/{bobRecord.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await adminClient.GetFromJsonAsync<AuditLog>($"/api/admin/audit/{aliceRecord.Id}");
        detail!.Details.Should().ContainSingle(x => x.OldValues.Contains("before"));
        (await adminClient.GetAsync($"/api/audit/{targetedAtAlice.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await adminClient.GetAsync($"/api/audit/{aliceRecord.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await host.Client.GetAsync("/api/audit/")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await host.LogsAsync()).Should().HaveCount(3, "audit queries must not create more audits");
    }

    [Fact]
    public async Task AccountFilter_ShouldMatchNamesAndIds_WithoutWideningPersonalAccess()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var alice = await host.CreateUserAsync("filteralice");
        var bob = await host.CreateUserAsync("filterbob");
        var admin = await host.CreateUserAsync("filteradmin", "SuperAdmin");
        var operation = new AuditLog { ActorUserId = alice.Id, ActorUserName = "oldalice", Module = "ScriptSet", Action = "Update" };
        var failedLogin = new AuditLog { SubjectUserId = alice.Id, TargetName = alice.UserName, Module = "Auth", Action = "Login", Outcome = "Failed" };
        var unrelatedTarget = new AuditLog { ActorUserId = bob.Id, ActorUserName = bob.UserName, TargetName = alice.UserName, Module = "ScriptSet", Action = "Update" };
        var deletedUser = new AuditLog { ActorUserId = Guid.NewGuid(), ActorUserName = "deletedaccount", Module = "User", Action = "Update" };
        var unknownLogin = new AuditLog { TargetName = "unknownaccount", Module = "Auth", Action = "Login", Outcome = "Failed" };
        await using (var scope = host.App.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.AuditLogs.AddRange(operation, failedLogin, unrelatedTarget, deletedUser, unknownLogin);
            await db.SaveChangesAsync();
        }
        using var client = await host.AsUserAsync(admin);
        foreach (var value in new[] { "filteralice", "  FILTERALICE  ", alice.Id.ToString("D"), alice.Id.ToString("N"), alice.Id.ToString().ToUpperInvariant() })
        {
            var page = await client.GetFromJsonAsync<PagedResult<AuditLog>>(
                $"/api/admin/audit/?account={Uri.EscapeDataString(value)}");
            page!.Items.Select(x => x.Id).Should().BeEquivalentTo([operation.Id, failedLogin.Id]);
        }
        foreach (var (name, expected) in new[] { ("oldalice", operation.Id), ("DELETEDACCOUNT", deletedUser.Id), ("unknownaccount", unknownLogin.Id) })
        {
            var page = await client.GetFromJsonAsync<PagedResult<AuditLog>>($"/api/admin/audit/?account={name}");
            page!.Items.Should().ContainSingle(x => x.Id == expected);
        }
        var missing = await client.GetFromJsonAsync<PagedResult<AuditLog>>("/api/admin/audit/?account=filteralic");
        missing!.TotalCount.Should().Be(0, "account names use exact matching");
        var combined = await client.GetFromJsonAsync<PagedResult<AuditLog>>($"/api/admin/audit/?account=filteralice&userId={bob.Id}");
        combined!.TotalCount.Should().Be(0);
        using var personal = await host.AsUserAsync(bob);
        var forged = await personal.GetFromJsonAsync<PagedResult<AuditLog>>("/api/audit/?account=filteralice");
        forged!.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Register_ShouldCaptureCommittedChanges_AndPreserveMembershipAndLogin()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var response = await host.Client.PostAsJsonAsync("/api/auth/register",
            new RegisterRequest("registered", "registered@example.test", AuditTestHost.Password));
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var log = (await host.LogsAsync()).Should().ContainSingle().Subject;
        log.Outcome.Should().Be("Succeeded");
        log.DataStatus.Should().Be("Committed");
        log.CaptureIncomplete.Should().BeFalse();
        log.ActorUserId.Should().NotBeNull();
        log.Details.Should().Contain(x => x.EntityType == "ApplicationUser" && x.ChangeType == "Added");
        log.Details.Should().Contain(x => x.EntityType == "UserMembership");
        JsonSerializer.Serialize(log).Should().NotContain(AuditTestHost.Password);
        var login = await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest("registered", AuditTestHost.Password));
        login.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task QueriesAndTokenRefresh_ShouldNotCreateAudits()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("queryuser");
        var login = await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.UserName!, AuditTestHost.Password));
        var token = await login.Content.ReadFromJsonAsync<AuthResponse>();
        using var client = await host.AsUserAsync(user);
        (await client.GetAsync("/api/auth/userinfo")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.GetAsync("/api/scripts/sets?pageNumber=1&pageSize=10")).StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.PostAsJsonAsync("/api/auth/refresh", new RefreshTokenRequest(token!.RefreshToken)))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await host.LogsAsync()).Should().ContainSingle(x => x.Action == "Login");
    }

    [Fact]
    public async Task ScriptCrud_ShouldRecordBeforeAfterAndKeepHistoryAfterDeletion()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("scriptuser");
        using var client = await host.AsUserAsync(user);
        var create = await client.PostAsJsonAsync("/api/scripts/sets/", new { name = "Before", description = "description" });
        create.StatusCode.Should().Be(HttpStatusCode.OK);
        var id = await create.Content.ReadFromJsonAsync<Guid>();
        (await client.PutAsJsonAsync($"/api/scripts/sets/{id}", new { name = "After", description = "description" }))
            .StatusCode.Should().Be(HttpStatusCode.OK);
        (await client.DeleteAsync($"/api/scripts/sets/{id}")).StatusCode.Should().Be(HttpStatusCode.OK);
        var logs = await host.LogsAsync();
        logs.Should().HaveCount(3).And.OnlyContain(x => x.ActorUserId == user.Id && x.DataStatus == "Committed");
        var changed = logs.Single(x => x.Action == "Update").Details.Should().ContainSingle().Subject;
        changed.OldValues.Should().Contain("Before").And.NotContain("description");
        changed.NewValues.Should().Contain("After");
        logs.Single(x => x.Action == "Delete").Details.Should().ContainSingle(x => x.OldValues.Contains("After"));
    }

    [Theory]
    [InlineData("pageSize=101")]
    [InlineData("pageNumber=0")]
    [InlineData("pageNumber=2147483647&pageSize=100")]
    [InlineData("from=2026-10-01T00:00:00Z&to=2026-09-01T00:00:00Z")]
    public async Task Query_ShouldRejectInvalidPaginationAndDates(string query)
    {
        await using var host = await AuditTestHost.CreateAsync();
        using var client = await host.AsUserAsync(await host.CreateUserAsync("filteruser"));
        (await client.GetAsync("/api/audit/?" + query)).StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ConcurrentRequests_ShouldKeepOperatorsAndDetailsSeparate()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var first = await host.CreateUserAsync("concurrentone");
        var second = await host.CreateUserAsync("concurrenttwo");
        using var firstClient = await host.AsUserAsync(first);
        using var secondClient = await host.AsUserAsync(second);
        var results = await Task.WhenAll(
            firstClient.PostAsJsonAsync("/api/scripts/sets/", new { name = "First user's set" }),
            secondClient.PostAsJsonAsync("/api/scripts/sets/", new { name = "Second user's set" }));
        results.Should().OnlyContain(x => x.StatusCode == HttpStatusCode.OK);
        var logs = await host.LogsAsync();
        logs.Should().HaveCount(2);
        logs.Single(x => x.ActorUserId == first.Id).Details.Should().ContainSingle(x => x.NewValues.Contains("First user"));
        logs.Single(x => x.ActorUserId == second.Id).Details.Should().ContainSingle(x => x.NewValues.Contains("Second user"));
    }

    [Fact]
    public async Task LoginFailures_ShouldPreserveLockoutRules()
    {
        await using var host = await AuditTestHost.CreateAsync();
        var user = await host.CreateUserAsync("lockoutuser");
        for (var i = 0; i < 5; i++)
            (await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.UserName!, "incorrect")))
                .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await host.Client.PostAsJsonAsync("/api/auth/login", new LoginRequest(user.UserName!, AuditTestHost.Password)))
            .StatusCode.Should().Be(HttpStatusCode.BadRequest);
        await using var scope = host.App.Services.CreateAsyncScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var reloaded = await manager.FindByIdAsync(user.Id.ToString());
        (await manager.IsLockedOutAsync(reloaded!)).Should().BeTrue();
        (await host.LogsAsync()).Should().HaveCount(6).And.OnlyContain(x => x.Outcome == "Failed");
    }
}
