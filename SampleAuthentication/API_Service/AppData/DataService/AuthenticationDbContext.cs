using API_Service.Utils;
using DataContext.DataProvider;
using DataContext.DataService;
using DataContext.Models;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace API_Service.AppData.DataService
{
    public class AuthenticationDbContext: DbContext
    {
        public AuthenticationDbContext(DbContextOptions<AuthenticationDbContext> options) : base(options)
        {               
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //User model builder
            modelBuilder.Entity<User>(entity => {
                entity.ToTable("Users");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).IsRequired().HasMaxLength(30);

                entity.Property(e => e.Email).IsRequired();
                entity.HasIndex(e => e.Email).IsUnique();

                entity.Property(x => x.IsVerified).HasDefaultValue(false);
            });

            //Account model builder
            modelBuilder.Entity<Account>(entity => {
                entity.ToTable("Accounts");
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Password).IsRequired();

                entity.Property(x => x.CreatedAt).IsRequired();

                entity.Property(x => x.LoggedInAt).IsRequired(false);

                entity.HasIndex(x => x.UserId).IsUnique();
                
                entity.HasOne<User>()
                      .WithOne()
                      .HasForeignKey<Account>(user => user.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            //UserRole model builder
            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.ToTable("UserRoles");
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.Role)
                        .HasConversion<int>()
                        .IsRequired();

                entity.HasOne<User>()
                        .WithOne()
                        .HasForeignKey<UserRole>(user => user.UserId)
                        .OnDelete(DeleteBehavior.Cascade);
            });

            SeedData(modelBuilder);
        }
        private static void SeedData(ModelBuilder modelBuilder)
        {
            // Users
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                    Name = "John Doe",
                    Email = "doe.john@gmail.com",
                    IsVerified = true
                },
                new User
                {
                    Id = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                    Name = "Jane Doe",
                    Email = "doe.jane@gmail.com",
                    IsVerified = true
                }
            );

            // User Roles
            modelBuilder.Entity<UserRole>().HasData(
                new UserRole
                {
                    UserId = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                    Role = UserRoleType.Superadmin
                },
                new UserRole
                {
                    UserId = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                    Role = UserRoleType.Admin
                }
             );

            // Accounts
            modelBuilder.Entity<Account>().HasData(
                new Account
                {
                    Id = Guid.Parse("48e283eb-8193-4de0-a025-e8dcb6bc678a"),
                    UserId = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                    Password = "TestJohn@1994",
                    CreatedAt = new DateTime(2025, 4, 10, 10, 15, 30)
                },
                new Account
                {
                    Id = Guid.Parse("50b26a44-c7f9-462a-9d4f-c66ac2e9938e"),
                    UserId = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                    Password = "TestJane@1994",
                    CreatedAt = new DateTime(2025, 5, 10, 10, 15, 30)
                }
            );
        }
    }

    public class DataAccessLayer : IContextProvider, IContextService
    {
        private readonly AuthenticationDbContext _context;

        public DataAccessLayer(AuthenticationDbContext context)
        {
            this._context = context;
        }

        public IQueryable<User> User => _context.Users;

        public IQueryable<UserRole> UserRole => _context.UserRoles;

        public IQueryable<Account> Account => _context.Accounts;

        public Task SaveAccountAsync(Account accountDetail)
        {
            _context.Accounts.AddAsync(accountDetail);
            return Task.CompletedTask;
        }

        public Task SaveUserAsync(User userDetail)
        {
             _context.Users.AddAsync(userDetail);
            return Task.CompletedTask;
        }

        public Task SaveUserRoleAsync(UserRole userRole)
        {
            _context.UserRoles.AddAsync(userRole);
            return Task.CompletedTask;
        }

        public Task UpdateAccountAsync(Account accountDetail)
        {
            _context.Accounts.Update(accountDetail);
            return Task.CompletedTask;
        }

        public Task UpdateUserAsync(User userDetail)
        {
            _context.Users.Update(userDetail);
            return Task.CompletedTask;
        }

        public Task UpdateUserRoleAsync(UserRole userRole)
        {
            _context.UserRoles.Update(userRole);
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync(User user)
        {
            _context.Users.Remove(user);
            return Task.CompletedTask;
        }        
    }

    public class ExecuteContextTask : IUnitOfWork
    {
        private readonly LoggerService<ExecuteContextTask> _logger;
        private readonly AuthenticationDbContext _context;
        public ExecuteContextTask(ILogger<ExecuteContextTask> logger, AuthenticationDbContext context)
        {
            this._logger = new LoggerService<ExecuteContextTask>(logger);
            this._context = context;
        }
        public async Task<bool> ExecuteAndCommit(params Func<Task>[] operations)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            string operationNames = string.Join(", ", operations.Select(op => op.Method.Name));
            bool isExecutionSuccessful = false;
            try
            {
                foreach (var operation in operations)
                {
                    await operation();
                }

                int result = await _context.SaveChangesAsync();

                if (result > 0) {                    

                    _logger.LogDetails(LogType.INFO, $"Successfully executed {operationNames}.");
                }
                else
                {
                    _logger.LogDetails(LogType.WARNING, $"No changes saved for {operationNames}.");
                }

                isExecutionSuccessful = true;
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogDetails(LogType.ERROR, $"Error executing {operationNames}: {ex.Message}");
                await transaction.RollbackAsync();
            }
            return isExecutionSuccessful;
        }
    }
}