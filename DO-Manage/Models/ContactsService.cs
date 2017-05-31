/* 
*  Copyright (c) Microsoft. All rights reserved. Licensed under the MIT license. 
*  See LICENSE in the source repository root for complete license information. 
*/

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
        public class GraphListResponse<T>
        {
            public List<T> Value { get; set; }
        }

        public class GraphUser
        {
            public string Mail { get; set; }
            public string DisplayName { get; set; }
        }

        // Update contacts
        public async Task<List<ResultsItem>> UpdateContact(GraphServiceClient graphClient, string id, string name)
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

        // Get all users.
        public async Task<List<ResultsItem>> GetContacts(GraphServiceClient graphClient)
        {
            List<ResultsItem> items = new List<ResultsItem>();
            string farmContactsId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgAuAAAAAADZ7WGke0A-TauETMcwNMLaAQDs7SemK8DqQJIXatzBqsNoAACo6iOFAAA=";
            // Get Contacts.
            IUserContactsCollectionPage contacts = await graphClient.Me.Contacts.Request().GetAsync();
            IUserContactFoldersCollectionPage contactFolders = await graphClient.Me.ContactFolders.Request().GetAsync();
            //ContactFolder farmContacts = await graphClient.Me.ContactFolders[farmContactsId].Request().GetAsync();
            IContactFolderContactsCollectionPage farmContacts = await graphClient.Me.ContactFolders[farmContactsId].Contacts.Request().GetAsync();


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
            string farmContactsId = "AAMkAGEyOTI1NmYyLTFjOGEtNGExYy04Y2RkLTRiMzNkNDUwNTVjNgAuAAAAAADZ7WGke0A-TauETMcwNMLaAQDs7SemK8DqQJIXatzBqsNoAACo6iOFAAA=";

            List<ResultsItem> items = new List<ResultsItem>();
            string guid = Guid.NewGuid().ToString();

            // Add the contact.
            IEnumerable<EmailAddress> _contactEmailAddress = new List<EmailAddress>() { new EmailAddress()
                            {
                                Address = sourceContact.eMail1 ?? "",
                                Name = String.Format("{0} {1}", sourceContact.FirstName, sourceContact.LastName),
                            }
            };

            // Check source contains all required fields.

            Contact contact = await graphClient.Me.ContactFolders[farmContactsId].Contacts.Request().AddAsync(new Contact
            {
                EmailAddresses = _contactEmailAddress,
                BusinessAddress = new PhysicalAddress { City = sourceContact.Address1 },
                GivenName = sourceContact.FirstName ?? "",
                Surname = sourceContact.LastName ?? "",
                MiddleName = sourceContact.MiddleName ?? ""
            });

            if (contact != null)
            {
                return contact.Id;
            }
            return "";
        }

    }
}