using EmailOTPVerificationMVCProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace EmailOTPVerificationMVCProject.Controllers
{
    public class OTPController : Controller
    {
        [HttpPost]
        public JsonResult GenerateOTP(string email)
        {
            EmailOTPModule emailOTPModule = new EmailOTPModule();
            string status;
            try
            {
                status = emailOTPModule.GenerateOTPEmail(email);
                Session["OTP"] = emailOTPModule;
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return Json(new { status = status });
        }

        [HttpPost]
        public JsonResult CheckOTP(string otp)
        {
            // Assuming you have a method to validate the OTP
            EmailOTPModule emailOTPModule = (EmailOTPModule)Session["OTP"];
            string status;
            try
            {
                status = emailOTPModule.CheckOTP(otp);
            }
            catch (Exception ex)
            {
                status = ex.Message;
            }
            return Json(new { status = status });
        }
    }
}