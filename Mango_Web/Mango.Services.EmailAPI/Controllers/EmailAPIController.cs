using Mango.Services.EmailAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Net;
using System.Text.RegularExpressions;

namespace Mango.Services.EmailAPI.Controllers
{
    [Route("api/email")]
    [ApiController]
    public class EmailAPIController : ControllerBase
    {
        private ResponseDto _response;

        public EmailAPIController()
        {
            _response = new ResponseDto();
        }

        [HttpPost("SendEmail")]
        public async Task<ResponseDto> SendEmail(string email, string subject, string content) 
        {
            //Step 1: Check regex email
            // Regular expression pattern for validating an email address
            string pattern = @"^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

            // Create a Regex object with the pattern
            Regex regex = new Regex(pattern);

            if(!regex.IsMatch(email))
            {
                _response.IsSuccess = false;
                _response.Message = "Your email is not valid.";
                return _response;
            }
            try
            {
                if(SendSmtp("tavandai2002@gmail.com", "nwanhjvkhffarllu", email, subject, content))
                {
                    _response.Message = "Send email successfully.";
                    return _response;
                }
                else
                {
                    _response.IsSuccess = false;
                    _response.Message = "Failed to send email.";
                    return _response;
                }
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Message = ex.ToString();
            }
            return _response;
        }

        [NonAction]
        private bool SendSmtp(string emailFrom, string passwordFrom, string emailTo, string subject, string content)
        {
            // Sender's email credentials
            string senderEmail = emailFrom;
            string senderPassword = passwordFrom;

            // Recipient email address
            string recipientEmail = emailTo;

            // Create a MailMessage object
            MailMessage mailMessage = new MailMessage(senderEmail, recipientEmail);
            mailMessage.Subject = subject;
            mailMessage.Body = content;

            // Create a SmtpClient object
            SmtpClient smtpClient = new SmtpClient("smtp.gmail.com");
            smtpClient.Port = 587;
            smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
            smtpClient.EnableSsl = true;

            try
            {
                // Send the email
                smtpClient.Send(mailMessage);
                Console.WriteLine("Email sent successfully!");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending email: {ex.Message}");
            }
            return false;
        }
    }
}
