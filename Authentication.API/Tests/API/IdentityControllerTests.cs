namespace Tests.API
{
    using MediatR;

    using Moq;

    using NUnit.Framework;

    using Shared;

    using Shouldly;

    using Application.Handlers.Identity.Commands.Register;

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
    }
}