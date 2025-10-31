using Microsoft.AspNetCore.Mvc.Filters;

namespace HRSystem.API.CustomActionFilters
{
    public class ValidateModelAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.ModelState.IsValid == false)
            {
                context.HttpContext.Response.StatusCode = 400; // Bad Request
                context.Result = new Microsoft.AspNetCore.Mvc.JsonResult(context.ModelState);
            }
        }
    }
}
