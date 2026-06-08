using System.Web.Mvc;
using TechForum.Helpers;

namespace TechForum.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            RememberLoginHelper.RestoreLoginFromCookie(HttpContext);

            base.OnActionExecuting(filterContext);
        }
    }
}