using System.Web;
using System.Web.Mvc;

namespace Kentor.AuthServices_Saml
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
