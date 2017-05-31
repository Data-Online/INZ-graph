using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using DO_Manage.Data;

namespace DO_Manage.Controllers
{
    public class SourceContactsController : Controller
    {
        private INZ_dbEntities db = new INZ_dbEntities();

        // GET: SourceContacts
        public ActionResult Index()
        {
            DateTime _lastUpdateDate = DateTime.Now.AddDays(-30);
            var _result = db.Contacts.Where(s => s.UpdatedOn > _lastUpdateDate).ToList();
            return View(_result);
        }

        // GET: SourceContacts/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // GET: SourceContacts/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: SourceContacts/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ContactId,FirstName,MiddleName,LastName,Title,Address1,Address2,SuburbId,CityId,RuralDelivery,RegionId,CountryId,Initials,BusinessPhoneNumber,MobilePhoneNumber,HomePhoneNumber,AdditionalPhoneNumber,eMail1,eMail2,PostCode,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,JoiningDate,INZPosition,INZMember")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Contacts.Add(contact);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(contact);
        }

        // GET: SourceContacts/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: SourceContacts/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContactId,FirstName,MiddleName,LastName,Title,Address1,Address2,SuburbId,CityId,RuralDelivery,RegionId,CountryId,Initials,BusinessPhoneNumber,MobilePhoneNumber,HomePhoneNumber,AdditionalPhoneNumber,eMail1,eMail2,PostCode,CreatedBy,UpdatedBy,CreatedOn,UpdatedOn,JoiningDate,INZPosition,INZMember")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contact);
        }

        // GET: SourceContacts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contacts.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: SourceContacts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Contact contact = db.Contacts.Find(id);
            db.Contacts.Remove(contact);
            db.SaveChanges();
            return RedirectToAction("Index");
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
