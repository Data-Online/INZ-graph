/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Threading.Tasks;
using Microsoft.Graph;
using DO_Manage.Helpers;
using DO_Manage.Models;
using System.Linq;
using System.IO;
using Resources;

namespace DO_Manage.Controllers.Users
{
    [Authorize]
    public class ContactsController : Controller
    {
        ContactsService contactsService = new ContactsService();
        SourceContactsService sourceContactsService = new SourceContactsService();

        // Load the view.
        public ActionResult Index()
        {
            return View("Contacts");
        }

        //  Sync contacts
        /// <summary>
        /// Create any contacts in the target that in the source have no graphId (i.e. they have not yet been synced)
        /// Update target records in cases where last change date on source is later that the last sucessful sync date on file.
        ///     Update the sync date upon completion.
        /// </summary>
        /// <returns></returns>
        // Get all contacts.
        public async Task<ActionResult> GetContacts()
        {

            //var sourceContacts = await sourceContactsService.GetSourceContacts(DateTime.Now.AddDays(-30));
            List<Data.Contact> sourceContacts = await sourceContactsService.GetNewSourceContacts(10);

            ResultsViewModel results = new ResultsViewModel();
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get contacts.
                results.Items = await contactsService.GetContacts(graphClient);

                foreach (var _contact in results.Items)
                {
                    var _result = contactsService.UpdateContact_(graphClient, _contact.Id, "test_update");
                }

                await sourceContactsService.AssignGraphIdToContact(sourceContacts[0].ContactId, results.Items.FirstOrDefault().Id);

            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            // Test update 

            return View("Contacts", results);
        }

        public async Task<ActionResult> GetStats()
        {
            StatsViewModel result = new StatsViewModel();
            result.TargetFolderOnO365 = Settings.O365FolderName;
            try
            {
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                if (await SetupGraphEnvironment(graphClient))
                {
                    result = await GetCommonStats(result, graphClient);
                }
                else
                {
                    result.TargetFolderStatus = string.Format(Resource.Contacts_FolderMissing, Settings.O365FolderName);
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }
            return View("Contacts", result);
        }



        public async Task<ActionResult> SyncNewContacts()
        {
            StatsViewModel result = new StatsViewModel();
            int addedEntries = 0, unableToAdd = 0;
            List<Data.Contact> sourceContacts = new List<Data.Contact>();
            try
            {
                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                bool complete = false;

                if (await SetupGraphEnvironment(graphClient))
                {
                    while (!complete)
                    {
                        sourceContacts = await sourceContactsService.GetNewSourceContacts(100);
                        // Loop through source contacts in chunks of x records
                        if (sourceContacts.Count > 0)
                        {
                            foreach (var sourceContact in sourceContacts)
                            {
                                string newGraphId = await contactsService.CreateContact(graphClient, sourceContact);
                                if (await sourceContactsService.AssignGraphIdToContact(sourceContact.ContactId, newGraphId))
                                { addedEntries++; }
                                else
                                { unableToAdd++; }
                            }
                        }
                        else
                        {
                            complete = true;
                        }
                    }
                    result = await GetCommonStats(result, graphClient);
                    result.ContactsSyncedToO365 = addedEntries;
                    result.ContactsNotSyncedToO365 = unableToAdd;
                }
                else
                {
                    // This is the only test performed at this stage
                    result.TargetFolderStatus = string.Format(Resource.Contacts_FolderMissing, Settings.O365FolderName);
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            return View("Contacts", result);
        }

        public async Task<ActionResult> SyncContactUpdates()
        {
            StatsViewModel result = new StatsViewModel();
            List<Data.Contact> sourceContacts = new List<Data.Contact>();


            try
            {
                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();
                bool complete = false;
                int addedEntries = 0, unableToAdd = 0;

                if (await SetupGraphEnvironment(graphClient))
                {
                    while (!complete)
                    {
                        sourceContacts = await sourceContactsService.GetUpdatedContacts(100);
                        // Loop through source contacts in chunks of x records
                        if (sourceContacts.Count > 0)
                        {
                            foreach (var sourceContact in sourceContacts)
                            {
                                // Mark source contact as updated by setting current date

                                string newGraphId = await contactsService.UpdateContact(graphClient, sourceContact);
                                if (await sourceContactsService.AssignGraphIdToContact(sourceContact.ContactId, newGraphId))
                                { addedEntries++; }
                                else
                                { unableToAdd++; }
                            }
                        }
                        else
                        {
                            complete = true;
                        }
                    }
                    result = await GetCommonStats(result, graphClient);
                    result.ContactsSyncedToO365 = addedEntries;
                    result.ContactsNotSyncedToO365 = unableToAdd;
                }
                else
                {
                    // This is the only test performed at this stage
                    result.TargetFolderStatus = string.Format(Resource.Contacts_FolderMissing, Settings.O365FolderName);
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }


            return View("Contacts", result);
        }
        private async Task<bool> SetupGraphEnvironment(GraphServiceClient graphClient)
        {
            return await contactsService.SetParameters(graphClient);
        }

        private async Task<StatsViewModel> GetCommonStats(StatsViewModel result, GraphServiceClient graphClient)
        {
            result = await sourceContactsService.GetStats();
            result = await contactsService.GetStats(graphClient, result);
            result.TargetFolderStatus = Resource.Contacts_FolderExists;
            result.TargetFolderOnO365 = Settings.O365FolderName;

            return result;
        }
    }
}