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
        private Mock<IUserDetailService> _userDetailServiceMock;
        private Mock<IAccountService> _accountDataServiceMock;
        private Mock<IJwtManager> _jwtManagerMock = new Mock<IJwtManager>();

        private AccountRepository _repository;
        
        [SetUp]
        public void Setup() {
            _userServiceMock = new Mock<IService<User>>();
            _accountServiceMock = new Mock<IService<Account>>();
            _userDetailServiceMock = new Mock<IUserDetailService>();
            _accountDataServiceMock = new Mock<IAccountService>();
            _loggerMock = new Mock<ILogger<AccountRepository>>();
            _repository = new AccountRepository
            (
                _loggerMock.Object, 
                _userServiceMock.Object, 
                _accountServiceMock.Object,
                _userDetailServiceMock.Object,
                _accountDataServiceMock.Object,
                _jwtManagerMock.Object
            );
        }

        #region CheckCredential
        [Test]
        public async Task Check_ReturnsFalse_WhenEmailNotFound()
        {
            //Arrange
            var credential = new UserCredential
            {
                Email = string.Empty,
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
            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(credential.Email)).ReturnsAsync(new UserDetail 
            {
                Id = Guid.NewGuid().ToString(),
                Name = "John Doe",
                Email = credential.Email
            });
            
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
            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(credential.Email)).ReturnsAsync(new UserDetail
            {
                Id = userId.ToString(),
                Name = "John Doe",
                Email = credential.Email
            });

            _accountDataServiceMock.Setup(x => x.CheckAndGetAccount(userId.ToString(), credential.Password)).ReturnsAsync(new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Password = credential.Password
            });

            _accountServiceMock.Setup(x => x.Update(It.IsAny<Account>())).ReturnsAsync(false);

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
            var credential = new UserCredential
            {
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(credential.Email)).ReturnsAsync(new UserDetail
            {
                Id = userId.ToString(),
                Name = "John Doe",
                Email = credential.Email
            });

            _accountDataServiceMock.Setup(x => x.CheckAndGetAccount(userId.ToString(), credential.Password)).ReturnsAsync(new Account
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Password = credential.Password
            });

            _accountServiceMock.Setup(x => x.Update(It.IsAny<Account>())).ReturnsAsync(true);

            _jwtManagerMock.Setup(x => x.GenerateToken(It.IsAny<UserDetail>())).Returns("xxxxx.yyyyy.zzzzz");
            

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
            var userRegistrationDetail = new UserDetail
            {
                Name = "Max Miller",
                Email = "miller.max@gmail.com",
                Password = "TestMiller@1995"
            };

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(userRegistrationDetail.Email)).ReturnsAsync(new UserDetail 
            {
                Id = Guid.NewGuid().ToString(),
                Name = userRegistrationDetail.Name,
                Email = userRegistrationDetail.Email,
                Role = "User",
                IsVerified = true
            });

            //Act
            var result = await _repository.RegisterUser(userRegistrationDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("User already exists!"));
        }

        [Test]
        public async Task Register_ReturnsFalse_WhenUserSaveFails()
        {
            //Arrange
            var userRegistrationDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };
            _userDetailServiceMock.Setup(x => x.GetUserByEmail(userRegistrationDetail.Email)).ReturnsAsync(new UserDetail());

            _userServiceMock.Setup(x => x.Save(It.IsAny<User>())).ReturnsAsync(false);      

            //Act
            var result = await _repository.RegisterUser(userRegistrationDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to create user"));
        }

        [Test]
        public async Task Register_RollsBack_WhenAccountSaveFails()
        {
            //Arrange
            var userRegistrationDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };
            
            User createdUser = new User { Id = Guid.NewGuid() };

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(userRegistrationDetail.Email)).ReturnsAsync(new UserDetail());
            _userDetailServiceMock.Setup(x => x.GetUser(createdUser.Id.ToString())).ReturnsAsync(new UserDetail { 
                Id = createdUser.Id.ToString() 
            });

            _userServiceMock.Setup(x => x.Save(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .ReturnsAsync(true);
            
            Account newAccount = new Account{
                Id = Guid.NewGuid(),
                UserId = createdUser.Id
            };
            _accountServiceMock.Setup(x => x.Save(It.IsAny<Account>())).ReturnsAsync(false);
            _accountDataServiceMock.Setup(x => x.GetAccountById(newAccount.Id.ToString())).ReturnsAsync(newAccount);
            
            _userServiceMock.Setup(x => x.Delete(It.IsAny<User>())).ReturnsAsync(true);       

            //Act
            var result = await _repository.RegisterUser(userRegistrationDetail);

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo("Failed to create account."));

            //Verify that the user deletion was attempted for rollback
            _userServiceMock.Verify(x => x.Delete(createdUser), Times.Once);
        }

        [Test]
        public async Task Register_ReturnsTrue_WhenSuccessful()
        {
            //Arrange
            var userRegistrationDetail = new UserDetail
            {
                Name = "John Doe",
                Email = "doe.john@gmail.com",
                Password = "TestJohn@1994"
            };

            User createdUser = new User();

            _userDetailServiceMock.Setup(x => x.GetUserByEmail(userRegistrationDetail.Email)).ReturnsAsync(new UserDetail());
            _userServiceMock.Setup(x => x.Save(It.IsAny<User>())).ReturnsAsync(true);
            _userServiceMock.Setup(x => x.Save(It.IsAny<User>()))
                .Callback<User>(user => createdUser = user)
                .ReturnsAsync(true);

            Account createdAccount = new Account();

            _accountServiceMock.Setup(x => x.Save(It.IsAny<Account>())).ReturnsAsync(true);
            _accountServiceMock.Setup(x => x.Save(It.IsAny<Account>()))
                .Callback<Account>(account => createdAccount = account)
                .ReturnsAsync(true);
            

            //Act
            var result = await _repository.RegisterUser(userRegistrationDetail);

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
            var user = new User { Id = Guid.NewGuid(), Name = "John Doe", Email = "doe.john@gmail.com", Role = "User", IsVerified = true };

            _userDetailServiceMock.Setup(x => x.GetUser(user.Id.ToString())).ReturnsAsync(new UserDetail { 
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified
            });
            _userServiceMock.Setup(x => x.Delete(It.IsAny<User>())).ReturnsAsync(true);

            //Act
            var result = await _repository.DeleteAccount(user.Id.ToString());

            //Assert
            Assert.That(result.Status, Is.True);
            Assert.That(result.Message, Is.EqualTo($"User, {user.Name} has been deleted successfully."));
        }

        [Test]
        public async Task DeleteAccount_ReturnsFalse_WhenFailed()
        {
            //Arrange
            var user = new User { Id = Guid.NewGuid(), Name = "John Doe", Email = "doe.john@gmail.com", Role = "User", IsVerified = true };

            _userDetailServiceMock.Setup(x => x.GetUser(user.Id.ToString())).ReturnsAsync(new UserDetail
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified
            });
            _userServiceMock.Setup(x => x.Delete(It.IsAny<User>())).ReturnsAsync(false);

            //Act
            var result = await _repository.DeleteAccount(user.Id.ToString());

            //Assert
            Assert.That(result.Status, Is.False);
            Assert.That(result.Message, Is.EqualTo($"Failed to delete user {user.Name}!"));
        }

        [Test]
        public async Task DeleteAccount_Encounters_Exception()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Name = "John Doe", Email = "doe.john@gmail.com", Role = "User", IsVerified = true };

            _userDetailServiceMock.Setup(x => x.GetUser(user.Id.ToString())).ReturnsAsync(new UserDetail
            {
                Id = user.Id.ToString(),
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsVerified = user.IsVerified
            });

            _userServiceMock
                .Setup(x => x.Delete(It.IsAny<User>()))
                .ThrowsAsync(new Exception("Database error during user deletion"));

            // Act
            var result = await _repository.DeleteAccount(user.Id.ToString());

            // Assert
            Assert.That(result.Status, Is.False);

            Assert.That(
                result.Message,
                Is.EqualTo($"Some error occurred while deleting user {user.Name}!"));
        }

        #endregion
    }
}
