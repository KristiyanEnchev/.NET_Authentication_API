namespace Web.Controllers.Account
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Application.Handlers.Account.Queries;

    using Web.Extentions;

    public class AccountController : ApiController
    {
        [AllowAnonymous]
        //[Authorize]
        //[Authorize(Roles = "Admin")]
        [HttpGet(nameof(GetUsers))]
        [SwaggerOperation("Get All Users.", "")]

        public async Task<IActionResult> GetUsers()
        {
            return await Mediator.Send(new GetUsersQuery()).ToActionResult();
        }
    }
}
