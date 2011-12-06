using System.Web.Mvc;
using System.Web.Routing;

[assembly: WebActivator.PreApplicationStartMethod(typeof($rootnamespace$.App_Start.IgnoreRoutes), "PreStart")]
namespace $rootnamespace$.App_Start 
{
    public static class IgnoreRoutes 
    {
        public static void PreStart() 
        {
            // Ignore routes to content used by the security framework.
            RouteTable.Routes.IgnoreRoute("{WebResource.axd*}");
            RouteTable.Routes.IgnoreRoute("{SecurityPortal.aspx*}");
        }
    }
}