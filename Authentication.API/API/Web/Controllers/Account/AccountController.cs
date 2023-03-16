namespace Web.Controllers.Account
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Authorization;

    using Swashbuckle.AspNetCore.Annotations;

    using Application.Handlers.Account.Queries;
    using Application.Handlers.Account.Common;

    using Web.Extentions;

    using Persistence.Constants;

    using Shared;

    using static Application.Handlers.Account.Queries.GetUsersPagedQuery;

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
        public async Task<PaginatedResult<UserResponseGetModel>> GetUsersPaged(int pageNumber,int pageSize, SortBy sortBy, Sort order)
        {
            return await Mediator.Send(new GetUsersPagedQuery(pageNumber, pageSize, sortBy, order));
        }
    }
}
