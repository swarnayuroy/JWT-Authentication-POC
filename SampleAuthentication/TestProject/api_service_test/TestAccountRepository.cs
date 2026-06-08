using API_Service.AppData;
using API_Service.Models.DTO;
using API_Service.Models.Entities;
using API_Service.Models.ResponseModel;
using API_Service.RepositoryLayer.Repository;
using API_Service.Utils;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestProject.api_service_test
{
    [TestFixture]
    public class TestAccountRepository
    {
        private Mock<ILogger<AccountRepository>> _loggerMock;
        private Mock<IService<User>> _userServiceMock;
        private Mock<IService<Account>> _accountServiceMock;        
        private Mock<IJwtManager> _jwtManagerMock = new Mock<IJwtManager>();

        private AccountRepository _repository;
        
        [SetUp]
        public void Setup() {
            _userServiceMock = new Mock<IService<User>>();
            _accountServiceMock = new Mock<IService<Account>>();
            _loggerMock = new Mock<ILogger<AccountRepository>>();
            _repository = new AccountRepository
            (
                _loggerMock.Object, 
                _userServiceMock.Object, 
                _accountServiceMock.Object, 
                _jwtManagerMock.Object
            );
        }

        #region CheckCredential
        [Test]
        public async Task Check_ReturnsFalse_WhenEmailNotFound()
        {
            //Arrange
            _userServiceMock.Setup(x => x.Get())
            .ReturnsAsync(new List<User>());

            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.CheckCredential(credential);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Incorrect email"));
        }

        [Test]
        public async Task Check_ReturnsFalse_WhenPasswordIncorrect()
        {
            //Arrange
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
                new User 
                { 
                    Id = userId, 
                    Email = "doe.john@gmail.com" 
                }
            });

            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<Account>());

            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.CheckCredential(credential);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Incorrect password"));
        }

        [Test]
        public async Task Check_ReturnsFalse_WhenUpdateFails()
        {
            //Arrange
            var userId = Guid.NewGuid();

            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
                {
                    new User { Id = userId, Email = "doe.john@gmail.com" }
                }
            );

            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<Account>
            {
                new Account
                {
                    UserId = userId,
                    Password = "TestJohn@1994"
                }
            });

            _accountServiceMock.Setup(x => x.Update(It.IsAny<Account>())).ReturnsAsync(false);

            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.CheckCredential(credential);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Some error ocurred!"));
        }

        [Test]
        public async Task Check_ReturnsTrue_WhenSuccessful()
        {
            //Arrange
            var userId = Guid.NewGuid();
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
                new User { Id = userId, Email = "doe.john@gmail.com", Name = "TestJohn@1994" }
            });

            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<Account>
            {
                new Account
                {
                    UserId = userId,
                    Password = "TestJohn@1994"
                }
            });

            _accountServiceMock.Setup(x => x.Update(It.IsAny<Account>())).ReturnsAsync(true);

            _jwtManagerMock.Setup(x => x.GenerateToken(It.IsAny<User>())).Returns("xxxxx.yyyyy.zzzzz");

            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.CheckCredential(credential);
            var dataResult = result as ResponseDataDetail<string>;

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result, Is.TypeOf<ResponseDataDetail<string>>());

            Assert.That(dataResult?.Data, Is.EqualTo("xxxxx.yyyyy.zzzzz"));
        }
        #endregion

        #region RegisterUser
        [Test]
        public async Task Register_ReturnsFalse_WhenEmailAlreadyExists()
        {
            //Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>
            {
                new User { Email = "doe.john@gmail.com" }
            });

            var userDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.RegisterUser(userDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("User already exists!"));
        }

        [Test]
        public async Task Register_ReturnsFalse_WhenUserSaveFails()
        {
            //Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>());

            _userServiceMock.Setup(x => x.Save(It.IsAny<User>())).ReturnsAsync(false);

            var userDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.RegisterUser(userDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to create user"));
        }

        [Test]
        public async Task Register_RollsBack_WhenAccountSaveFails()
        {
            //Arrange
            User createdUser = new User();

            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(() =>
            {
                if (createdUser != null)
                    return new List<User> { createdUser };
                return new List<User>();
            });

            _userServiceMock.Setup(x => x.Save(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .ReturnsAsync(true);

            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<Account>());
            _accountServiceMock.Setup(x => x.Save(It.IsAny<Account>())).ReturnsAsync(false);
            _userServiceMock.Setup(x => x.Delete(It.IsAny<string>())).ReturnsAsync(true);

            var userDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.RegisterUser(userDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to create account."));

            //Verify that the user deletion was attempted for rollback
            _userServiceMock.Verify(x => x.Delete(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task Register_ReturnsTrue_WhenSuccessful()
        {
            //Arrange
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User>());
            _userServiceMock.Setup(x => x.Save(It.IsAny<User>())).ReturnsAsync(true);

            _accountServiceMock.Setup(x => x.Save(It.IsAny<Account>())).ReturnsAsync(true);

            var userDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            //Act
            var result = await _repository.RegisterUser(userDetail);

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo("Account created successfully"));
        }
        #endregion

        #region DeleteAccount

        [Test]
        public async Task DeleteAccount_ReturnsTrue_WhenSuccessful()
        {
            //Arrange
            var user = new User { Id = Guid.NewGuid(), Name = "John Doe" };
            var account = new Account { Id = Guid.NewGuid(), UserId = user.Id };

            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<User> { user });
            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(new List<Account> { account });
            _userServiceMock.Setup(x => x.Delete(It.IsAny<string>())).ReturnsAsync(true);
            _accountServiceMock.Setup(x => x.Delete(It.IsAny<string>())).ReturnsAsync(true);

            //Act
            var result = await _repository.DeleteAccount(user.Id.ToString());

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo($"User, {user.Name} has been deleted successfully."));

            //Verify both user and account were deleted
            _userServiceMock.Verify(x => x.Delete(user.Id.ToString()), Times.Once);
            _accountServiceMock.Verify(x => x.Delete(account.Id.ToString()), Times.Once);
        }

        [Test]
        public async Task DeleteAccount_RollsBack_WhenDeletionFails()
        {
            // Arrange
            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = "John Doe"
            };

            var account = new Account
            {
                Id = Guid.NewGuid(),
                UserId = user.Id
            };

            var userGetCallCount = 0;
            _userServiceMock.Setup(x => x.Get()).ReturnsAsync(() =>
            {
                userGetCallCount++;

                if (userGetCallCount == 1)
                    return new List<User> { user };

                return new List<User>();
            });

            var accountGetCallCount = 0;
            _accountServiceMock.Setup(x => x.Get()).ReturnsAsync(() =>
            {
                accountGetCallCount++;

                if (accountGetCallCount == 1)
                    return new List<Account> { account };

                return new List<Account>();
            });

            _userServiceMock
                .Setup(x => x.Delete(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Database error during user deletion"));

            _accountServiceMock
                .Setup(x => x.Delete(It.IsAny<string>()))
                .ReturnsAsync(true);

            _userServiceMock
                .Setup(x => x.Save(It.IsAny<User>()))
                .ReturnsAsync(true);

            _accountServiceMock
                .Setup(x => x.Save(It.IsAny<Account>()))
                .ReturnsAsync(true);

            // Act
            var result = await _repository.DeleteAccount(user.Id.ToString());

            // Assert
            Assert.That(result.Status, Is.False);

            Assert.That(
                result.Message,
                Is.EqualTo($"Deletion of user, {user.Name} failed!"));

            _userServiceMock.Verify(
                x => x.Delete(user.Id.ToString()),
                Times.Once);

            _accountServiceMock.Verify(
                x => x.Delete(account.Id.ToString()),
                Times.Once);

            _userServiceMock.Verify(
                x => x.Save(It.IsAny<User>()),
                Times.Once);

            _accountServiceMock.Verify(
                x => x.Save(It.IsAny<Account>()),
                Times.Once);
        }

        #endregion
    }
}
