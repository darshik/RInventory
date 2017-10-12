using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using SmartInventory;

namespace SmartInventory.Controllers
{
    public class UserAccountController : Controller
    {
        private InventoryEntities db = new InventoryEntities();

      

        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        public ActionResult Login()
        {
            if (Session["EmailId"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public ActionResult Login(string emailId,string password)
        {
            if (db.InventoryLogins.Any(x => x.EmailID.Equals(emailId) /*&& x.Password.Equals(password)*/))
            {
                var userRole = db.InventoryLogins.First(x => x.EmailID.Equals(emailId)).Role;
                Session["EmailId"] = emailId;
                Session["Role"] = userRole;

                //if (userRole.Equals("admin"))
                //{
                //    return RedirectToAction("Index", "Home");
                //}

                //if (userRole.Equals("user"))
                //{
                //    return RedirectToAction("Index", "Home");
                //}


                return RedirectToAction("Index", "Home");
            }
            
            TempData["Error"] = "Unable to find Email ID in our database. Please enter correct Email ID or contact admin";
            return RedirectToAction("Login");
        }

        public ActionResult LogOff()
        {
            Session.Clear();
            return RedirectToAction("Login", "UserAccount");
        }

        [SessionExpire]
        public ActionResult Settings()
        {
            return View();
        }

        [SessionExpire]
        public ActionResult UpdatePassword(string password, string confirmPassword)
        {
            if (password.Equals(confirmPassword))
            {
                string emailId = Session["EmailId"]+"";
               var user=db.InventoryLogins.First(x => x.EmailID.Equals(emailId));
                user.Password = confirmPassword;
                db.InventoryLogins.AddOrUpdate(user);
                db.SaveChanges();
                TempData["Success"] = "Password has been changed successfully";
            }
            else
            {
                TempData["Error"] = "Password does not match";
            }
            return RedirectToAction("Settings");
        }

        [SessionExpire]
        public ActionResult List()
        {
            return View(db.InventoryLogins.ToList());
        }


        // GET: InventoryLogins/Details/5
        [SessionExpire]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);
            if (inventoryLogin == null)
            {
                return HttpNotFound();
            }
            return View(inventoryLogin);
        }

        // GET: InventoryLogins/Create
        [SessionExpire]
        public ActionResult Create()
        {
            return View();
        }

        [SessionExpire]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "EmailID,Username,Role,FirstName,LastName,PhoneNo,Department")] InventoryLogin inventoryLogin)
        {
            if (ModelState.IsValid)
            {
                if (!db.InventoryLogins.Any(x => x.EmailID.Equals(inventoryLogin.EmailID)))
                {
                    db.InventoryLogins.Add(inventoryLogin);
                    db.SaveChanges();
                    return RedirectToAction("List");
                }
                
                TempData["Error"] = "Email ID is already registered";
                return RedirectToAction("Create");
            }

            return View(inventoryLogin);
        }

        // GET: InventoryLogins/Edit/5
        [SessionExpire]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);
            if (inventoryLogin == null)
            {
                return HttpNotFound();
            }
            return View(inventoryLogin);
        }

        // POST: InventoryLogins/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [SessionExpire]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "EmailID,Username,Role,FirstName,LastName,PhoneNo,Department")] InventoryLogin inventoryLogin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventoryLogin).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("List","UserAccount");
            }
            return RedirectToAction("List", "UserAccount");
        }

        // GET: InventoryLogins/Delete/5
        [SessionExpire]
        public ActionResult Delete(string id)
        {
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);
            db.InventoryLogins.Remove(inventoryLogin);
            db.SaveChanges();
            return RedirectToAction("List");
        }

        // POST: InventoryLogins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [SessionExpire]
        public ActionResult DeleteConfirmed(string id)
        {
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);
            db.InventoryLogins.Remove(inventoryLogin);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [SessionExpire]
        public ActionResult User(string id)
        {
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);
            return View(inventoryLogin);
        }


        [SessionExpire]
        public ActionResult updateDetails()
        {
            string id = Session["EmailID"].ToString();
            InventoryLogin inventoryLogin = db.InventoryLogins.Find(id);

            return PartialView(inventoryLogin);
        }

        [SessionExpire]
        [HttpPost]
        public ActionResult updateDetails([Bind(Include = "EmailID,Username,FirstName,LastName,PhoneNo,Department")] InventoryLogin inventoryLogin)
        {
            if (ModelState.IsValid)
            {
                db.Entry(inventoryLogin).State = EntityState.Modified;
                db.Entry(inventoryLogin).Property(x => x.Password).IsModified = false;
                db.Entry(inventoryLogin).Property(x => x.Role).IsModified = false;
                TempData["Success"] = "Details updated successfully";
                db.SaveChanges();
                return RedirectToAction("Settings", "UserAccount");
            }
            return RedirectToAction("Settings", "UserAccount");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
