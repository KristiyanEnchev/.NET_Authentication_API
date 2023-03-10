namespace Web.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Web.Extentions;

    using Application.Identity.Common;
    using Application.Identity.Commands.Login;
    using Application.Identity.Commands.Register;
    using Application.Identity.Commands.Refresh;
    using Application.Identity.Commands.Logout;

    using Shared;

    public class IdentityController : ApiController
    {
        [HttpPost(nameof(Register))]
        [AllowAnonymous]
        [SwaggerOperation("Register a user.", "")]
        public async Task<ActionResult<string>> Register(UserRegisterCommand request)
        {
            return await Mediator.Send(request).ToActionResult();
        }

        [HttpPost(nameof(Login))]
        [AllowAnonymous]
        [SwaggerOperation("Request an access token using credentials.", "")]
        public async Task<ActionResult<Result<UserResponseModel>>> Login(UserLoginCommand request)
        {
            return await Mediator.Send(request).ToActionResult();
        }

        [HttpPost(nameof(Refresh))]
        [SwaggerOperation("Request an access token using a refresh token.", "")]
        public async Task<ActionResult<Result<UserResponseModel>>> Refresh(UserRefreshCommand request)
        {
            return await Mediator.Send(request).ToActionResult();
        }

        [HttpPost(nameof(Logout))]
        [SwaggerOperation("Logs out a user", "")]
        public async Task<ActionResult<Result<string>>> Logout(UserLogoutCommand email)
        {
            return await Mediator.Send(email).ToActionResult();
        }
    }
}