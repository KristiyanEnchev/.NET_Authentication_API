namespace Web.Extentions
{
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc;

    using Shared;

    public static class ResultExtensions
    {
        public static async Task<ActionResult<TData>> ToActionResult<TData>(this Task<Result<TData>> resultTask)
        {
            var result = await resultTask;

            if (!result.Succeeded)
            {
                if (result.Exception != null)
                {
                    return new BadRequestObjectResult(new { error = result.Exception.Message });
                }
                return new BadRequestResult();
            }

            if (result.Data == null && result.Messages.Any())
            {
                return new OkObjectResult(new { message = result.Messages.FirstOrDefault() });
            }
            else if (result.Data == null)
            {
                return new NoContentResult();
            }

            return new OkObjectResult(new { data = result.Data, message = result.Messages.FirstOrDefault() });
        }

    }
}