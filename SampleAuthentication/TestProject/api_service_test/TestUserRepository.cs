using API_Service.AppData;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Repository;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.api_service_test
{
    [TestFixture]
    public class TestUserRepository
    {
        private Mock<ILogger<UserRepository>> _loggerMock;
        private Mock<IService<User>> _userServiceMock;

        private UserRepository _repository;

        [SetUp]
        public void Setup()
        {
            _userServiceMock = new Mock<IService<User>>();
            _loggerMock = new Mock<ILogger<UserRepository>>();

            _repository = new UserRepository(
                _loggerMock.Object,
                _userServiceMock.Object
            );
        }

        #region GetAllUsersAsync
        [Test]
        public async Task GetAllUsersAsync_ReturnsFalse_WhenNoUsersFound()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>());

            // Act
            var result = await _repository.GetAllUsersAsync(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5);

            // Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("No users found"));
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsSingleUser_WhenOneUserExists()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
                new User
                {
                    Id = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                    Name = "John Doe",
                    Email = "doe.john@gmail.com",
                    IsVerified = true
                }
            });

            // Act
            var result = await _repository.GetAllUsersAsync(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5);
            var dataResult = result as ResponseDataDetail<PagedResult<UserDetail>>;
            var userDetail = dataResult!.Data.Items.First();

            // Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo("1 user fetched successfully"));
            Assert.That(result, Is.TypeOf<ResponseDataDetail<PagedResult<UserDetail>>>());
            Assert.That(dataResult.Data.ItemCount, Is.EqualTo(1));
            Assert.That(dataResult.Data.CurrentPage, Is.EqualTo(1));

            Assert.That(userDetail.Password, Is.EqualTo(string.Empty)); // Ensure password hidden
            Assert.That(userDetail.Email, Is.EqualTo("doe.john@gmail.com"));
        }

        [Test]
        public async Task GetAllUsersAsync_ReturnsMultipleUsers_WhenMoreThanOneExists()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
                new User
                {
                    Id = Guid.Parse("1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e"),
                    Name = "John Doe",
                    Email = "doe.john@gmail.com",
                    IsVerified = false
                },
                new User
                {
                    Id = Guid.Parse("4b79aeeb-96cd-49bf-abf0-8b5f6f693467"),
                    Name = "Jane Doe",
                    Email = "doe.jane@gmail.com",
                    IsVerified = true
                }
            });

            // Act
            var result = await _repository.GetAllUsersAsync(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5);
            var dataResult = result as ResponseDataDetail<PagedResult<UserDetail>>;

            // Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo("2 users fetched successfully"));
            Assert.That(dataResult!.Data.ItemCount, Is.EqualTo(2));
        }
        #endregion

        #region GetUserBySearch
        [Test]
        public async Task GetUserBySearch_WhenSearchTextMatchesName()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
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
            });

            //Act
            var result = await _repository.GetUserBySearch(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5, searchText: "doe");
            var dataResult = result as ResponseDataDetail<PagedResult<UserDetail>>;

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(dataResult!.Data.ItemCount, Is.EqualTo(2));
            Assert.That(dataResult!.Data.Items.All(User => User.Name.Contains("Doe", StringComparison.OrdinalIgnoreCase)), Is.True);
        }

        [Test]
        public async Task GetUserBySearch_WhenSearchTextMatchesEmail()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
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
                    Email = "doe.jane@outlook.com",
                    IsVerified = true
                }
            });

            //Act
            var result = await _repository.GetUserBySearch(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5, searchText: "@outlook.com");
            var dataResult = result as ResponseDataDetail<PagedResult<UserDetail>>;

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(dataResult!.Data.ItemCount, Is.EqualTo(1));
            Assert.That(dataResult!.Data.Items.First().Email, Is.EqualTo("doe.jane@outlook.com"));
        }

        [Test]
        public async Task GetUserBySearch_WhenSearchTextHasNoMatch()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
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
                    Email = "doe.jane@outlook.com",
                    IsVerified = true
                }
            });

            //Act
            var result = await _repository.GetUserBySearch(userId: Guid.NewGuid().ToString(), page: 1, pageSize: 5, searchText: "XYZ");
            var dataResult = result as ResponseDataDetail<PagedResult<UserDetail>>;

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("No users found matching with search text 'XYZ'"));
        }
        #endregion

        #region GetUserAsync
        [Test]
        public async Task GetUserAsync_ReturnsFalse_WhenUserNotFound()
        {
            // Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>());

            // Act
            var result = await _repository.GetUserAsync(Guid.NewGuid().ToString());

            // Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("User not found"));
        }

        [Test]
        public async Task GetUserAsync_ReturnsUser_WhenUserExists()
        {
            //Arrange
            string userId = "1e61f4a4-0e98-4fd9-bfc4-0c1c0da4a66e";

            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {                
                new User
                {
                    Id = Guid.Parse(userId),
                    Name = "John Doe",
                    Email = "doe.john@gmail.com",
                    IsVerified = true
                }
            });

            //Act
            var result = await _repository.GetUserAsync(userId);
            var dataResult = result as ResponseDataDetail<UserDetail>;


            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo("User fetched successfully"));
            Assert.That(result, Is.TypeOf<ResponseDataDetail<UserDetail>>());

            
            Assert.That(dataResult!.Data.Password, Is.EqualTo(string.Empty)); // Password hidden
            Assert.That(dataResult.Data.Name, Is.EqualTo("John Doe"));
        }
        #endregion
    }
}
