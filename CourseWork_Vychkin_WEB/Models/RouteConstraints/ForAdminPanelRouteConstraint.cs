namespace CourseWork_Vychkin_WEB.Models.RouteConstraints
{
    public class ForAdminPanelRouteConstraint : IRouteConstraint
    {

        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values["controller"] != null)
            {
                string str = values["controller"].ToString().ToLower();
                return str != "home" && str != "upload"&&str!="account";
            }
            return false;
        }
    }
}
