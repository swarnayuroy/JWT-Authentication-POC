using DataContext.DataProvider;
using DataContext.DataService;
using DataContext.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContext.SampleData
{
    public class AccountData : IDataProvider, IDataService
    {
        private static IList<User> _userList = new List<User>()
        {
            new User
            {
                Id = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                IsVerfied = true
            },
            new User
            {
                Id = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                Name = "Jane Doe",
                Email = "doe.jane@gmail.com",
                IsVerfied = true
            },
            new User
            {
                Id = Guid.Parse("4d96c0ff-6f5e-4433-b7ed-46fa38974d79"),
                Name = "Max Miller",
                Email = "miller.max@outlook.com",
                IsVerfied = true
            },
            new User
            {
                Id = Guid.Parse("249af661-4046-48cc-b18c-aacc73de6bb0"),
                Name = "Dave Johnson",
                Email = "johnson.dave@gmail.com",
                IsVerfied = false
            },
            new User
            {
                Id = Guid.Parse("9ee3ea79-058a-47c6-a07d-a77b7a8f9856"),
                Name = "Sarah Knnox",
                Email = "knnox.sarah@yahoo.com",
                IsVerfied = false
            },
            new User
            {
                Id = Guid.Parse("36667530-8df9-467f-a8d9-7988d7babe90"),
                Name = "Aaron Knnox",
                Email = "knnox.aaron@yahoo.com",
                IsVerfied = false
            },
            new User
            {
                Id = Guid.Parse("e534ee67-4a15-4ea1-82eb-08f805718b31"),
                Name = "Bob Hardy",
                Email = "hardy.bob@gmail.com",
                IsVerfied = false
            },
            new User
            {
                Id = Guid.Parse("62f550f6-bf06-4e5f-815e-a51abe756719"),
                Name = "Joseph Croft",
                Email = "joseph.croft@gmail.com",
                IsVerfied = false
            }
        };
        private static IList<UserRole> _userRoles = new List<UserRole>()
        {
            new UserRole
            {
                UserId = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                Role = UserRoleType.Admin
            },
            new UserRole
            {
                UserId = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                Role = UserRoleType.Admin
            },
            new UserRole
            {
                UserId = Guid.Parse("4d96c0ff-6f5e-4433-b7ed-46fa38974d79"),
                Role = UserRoleType.User
            },
            new UserRole
            {
                UserId = Guid.Parse("249af661-4046-48cc-b18c-aacc73de6bb0"),
                Role = UserRoleType.User
            },
            new UserRole
            {
                UserId = Guid.Parse("9ee3ea79-058a-47c6-a07d-a77b7a8f9856"),
                Role = UserRoleType.User
            },
            new UserRole
            {
                UserId = Guid.Parse("36667530-8df9-467f-a8d9-7988d7babe90"),
                Role = UserRoleType.User
            },
            new UserRole
            {
                UserId = Guid.Parse("e534ee67-4a15-4ea1-82eb-08f805718b31"),
                Role = UserRoleType.User
            },
            new UserRole
            {
                UserId = Guid.Parse("62f550f6-bf06-4e5f-815e-a51abe756719"),
                Role = UserRoleType.User
            }
        };
        private static IList<Account> _accountsDetail = new List<Account>()
        {
            new Account
            {
                Id = Guid.Parse("48e283eb-8193-4de0-a025-e8dcb6bc678a"),
                UserId = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                Password = "TestJohn@1994",
                CreatedAt = DateTime.Parse("2025-04-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("50b26a44-c7f9-462a-9d4f-c66ac2e9938e"),
                UserId = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                Password = "TestJane@1994",
                CreatedAt = DateTime.Parse("2025-05-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("619778a8-e20a-4f1d-a52d-f5dc75a6bc21"),
                UserId = Guid.Parse("4d96c0ff-6f5e-4433-b7ed-46fa38974d79"),
                Password = "TestMiller@1995",
                CreatedAt = DateTime.Parse("2025-05-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("0d711e2c-ed04-4176-9790-b0cac8c98195"),
                UserId = Guid.Parse("249af661-4046-48cc-b18c-aacc73de6bb0"),
                Password = "TestDave@1995",
                CreatedAt = DateTime.Parse("2026-02-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("11b9f9f9-f172-46e5-b71c-9b20f47d98c4"),
                UserId = Guid.Parse("9ee3ea79-058a-47c6-a07d-a77b7a8f9856"),
                Password = "TestKnnox@1995",
                CreatedAt = DateTime.Parse("2026-02-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("14afbedf-2404-4c43-a03c-fdf8222e4096"),
                UserId = Guid.Parse("36667530-8df9-467f-a8d9-7988d7babe90"),
                Password = "TestAaron@1995",
                CreatedAt = DateTime.Parse("2026-03-10T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("8b19e468-9dee-47ab-aa8f-76fe5ef4b957"),
                UserId = Guid.Parse("e534ee67-4a15-4ea1-82eb-08f805718b31"),
                Password = "TestBob@1995",
                CreatedAt = DateTime.Parse("2026-03-01T10:15:30")
            },
            new Account
            {
                Id = Guid.Parse("eca5bfa3-2fcf-4cd6-85f2-2e978b25bab6"),
                UserId = Guid.Parse("62f550f6-bf06-4e5f-815e-a51abe756719"),
                Password = "TestJoseph@1995",
                CreatedAt = DateTime.Parse("2026-03-01T10:15:30")
            }
        };

        public IList<User> User { get { return _userList; } }
        public IList<UserRole> UserRole { get { return _userRoles; } }
        public IList<Account> Account { get { return _accountsDetail; } }

        public async Task SaveAccountAsync(Account accountDetail)
        {
            await Task.Run(() => _accountsDetail.Add(accountDetail));
        }
        public async Task SaveUserAsync(User userDetail)
        {
            await Task.Run(() => _userList.Add(userDetail));
        }
        public async Task SaveUserRoleAsync(UserRole userRole)
        {
            await Task.Run(() => _userRoles.Add(userRole));
        }

        public async Task<bool> UpdateUserAsync(User userDetail) 
        {
            await Task.Run(() => 
            _userList[_userList.IndexOf(_userList.First(user=>user.Id==userDetail.Id))] = userDetail
                );
            return true;
        }
        public async Task<bool> UpdateUserRoleAsync(UserRole userRole)
        {
            await Task.Run(() =>
            _userRoles[_userRoles.IndexOf(_userRoles.First(user => user.UserId == userRole.UserId))] = userRole
                );
            return true;
        }
        public async Task<bool> UpdateAccountAsync(Account accountDetail)
        {
            await Task.Run(() => 
            _accountsDetail[_accountsDetail.IndexOf(_accountsDetail.First(account=>account.UserId==accountDetail.UserId))] = accountDetail
                );
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            return await Task.Run(() =>
            {
                var user = _userList.FirstOrDefault(u => u.Id == userId);
                var userRoleDetail = _userRoles.FirstOrDefault(u => u.UserId == userId);
                if (user != null && userRoleDetail != null)
                {
                    bool isUserRemoved = _userList.Remove(user);
                    bool isUserRoleRemoved = _userRoles.Remove(userRoleDetail);

                    return isUserRemoved && isUserRoleRemoved;
                }
                return false;
            });
        }
        public async Task<bool> DeleteAccountAsync(Guid accountId)
        {
            return await Task.Run(() =>
            {
                var account = _accountsDetail.FirstOrDefault(a => a.Id == accountId);
                if (account != null)
                {
                    return _accountsDetail.Remove(account);
                }
                return false;
            });
        }
    }
}
