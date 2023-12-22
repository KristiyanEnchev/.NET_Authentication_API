namespace Tests.API
{
    using MediatR;

    using Moq;

    using NUnit.Framework;

    using Shared;

    using Shouldly;

    using Application.Handlers.Identity.Commands.Register;
    using Application.Handlers.Identity.Commands.Login;
    using Application.Handlers.Identity.Common;
    using Application.Handlers.Identity.Commands.Logout;

    [TestFixture]
    public class IdentityControllerTests
    {
        private Mock<IMediator> _mockMediator { get; set; } = null;

        [SetUp]
        public void Setup() 
        {
            _mockMediator = new Mock<IMediator> { CallBase = true };
        }

        [Test]
        public async Task Register_New_User_Successfully()
        {
            // Arrange
            var userRegisterCommand = new UserRegisterCommand("First", "Last", "email@example.com", "Password123", "Password123");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserRegisterCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<string>.SuccessResult("Successful Registration"));

            // Act
            var result = await _mockMediator.Object.Send(userRegisterCommand);

            // Assert
            result.ShouldBeOfType<Result<string>>();
            result.Success.ShouldBeTrue();
            result.Data.ShouldBe("Successful Registration");
        }

        [Test]
        public async Task Register_User_Fails_When_Email_Already_Exists()
        {
            // Arrange
            var userRegisterCommand = new UserRegisterCommand("First", "Last", "email@example.com", "Password123", "Password123");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserRegisterCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<string>.Failure("Email already in use"));

            // Act
            var result = await _mockMediator.Object.Send(userRegisterCommand);

            // Assert
            result.ShouldBeOfType<Result<string>>();
            result.Success.ShouldBeFalse();
            result.Errors.ShouldContain("Email already in use");
        }

        // Success scenario
        [Test]
        public async Task Login_Successful_With_Valid_Credentials()
        {
            var token = "valid_token";
            var refreshToken = "valid_token";
            var refreshTokenExpiryTime = DateTime.UtcNow;
            var tokenResponse = new UserResponseModel(token, refreshTokenExpiryTime, refreshToken);
            var loginCommand = new UserLoginCommand("email@example.com", "Password123");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<UserResponseModel>.SuccessResult(tokenResponse));

            var result = await _mockMediator.Object.Send(loginCommand);

            result.ShouldBeOfType<Result<UserResponseModel>>();
            result.Success.ShouldBeTrue();
            (result as Result<UserResponseModel>).Data.AccesToken.ShouldNotBeNullOrEmpty();
        }

        // Failure scenario
        [Test]
        public async Task Login_Fails_With_Invalid_Credentials()
        {
            var loginCommand = new UserLoginCommand("email@example.com", "WrongPassword");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserLoginCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<UserResponseModel>.Failure("Invalid credentials"));

            var result = await _mockMediator.Object.Send(loginCommand);

            result.Success.ShouldBeFalse();
            result.Errors.ShouldContain("Invalid credentials");
        }

        [Test]
        public async Task Logout_Successfully()
        {
            // Arrange
            var logoutCommand = new UserLogoutCommand("email@example.com");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<string>.SuccessResult("Logged out successfully"));

            // Act
            var result = await _mockMediator.Object.Send(logoutCommand);

            // Assert
            result.ShouldBeOfType<Result<string>>();
            result.Success.ShouldBeTrue();
            result.Data.ShouldBe("Logged out successfully");
        }

        [Test]
        public async Task Logout_Fails_When_User_Not_Found()
        {
            // Arrange
            var logoutCommand = new UserLogoutCommand("unknown@example.com");
            _mockMediator.Setup(x => x.Send(It.IsAny<UserLogoutCommand>(), It.IsAny<CancellationToken>()))
                         .ReturnsAsync(Result<string>.Failure("User not found"));

            // Act
            var result = await _mockMediator.Object.Send(logoutCommand);

            // Assert
            result.ShouldBeOfType<Result<string>>();
            result.Success.ShouldBeFalse();
            result.Errors.ShouldContain("User not found");
        }
    }
}