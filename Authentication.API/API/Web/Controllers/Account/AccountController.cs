namespace Web.Controllers.Account
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Application.Handlers.Account.Queries;
    using Application.Handlers.Account.Common;
    using Application.Handlers.Account.Comands;

    using Web.Extentions;

    using Persistence.Constants;

    using Shared;

    using Models.Enums;

    public class AccountController : ApiController
    {
        [Authorize(Roles = Roles.Administrator)]
        [HttpGet(nameof(GetUsers))]
        [SwaggerOperation("Get All Users.", "")]
        public async Task<IActionResult> GetUsers()
        {
            return await Mediator.Send(new GetUsersQuery()).ToActionResult();
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpGet(nameof(GetUsersPaged))]
        [SwaggerOperation("Get All Users.", "")]
        public async Task<PaginatedResult<UserResponseGetModel>> GetUsersPaged(int pageNumber, int pageSize, SortBy sortBy, Sort order)
        {
            return await Mediator.Send(new GetUsersPagedQuery(pageNumber, pageSize, sortBy, order));
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpGet(nameof(GetBy))]
        [SwaggerOperation("Get User By.", "")]
        public async Task<IActionResult> GetBy(FindBy findBy, string value)
        {
            return await Mediator.Send(new GetUserQuery(findBy, value)).ToActionResult();
        }

        [Authorize(Roles = Roles.Administrator)]
        [HttpPost(nameof(ToggleStatus))]
        [SwaggerOperation("Toggle user status.", "")]
        public async Task<IActionResult> ToggleStatus(string identifier , [FromQuery]ToggleUserValue toggleValiue)
        {
            return await Mediator.Send(new ToggleStatusCommand(identifier, toggleValiue)).ToActionResult();
        }
    }
}