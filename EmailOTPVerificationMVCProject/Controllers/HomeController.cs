using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EmailOTPVerificationMVCProject.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GenerateOTP()
        {
            return View();
        }

        public ActionResult CheckOTP()
        {

            return View();
        }
    }
}