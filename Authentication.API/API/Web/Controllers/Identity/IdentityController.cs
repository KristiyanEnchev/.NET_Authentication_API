namespace Web.Controllers.Identity
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Application.Identity.Commands.Register;

    using Swashbuckle.AspNetCore.Annotations;

    using Web.Extentions;

    public class IdentityController : ApiController
    {
        [HttpPost(nameof(Register))]
        [AllowAnonymous]
        [SwaggerOperation("Register a user.", "")]
        public async Task<ActionResult<string>> Register(UserRegisterCommand request)
        {
            return await Mediator.Send(request).ToActionResult();
        }
    }
}
