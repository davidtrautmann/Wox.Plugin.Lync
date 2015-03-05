using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Extensibility;
using System.Threading;

namespace Wox.Plugin.Lync
{
    /// <summary>
    ///  Provides lync connectivity.
    /// </summary>
    public class LyncManager
    {
        private LyncClient              _LyncClient;
        private ContactManager          _ContactManager;
        private SearchProviders         _MyActiveSearchProviders;
        private bool                    _Finished;

        public  ContactSubscription     _SearchResultSubscription { get; set; }

        public LyncManager()
        {
            _LyncClient = LyncClient.GetClient();
            _ContactManager = _LyncClient.ContactManager;
            _MyActiveSearchProviders = new SearchProviders();   
            _SearchResultSubscription = _ContactManager.CreateSubscription();       
        }

        /// <summary>
        /// Search for a contact or group
        /// </summary>
        /// <param name="searchName">string. Name of contact or group to search for</param>
        /// <param name="numResults">uint. Number of results to return.</param>
        public void SearchForGroupOrContact(string searchName, uint numResults)
        {
            // Initiate search for entity based on name.

            SearchFields searchFields = _LyncClient.ContactManager.GetSearchFields();
            object[] _asyncState = { _LyncClient.ContactManager, searchName };
            _Finished = _LyncClient.ContactManager.BeginSearch(
                searchName
                , _MyActiveSearchProviders //Synchronized providers
                , searchFields            //All availalble search fields
                , SearchOptions.Default
                , numResults              //Number of results to return
                , SearchResultsCallback   //System.IAsyncResult callback – used to obtain search results
                , _asyncState).IsCompleted;

            while (!_Finished)
            {
                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Handles callback containing results of a search
        /// </summary>
        /// <param name="source"></param>
        /// <param name="results"></param>
        /// <param name="_asyncOperation"></param>
        public void SearchResultsCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted == true)
            {
                SearchResults results = null;
                object[] _asyncState = (object[])ar.AsyncState;
                List<Contact> contacts = new List<Contact>();
                try
                {
                    results = ((ContactManager)_asyncState[0]).EndSearch(ar);
                    if (results.AllResults.Count != 0)
                    {
                        //subscribe
                        this.SubscribeToSearchResults(results.Contacts);
                        _Finished = true;
                    }
                    else
                    {
                        Console.WriteLine("0 instances found for " + _asyncState[1].ToString());
                    }
                }
                catch (SearchException se)
                {
                    Console.WriteLine("Search failed: " + se.Reason.ToString());
                }
            }
        }

        /// <summary>
        /// Adds contacts found through a search to a ContactSubscription and raises
        /// ContactAddedEvent to UI.
        /// </summary>
        /// <param name="pContact">List[Contact]. The list of contacts found in a search.</param>
        /// <param name="subscribeContext">string. The context in which this method is called.</param>
        public void SubscribeToSearchResults(IList<Contact> pContactList)
        {
            try
            {
                if (_SearchResultSubscription == null)
                {
                    _SearchResultSubscription = _ContactManager.CreateSubscription();
                }
                else
                {
                    //remove all existing search results
                    _SearchResultSubscription.Unsubscribe();
                    foreach (Contact c in _SearchResultSubscription.Contacts)
                    {
                        _SearchResultSubscription.RemoveContact(c);
                    }
                }

                //add the Contact to a ContactSubscription
                _SearchResultSubscription.AddContacts(pContactList);

                //Specify the Contact Information Types to be returned in ContactInformationChanged events.
                ContactInformationType[] ContactInformationTypes = { ContactInformationType.Availability, ContactInformationType.ActivityId };

                //Activate the subscription
                _SearchResultSubscription.Subscribe(ContactSubscriptionRefreshRate.High, ContactInformationTypes);
            }
            catch (Exception) { }
        }

        /// <summary>
        ///  Start a conversation wit the given contact and mode.
        /// </summary>
        /// <param name="pContact">
        ///  Contact for this conversation.
        /// </param>
        /// <param name="pMode">
        ///  Mode of the conversation.
        /// </param>
        public void StartConversation(Contact pContact, AutomationModalities pMode)
        {
            Automation automation = LyncClient.GetAutomation();
            Dictionary<AutomationModalitySettings, object> conversationSettings = new Dictionary<AutomationModalitySettings, object>();
            List<string> participants = new List<string>();

            participants.Add(pContact.Uri);
            automation.BeginStartConversation(pMode, participants, null, StartConversationCallback, automation);
        }

        public void StartConversationCallback(IAsyncResult ar)
        {
            if (ar.IsCompleted == true)
            {

            }
        }
    }
}
