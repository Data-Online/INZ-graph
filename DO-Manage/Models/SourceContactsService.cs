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
            var _result = db.Contacts.Where(s => s.graphId == null).Take(Math.Max(1, maxEntriesToReturn)).ToList();
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
            _contact.LastO365Sync = DateTime.Now;

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

        public async Task<StatsViewModel> GetStats()
        {
            // GPA -- Need last sync date storage in source database
            StatsViewModel result = new StatsViewModel();
           // DateTime lastRefreshDate = DateTime.Now.AddDays(-30);
            result.ContactsOnLocal = db.Contacts.Count();
            //result.ContactsUpdatedSinceLastSync = db.Contacts.Where(s => s.UpdatedOn > lastRefreshDate).Count();
            result.ContactsUpdatedSinceLastSync = db.Contacts.Where(s => s.UpdatedOn > s.LastO365Sync).Count();
            result.ContactsNotYetSyncedToO365 = db.Contacts.Where(s => s.graphId == null).Count();
            return result;
        }

        public async Task<List<Contact>> GetUpdatedContacts(int maxEntriesToReturn)
        {
            var _result = db.Contacts.Where(s => s.UpdatedOn > s.LastO365Sync | s.LastO365Sync == null).Take(Math.Max(1, maxEntriesToReturn)).ToList();
            return _result;
        }

        //public async void UpdateSyncDateTime()
        //{
        //    PortalConfig currentSettings = db.PortalConfigs.FirstOrDefault();

        //    //DateTime lastUpdate = db.PortalConfigs.Select(s => s.ContactsLastSyncDate).FirstOrDefault() ?? DateTime.Now;
        //    currentSettings.ContactsLastSyncDate = DateTime.Now;
        //    db.Entry(currentSettings).State = EntityState.Modified;

        //    db.SaveChanges();

        //}
    }
}