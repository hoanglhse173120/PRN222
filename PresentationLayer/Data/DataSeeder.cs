using Microsoft.AspNetCore.Identity;

namespace PresentationLayer.Data
{
    public static class DataSeeder
    {
        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();

            string[] roleNames = { "Teacher", "Student" };
            
            // 1. Tạo Role
            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. Tạo User Giáo viên
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
                var createPowerUser = await userManager.CreateAsync(teacherUser, "Teacher@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(teacherUser, "Teacher");
                }
            }

            // 3. Tạo User Sinh viên
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
                var createStudentUser = await userManager.CreateAsync(studentUser, "Student@123");
                if (createStudentUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
        }
    }
}
