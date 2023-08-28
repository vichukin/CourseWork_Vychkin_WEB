namespace CourseWork_Vychkin_WEB.Models.RouteConstraints
{
    public class ActionRouteConstraint : IRouteConstraint
    {
        public string ActionName { get; set; }
        public ActionRouteConstraint(string ActionName)
        {
            this.ActionName = ActionName;
        }
        public bool Match(HttpContext? httpContext, IRouter? route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            if (values["action"] != null)
            {
                string action = values["action"].ToString().ToLower();
                return action == ActionName.ToLower();
            }
            return false;
        }
    }
}
