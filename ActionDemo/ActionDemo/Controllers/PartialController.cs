using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionDemo.Models;

namespace ActionDemo.Controllers
{
    public class PartialController : BaseController
    {
        //
        // GET: /Partial/
        [ChildActionOnly]
        public ActionResult Partial()
        {
            ActionDemo.Models.Data d = new ActionDemo.Models.Data();
            d.name = "testname";
            d.dep = "testdep";
            return PartialView(d); 
        }
    }
}
