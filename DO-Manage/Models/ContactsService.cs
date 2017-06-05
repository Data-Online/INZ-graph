/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

using DO_Manage.Helpers;
using Microsoft.Graph;
using Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DO_Manage.Models
{
    public class ContactsService
    {
        //string farmContactsId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgAuAAAAAADZ7WGke0A-TauETMcwNMLaAQDs7SemK8DqQJIXatzBqsNoAACo6iOFAAA=";
        string folderId;

        public class GraphListResponse<T>
        {
            public List<T> Value { get; set; }
        }

        public class GraphUser
        {
            public string Mail { get; set; }
            public string DisplayName { get; set; }
        }

        public async Task<bool> SetParameters(GraphServiceClient graphClient)
        {
            this.folderId = await GetFolderId(Settings.O365FolderName, graphClient);
            return this.folderId == "" ? false : true;
        }

        // Update contacts
        public async Task<List<ResultsItem>> UpdateContact_(GraphServiceClient graphClient, string id, string name)
        {
            List<ResultsItem> items = new List<ResultsItem>();

            // Update the contact.
            await graphClient.Me.Contacts[id].Request().UpdateAsync(new Contact
            {
                MiddleName = Resource.Updated + " " + name
            });

            items.Add(new ResultsItem
            {

                // This operation doesn't return anything.
                Properties = new Dictionary<string, object>
                {
                    { Resource.No_Return_Data, "" }
                }
            });
            return items;
        }

        public async Task<string> UpdateContact(GraphServiceClient graphClient, Data.Contact sourceContact)
        {
            Contact contact = await graphClient.Me.Contacts[sourceContact.graphId].Request().UpdateAsync(MapContactDetails(sourceContact));
            if (contact != null)
            {
                return contact.Id;
            }
            return "";
        }

        public async Task<StatsViewModel> GetStats(GraphServiceClient graphClient, StatsViewModel result)
        {
            string httpRequest = string.Format("https://graph.microsoft.com/v1.0/me/contactfolders/{0}/Contacts/$count", this.folderId);
            string accessToken = await Helpers.SampleAuthProvider.Instance.GetUserAccessTokenAsync();
            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            var response = await httpClient.GetAsync(httpRequest);
            response.EnsureSuccessStatusCode();
            var _result = await response.Content.ReadAsStringAsync() ?? "0";

            //IContactFolderContactsCollectionPage sourceContacts = await graphClient.Me.ContactFolders[this.folderId].Contacts.Request().GetAsync();
            //result.ContactsOnRemote = sourceContacts.Count();

            result.ContactsOnRemote = Int32.Parse(_result);
            return result;
        }

        private async Task<string> GetFolderId(string folderName, GraphServiceClient graphClient)
        {
            string folderId = "";
            try
            {
                var folderList = await graphClient.Me.ContactFolders.Request().GetAsync();

                if (folderList.Count > 0)
                {
                    folderId = folderList.Where(s => s.DisplayName == folderName).Select(s => s.Id).FirstOrDefault().ToString();
                }

                if (Settings.CreateO365Folder == "true" && String.IsNullOrEmpty(folderId))
                {
                    var _newFolder = graphClient.Me.ContactFolders.Request().AddAsync(new ContactFolder() { DisplayName = Settings.O365FolderName });
                    folderId = _newFolder.Id.ToString();
                    //var newFolder = await folderList.Add(newFolder);
                }
            }
            catch (Exception ex)
            {
            }

            return folderId;
        }
        private async Task<Contact> GetContact(GraphServiceClient graphClient, string id)
        {
            Contact contact = await graphClient.Me.Contacts[id].Request().GetAsync();
            return contact;
        }

        // Get all users.
        public async Task<List<ResultsItem>> GetContacts(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            //string farmContactsId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgAuAAAAAADZ7WGke0A-TauETMcwNMLaAQDs7SemK8DqQJIXatzBqsNoAACo6iOFAAA=";
            // Get Contacts.


            IUserContactsCollectionPage contacts = await graphClient.Me.Contacts.Request().GetAsync();
            IUserContactFoldersCollectionPage contactFolders = await graphClient.Me.ContactFolders.Request().GetAsync();
            //ContactFolder farmContacts = await graphClient.Me.ContactFolders[farmContactsId].Request().GetAsync();
            IContactFolderContactsCollectionPage farmContacts = await graphClient.Me.ContactFolders[this.folderId].Contacts.Request().GetAsync();


            //            var zz = await graphClient.Organization.Request().GetAsync();


            // TESTS

            ////// Get all contacts
            ////// Working:
            ////string accessToken = await Helpers.SampleAuthProvider.Instance.GetUserAccessTokenAsync();
            ////var httpClient = new HttpClient();

            ////httpClient.DefaultRequestHeaders.Add("Authorization", string.Format("Bearer {0}", accessToken));
            ////var response = await httpClient.GetAsync("https://graph.microsoft.com/beta/contacts");
            ////response.EnsureSuccessStatusCode();
            ////var result = await response.Content.ReadAsStringAsync();//.ReadAsByteArrayAsync(); /// .ReadAsAsync<GraphListResponse<GraphUser>>();
            ////                                                        //List<GraphUser> users = result.Value;

            ////// Update?? - not supported??

            //////var _request = new HttpRequestMessage(new HttpMethod("PATCH"), "https://graph.microsoft.com/beta/contacts/ID");

            //////_request.Content = new HttpContent();
            //////var _response = await httpClient.SendAsync(_request);
            ////string _testId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgBGAAAAAADZ7WGke0A-TauETMcwNMLaBwDs7SemK8DqQJIXatzBqsNoAAAAAAEOAADs7SemK8DqQJIXatzBqsNoAAB0OACoAAA=";
            ////string _testJSON = string.Format("'businessPhones': ['{0}']", "business-phone-value");

            ////var _response = await Extensions.HttpClientEx.PatchJsonAsync(httpClient, "https://graph.microsoft.com/beta/contacts/"+_testId,typeof(HttpContent), _testJSON);
            // TESTS end

            // Populate the view model.
            if (contacts?.Count > 0)
            {
                foreach (Contact contact in contacts)
                {

                    // Get user properties.
                    items.Add(new ResultsItem
                    {
                        Display = contact.DisplayName,
                        Id = contact.Id
                    });

                    ////// Update the user.
                    ////await graphClient.Me.Contacts[contact.Id].Request().UpdateAsync(new Contact
                    ////{
                    ////    MiddleName = Resource.Updated + "updated"
                    ////});
                }


                foreach (Contact contact in farmContacts)
                {
                    items.Add(new ResultsItem
                    {
                        Display = contact.DisplayName,
                        Id = contact.Id
                    });
                }
            }
            return items;
        }

        public async Task<string> CreateContact(GraphServiceClient graphClient, Data.Contact sourceContact)
        {
            //string farmContactsId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgAuAAAAAADZ7WGke0A-TauETMcwNMLaAQDs7SemK8DqQJIXatzBqsNoAACo6iOFAAA=";
            // TRY / CATCH! (controller does this)
            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // Add the contact.
            ////IEnumerable<EmailAddress> _contactEmailAddress = new List<EmailAddress>() {
            ////    new EmailAddress {Address = sourceContact.eMail1 ?? "", Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName) },
            ////    new EmailAddress {  Address = sourceContact.eMail2 ?? "", Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName)} };

            //_contactEmailAddress.Add(new EmailAddress()
            //{
            //    Address = sourceContact.eMail1 ?? "",
            //    Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName),
            //});
            //_contactEmailAddress.Add(new EmailAddress()
            //{
            //    Address = sourceContact.eMail2 ?? "",
            //    Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName),
            //});

            // Check source contains all required fields.

            ////Contact newContact = new Contact
            ////{
            ////    EmailAddresses = _contactEmailAddress,
            ////    BusinessAddress = new PhysicalAddress
            ////    {
            ////        City = sourceContact.City == null ? "" : sourceContact.City.City1,
            ////        Street = sourceContact.Address1 ?? "" + ", " + sourceContact.Address2 ?? "",
            ////        PostalCode = sourceContact.PostCode ?? "",
            ////        State = sourceContact.Region == null ? "" : sourceContact.Region.Region1,
            ////        CountryOrRegion = sourceContact.Country == null ? "" : sourceContact.Country.Country1
            ////    },
            ////    GivenName = sourceContact.FirstName ?? "",
            ////    Surname = sourceContact.LastName ?? "",
            ////    MiddleName = sourceContact.MiddleName ?? ""
            ////};

            ////Contact contact = await graphClient.Me.ContactFolders[this.folderId].Contacts.Request().AddAsync(newContact);
            Contact contact = await graphClient.Me.ContactFolders[this.folderId].Contacts.Request().AddAsync(MapContactDetails(sourceContact));

            if (contact != null)
            {
                return contact.Id;
            }
            return "";
        }

        private Contact MapContactDetails(Data.Contact sourceContact)
        {
            // Add the contact.
            IEnumerable<EmailAddress> _contactEmailAddress = new List<EmailAddress>() {
                new EmailAddress {Address = sourceContact.eMail1 ?? "", Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName) },
                new EmailAddress {  Address = sourceContact.eMail2 ?? "", Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName)} };

            return new Contact
            {
                EmailAddresses = _contactEmailAddress,
                BusinessAddress = new PhysicalAddress
                {
                    City = sourceContact.City == null ? "" : sourceContact.City.City1,
                    Street = sourceContact.Address1 ?? "" + ", " + sourceContact.Address2 ?? "",
                    PostalCode = sourceContact.PostCode ?? "",
                    State = sourceContact.Region == null ? "" : sourceContact.Region.Region1,
                    CountryOrRegion = sourceContact.Country == null ? "" : sourceContact.Country.Country1
                },
                GivenName = sourceContact.FirstName ?? "",
                Surname = sourceContact.LastName ?? "",
                MiddleName = sourceContact.MiddleName ?? ""
            };
        }

    }
}