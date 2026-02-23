using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace InstituteWebAPI.CustomActionFilters
{
    public class ValidateModelAttribute: ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .SelectMany(entry => entry.Value!.Errors.Select(error =>
                        string.IsNullOrWhiteSpace(error.ErrorMessage)
                            ? $"The {entry.Key} field is invalid."
                            : error.ErrorMessage))
                    .ToArray();

                context.Result = new BadRequestObjectResult(new
                {
                    Message = "Validation failed.",
                    Errors = errors
                });
            }
        }
    }
}
