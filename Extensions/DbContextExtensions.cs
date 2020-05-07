using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EZms.Core.Enums;
using EZms.Core.Helpers;
using EZms.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;

namespace EZms.Core.Extensions
{
    public static class DbContextExtensions
    {
        public static bool AllMigrationsApplied(this DbContext context)
        {
            var applied = context.GetService<IHistoryRepository>()
                .GetAppliedMigrations()
                .Select(m => m.MigrationId);

            var total = context.GetService<IMigrationsAssembly>()
                .Migrations
                .Select(m => m.Key);

            return !total.Except(applied).Any();
        }

        public static void EnsureSeeded(this EZmsContext context, string basePath)
        {
            if (!context.Content.Any())
            {
                var types = JsonConvert.DeserializeObject<List<Content>>(
                    File.ReadAllTextAsync($"{basePath}{Path.DirectorySeparatorChar}seed{Path.DirectorySeparatorChar}contents.json").GetAwaiter().GetResult()
                );
                context.AddRange(types);
                context.SaveChanges();
            }

            if (!context.Roles.Any())
            {
                var roles = JsonConvert.DeserializeObject<List<IdentityRole>>(
                    File.ReadAllTextAsync($"{basePath}{Path.DirectorySeparatorChar}seed{Path.DirectorySeparatorChar}roles.json").GetAwaiter().GetResult()
                );
                context.AddRange(roles);
                context.SaveChanges();
            }

            if (!context.Users.Any())
            {
                var users = JsonConvert.DeserializeObject<List<IdentityUser>>(
                    File.ReadAllTextAsync($"{basePath}{Path.DirectorySeparatorChar}seed{Path.DirectorySeparatorChar}users.json").GetAwaiter().GetResult()
                );
                context.AddRange(users);
                context.SaveChanges();

                var userManager = ServiceLocator.Current.GetInstance<UserManager<IdentityUser>>();
                var user = userManager.FindByIdAsync(users.First().Id).GetAwaiter().GetResult();
                var token = userManager.GeneratePasswordResetTokenAsync(user).GetAwaiter().GetResult();
                var result = userManager.ResetPasswordAsync(user, token, "EZmsAdmin2019!").GetAwaiter().GetResult();
            }

            if (!context.UserRoles.Any())
            {
                var userRoles = JsonConvert.DeserializeObject<List<IdentityUserRole<string>>>(
                    File.ReadAllTextAsync($"{basePath}{Path.DirectorySeparatorChar}seed{Path.DirectorySeparatorChar}userroles.json").GetAwaiter().GetResult()
                );
                context.AddRange(userRoles);
                context.SaveChanges();
            }
        }
    }
}
