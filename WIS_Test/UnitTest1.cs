using NUnit.Framework;
using Moq;
using System.Collections.Generic;
using System.Linq;
using WIS.Core.Services;
using WIS;
using WIS.AplicationData;

namespace WIS_Test
{
    public class AuthServiceTests
    {
        private AuthService _authService;
        private Mock<WISEntities> _mockContext;

        [SetUp]
        public void Setup()
        {
            var fakeUsers = new List<WIS_Users>
            {
                new WIS_Users
                {
                    user_login = "admin",
                    user_password_hash = HashHelper.ComputeSha256Hash("admin123"),
                    user_role_ID = 1,
                    RoleName = "Администратор"
                }
            }.AsQueryable();

            var mockDbSet = new Mock<DbSet<WIS_Users>>();
            mockDbSet.As<IQueryable<WIS_Users>>().Setup(m => m.Provider).Returns(fakeUsers.Provider);
            mockDbSet.As<IQueryable<WIS_Users>>().Setup(m => m.Expression).Returns(fakeUsers.Expression);
            mockDbSet.As<IQueryable<WIS_Users>>().Setup(m => m.ElementType).Returns(fakeUsers.ElementType);
            mockDbSet.As<IQueryable<WIS_Users>>().Setup(m => m.GetEnumerator()).Returns(fakeUsers.GetEnumerator());

            _mockContext = new Mock<WISEntities>();
            _mockContext.Setup(c => c.WIS_Users).Returns(mockDbSet.Object);

            _authService = new AuthService(_mockContext.Object);
        }

        [Test]
        public void Authenticate_ValidCredentials_ReturnsSuccess()
        {
            var result = _authService.Authenticate("admin", "admin123");

            Assert.IsTrue(result.Success);
            Assert.IsNotNull(result.User);
            Assert.AreEqual("admin", result.User.user_login);
        }

        [Test]
        public void Authenticate_InvalidPassword_ReturnsFailure()
        {
            var result = _authService.Authenticate("admin", "wrongpass");

            Assert.IsFalse(result.Success);
            Assert.IsNull(result.User);
            Assert.AreEqual("Неверный логин или пароль", result.ErrorMessage);
        }

        [Test]
        public void Authenticate_EmptyLogin_ReturnsFailure()
        {
            var result = _authService.Authenticate("", "admin123");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Логин и пароль не могут быть пустыми", result.ErrorMessage);
        }

        [Test]
        public void Authenticate_EmptyPassword_ReturnsFailure()
        {
            var result = _authService.Authenticate("admin", "");

            Assert.IsFalse(result.Success);
            Assert.AreEqual("Логин и пароль не могут быть пустыми", result.ErrorMessage);
        }
    }
}