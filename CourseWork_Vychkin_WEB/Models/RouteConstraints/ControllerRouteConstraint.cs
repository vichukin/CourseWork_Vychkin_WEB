namespace CourseWork_Vychkin_WEB.Models.RouteConstraints
{
    public class ControllerRouteConstraint : IRouteConstraint
    {
        public string ControllerName { get; set; }
        public ControllerRouteConstraint(string controllerName)
        {
            ControllerName = controllerName;
        }
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values["controller"] != null)
            {
                string controller = values["controller"].ToString().ToLower();
                return controller == ControllerName.ToLower();
            }
            return false;
        }
    }
}
