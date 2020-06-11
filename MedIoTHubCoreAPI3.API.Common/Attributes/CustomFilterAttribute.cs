using Microsoft.AspNetCore.Mvc.Filters;

namespace MedIoTHubCoreAPI3.API.Common.Attributes
{
    public class CustomFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext actionContext)
        {
            //TODO: actions to implement
        }
    }
}
