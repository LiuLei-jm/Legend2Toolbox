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

        if (await userManager.FindByNameAsync(AdminInfo.AdminUserName) == null)
        {
            var userId = Guid.NewGuid();
            var adminUser = new ApplicationUser
            {
                Id = userId,
                UserName = AdminInfo.AdminUserName,
                Email = AdminInfo.AdminEmail,
                EmailConfirmed = true,
                IsActive = true
            };

            adminUser.SecurityKey = SecurityKey.Create(userId, adminUser.UserName);
            adminUser.CardNumberPath = CardNumberPath.Create(userId);

            var createResult = await userManager.CreateAsync(adminUser, AdminInfo.AdminPassword);

            if (createResult.Succeeded)
            {
                logger.LogInformation("超级管理员账户 {AdminUserName} 创建成功.", AdminInfo.AdminUserName);
                await userManager.AddToRoleAsync(adminUser, nameof(Roles.SuperAdmin));
                logger.LogInformation("已为账户 {AdminUserName} 授予 SuperAdmin 权限.", AdminInfo.AdminUserName);
            }
            else
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                logger.LogError("超级管理员账户创建失败: {Errors}", errors);
            }
        }
    }
}
