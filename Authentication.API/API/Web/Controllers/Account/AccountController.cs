namespace Web.Controllers.Account
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Application.Handlers.Account.Queries;
    using Application.Handlers.Account.Common;

    using Shared;

    public class AccountController : ApiController
    {
        [HttpPost(nameof(GetUsers))]
        [AllowAnonymous]
        [SwaggerOperation("Get All Users.", "")]

        public async Task<ActionResult<Result<UserResponseGetModel>>> GetUsers(GetUsersQuery request)
        {
            return await Mediator.Send(request).ToActionResult();
        }
    }
}
