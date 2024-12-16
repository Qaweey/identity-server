using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace settl.identityserver.Domain.Shared.Filters
{
    public class ModelValidationFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList());
                //var modelErrors = context.ModelState.Values.SelectMany(x => x.Errors).Select(x => x.ErrorMessage).ToList();
                //(modelErrors, responses[CustomApiResponse.Status.MISSING_REQUIRED_PARAMETER]);
            }
        }
    }
}