using IAutoM8.Domain.Models.User;
using IAutoM8.Global.Options;
using IAutoM8.Repository.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;

namespace IAutoM8.Repository
{
    public class Context
        :
        IdentityDbContext<
            User,
            Role,
            Guid,
            IdentityUserClaim<Guid>,
            UserRole, IdentityUserLogin<Guid>,
            IdentityRoleClaim<Guid>,
            IdentityUserToken<Guid>>
    {
        public DbOptions Options { get; }

        public Context(DbOptions options)
        {
            Options = options;
        }

        public Context(IOptions<DbOptions> options) : this(options.Value) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cnnStr = !Options.ConnectionString.EndsWith(";")
                ? Options.ConnectionString + ";"
                : Options.ConnectionString;

            optionsBuilder.EnableSensitiveDataLogging(true);
            optionsBuilder.UseSqlServer(cnnStr + "MultipleActiveResultSets=True;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            ContextModelBuilder.Configure(builder);
        }
    }
}
