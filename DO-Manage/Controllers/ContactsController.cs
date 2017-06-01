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
                    var _result = contactsService.UpdateContact(graphClient, _contact.Id, "test_update");
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
            StatsViewModel result = await sourceContactsService.GetStats();
            return View("Contacts", result);
        }

        public async Task<ActionResult> SyncNewContacts()
        {
            ResultsViewModel results = new ResultsViewModel();
            int addedEntries = 0, unableToAdd = 0;
            try
            {

                // Initialize the GraphServiceClient.
                GraphServiceClient graphClient = SDKHelper.GetAuthenticatedClient();

                // Get Source Contacts
                List<Data.Contact> sourceContacts = await sourceContactsService.GetNewSourceContacts(10);

                // Get contacts.
                //results.Items = await contactsService.GetContacts(graphClient);

                foreach (var _sourceContact in sourceContacts)
                {
                    string _newGraphId = await contactsService.CreateContact(graphClient, _sourceContact);
                    if (await sourceContactsService.AssignGraphIdToContact(_sourceContact.ContactId, _newGraphId))
                    { addedEntries++; }
                    else
                    { unableToAdd++; }
                }
            }
            catch (ServiceException se)
            {
                if (se.Error.Message == Resource.Error_AuthChallengeNeeded) return new EmptyResult();
                return RedirectToAction("Index", "Error", new { message = string.Format(Resource.Error_Message, Request.RawUrl, se.Error.Code, se.Error.Message) });
            }

            // Test update 

            return View("Contacts", results);
        }

    }
}