using System.Web;
using System.Web.Mvc;
//using MvcTest.Filters;
using WebMatrix.WebData;
namespace MvcTest
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}