using DO_Manage.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace DO_Manage.Models
{

    public class SourceContactsService
    {
        private INZ_dbEntities db = new INZ_dbEntities();

        public async Task<List<Contact>> GetSourceContacts(DateTime lastUpdateDate)
        {
            var _result = db.Contacts.Where(s => s.UpdatedOn > lastUpdateDate).ToList();

            return _result;
        }

        public async Task<List<Contact>> GetNewSourceContacts(int maxEntriesToReturn)
        {
            var _result = db.Contacts.Where(s => s.graphId == null).Take(Math.Max(1,maxEntriesToReturn)).ToList();
            return _result;
        }

        public async Task<bool> AssignGraphIdToContact(int id, string graphId)
        {
            if (String.IsNullOrEmpty(graphId))
            {
                return false;
            }

            Contact _contact = db.Contacts.Find(id);
            if (_contact == null)
            {
                return false;
            }

            _contact.graphId = graphId;

            db.Entry(_contact).State = EntityState.Modified;
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                
            }

            return true;
        }
    }
}