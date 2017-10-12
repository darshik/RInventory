using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace SmartInventory.Controllers
{
    public class ContactController : Controller
    {
        [SessionExpire]
        // GET: Contact
        public ActionResult Index()
        {
            return View();
        }


        [HttpPost]
        [SessionExpire]
        public ActionResult SendMail(string subject,string message)
        {

            MailMessage mail = new MailMessage();
            var username = Session["EmailID"].ToString();
            mail.To.Add(ConfigurationManager.AppSettings["ContactEmail"]);
            mail.To.Add(username);

            mail.From = new MailAddress(ConfigurationManager.AppSettings["ContactEmail"]);
            mail.Subject = subject + " - " + username;


            mail.Body = "<br/><h4>Message:" + message + "</h4>";
            mail.IsBodyHtml = true;
            SmtpClient mSmtpClient = new SmtpClient();
            mSmtpClient.Send(mail);

            TempData["Success"] = "Your message has been successfully sent. We will get back to you as soon as possible";

            return RedirectToAction("Index");
        }
    }
}