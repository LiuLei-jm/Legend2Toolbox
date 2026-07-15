using Legend2Toolbox.Domain.Enums;
using Legend2Toolbox.Infrastructure.Identity;
using Microsoft.Extensions.Logging;

namespace Legend2Toolbox.Infrastructure.Persistence;

public static class IdentityDataSeeder
{
    public static async Task SeedAsync(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        ILogger logger
        )
    {
        foreach (var roleName in Enum.GetNames(typeof(Roles)))
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                var roleResult = await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
                if (roleResult.Succeeded)
                {
                    logger.LogInformation("系统角色 {RoleName} 创建成功.", roleName);
                }
            }

        }
        const string adminUserName = "admin";
        const string adminEmail = "admin@legendtoolbox.com";
        const string adminPassword = "LegendAdmin@2026";

        if (await userManager.FindByNameAsync(adminUserName) == null)
        {
            var adminUser = new ApplicationUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(adminUser, adminPassword);

            if (createResult.Succeeded)
            {
                logger.LogInformation("超级管理员账户 {AdminUserName} 创建成功.", adminUserName);
                await userManager.AddToRoleAsync(adminUser, nameof(Roles.SuperAdmin));
                logger.LogInformation("已为账户 {AdminUserName} 授予 SuperAdmin 权限.", adminUserName);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("超级管理员账户创建失败: {Errors}", errors);
            }
        }
    }
}
