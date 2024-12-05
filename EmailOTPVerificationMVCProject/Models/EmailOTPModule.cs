using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace EmailOTPVerificationMVCProject.Models
{
    public class EmailOTPModule
    {
        private string CurrentOtp;
        private DateTime OtpExpiryTime;
        private int OtpAttempts;

        private const int MaxOtpAttempts = 10;
        private const int OtpValidDurationMinutes = 5;


        //Database ConnectionString

        string connectionString = ConfigurationManager.ConnectionStrings["OTPConnectionString"].ConnectionString;

        //Constant Variables
        public enum EmailOTPStatus
        {
            EmailOK,
            EmailFail,
            EmailInvalid,
            OTP_OK,
            OTP_Fail,
            OTP_Timeout,
            OTP_Attempt_Exceeded
        }

        public string GenerateOTPEmail(string userEmail)
        {


            // Email domain validation
            if (!userEmail.EndsWith("@gmail.com"))
            {
                return EmailOTPStatus.EmailInvalid.ToString();
            }

            // Generate a random 6-digit OTP
            Random random = new Random();
            CurrentOtp = random.Next(100000, 999999).ToString();
            OtpExpiryTime = DateTime.Now.AddMinutes(OtpValidDurationMinutes);
            OtpAttempts = 0; // Reset attempts counter for the new OTP

            string emailBody = $"Your OTP Code is {CurrentOtp}. The code is valid for {OtpValidDurationMinutes} minute";

            Console.WriteLine($"Generated OTP: {CurrentOtp}"); // Log generated OTP for debugging
            Console.WriteLine($"OTP Expiry Time: {OtpExpiryTime}"); // Log OTP expiry time for debugging

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sqlQuery = @"INSERT INTO OTPTable(EmailAddress, OTP, GeneratedAt) VALUES(@EmailAddress, @OTP, @GeneratedAt)";
                    using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                    {
                        cmd.Parameters.AddWithValue("@EmailAddress", userEmail);
                        cmd.Parameters.AddWithValue("@OTP", CurrentOtp);
                        cmd.Parameters.AddWithValue("@GeneratedAt", OtpExpiryTime);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while inserting OTP into database: " + ex.Message);
                return EmailOTPStatus.EmailFail + ex.Message;
            }



            try
            {
                SendEmail(userEmail, emailBody);
                return EmailOTPStatus.EmailOK.ToString();
            }
            catch (Exception ex)
            {
                return EmailOTPStatus.EmailFail + ex.Message;
            }
        }

        private void SendEmail(string emailAddress, string emailBody)
        {
            try
            {
                string email = ConfigurationManager.AppSettings["SMTP_EMAIL"];
                string password = ConfigurationManager.AppSettings["SMTP_PASSWORD"];
                string host = ConfigurationManager.AppSettings["SMTP_HOST"];
                int port = int.Parse(ConfigurationManager.AppSettings["SMTP_PORT"]);

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    throw new InvalidOperationException("SMTP credentials are not set.");
                }

                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(email);
                mail.To.Add(emailAddress);
                mail.Subject = "Email OTP Verification Code";
                mail.Body = emailBody;

                using (SmtpClient smtp = new SmtpClient(host, port))
                {
                    smtp.Credentials = new System.Net.NetworkCredential(email, password);
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Could not send email.", ex);
            }
        }

        public string CheckOTP(string otp)
        {
            try
            {
                DateTime currentTime = DateTime.Now;
                Console.WriteLine($"Current Time: {currentTime}, OTP Expiry Time: {OtpExpiryTime}, Current OTP: {CurrentOtp}");
                // Log times and current OTP
                if (otp == CurrentOtp)
                {
                    // Reset OTP after successful verification to avoid reuse
                    CurrentOtp = null;
                    return EmailOTPStatus.OTP_OK.ToString();
                }

                if (string.IsNullOrEmpty(CurrentOtp)) // Check if OTP was not generated
                {
                    throw new Exception(EmailOTPStatus.OTP_Fail.ToString());
                }

                if (currentTime > OtpExpiryTime)
                {
                    throw new Exception(EmailOTPStatus.OTP_Timeout.ToString());
                }

                if (OtpAttempts >= MaxOtpAttempts)
                {
                    throw new Exception(EmailOTPStatus.OTP_Attempt_Exceeded.ToString());
                }

                OtpAttempts++;



                throw new Exception(EmailOTPStatus.OTP_Fail.ToString());
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}