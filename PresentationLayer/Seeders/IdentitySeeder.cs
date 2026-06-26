using Microsoft.AspNetCore.Identity;

namespace PresentationLayer.Seeders
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // 1. Tạo tất cả Role
            string[] roleNames = { "Admin", "Teacher", "Student" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // 2. Seed tài khoản Admin
            var adminEmail = "admin@chatbot.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Seed tài khoản Giáo viên mẫu
            var teacherEmail = "teacher@test.com";
            var teacherUser = await userManager.FindByEmailAsync(teacherEmail);
            if (teacherUser == null)
            {
                teacherUser = new IdentityUser
                {
                    UserName = teacherEmail,
                    Email = teacherEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(teacherUser, "Teacher@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
            }

            // 4. Seed tài khoản Sinh viên mẫu
            var studentEmail = "student@test.com";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            if (studentUser == null)
            {
                studentUser = new IdentityUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(studentUser, "Student@123");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(studentUser, "Student");
            }
        }
    }
}
