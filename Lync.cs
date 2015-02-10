using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Management;
using System.IO;
using Microsoft.Lync.Model;
using Microsoft.Lync.Model.Extensibility;

namespace Wox.Plugin.Lync
{
    /// <summary>
    ///  Launcher class for axc files.
    /// </summary>
    public class Lync : IPlugin
    {
        /// <summary>
        ///  Process the query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public List<Result> Query(Query query)
        {
            List<Result> results = new List<Result>();
            LyncManager lyncManager = new LyncManager();
            List<Contact> contacts;
            string searchName;
            AutomationModalities mode;

            // do not search when no parameters
            if (query.ActionParameters.Count == 0)
            {
                return results;
            }

            // check for action keywords
            if ("im".Equals(query.ActionParameters[0]))
            {
                // instant messaging
                mode = AutomationModalities.InstantMessage;
                query.ActionParameters[0] = "";
            }
            else if ("call".Equals(query.ActionParameters[0]))
            {
                // audio call
                mode = AutomationModalities.Audio;
                query.ActionParameters[0] = "";
            }
            else
            {
                // default
                mode = AutomationModalities.InstantMessage;
            }

            // concat search parameter
            searchName = String.Empty;
            foreach (string param in query.ActionParameters)
            {
                searchName += " " + param;
            }            

            // search in lync for contacts
            lyncManager.SearchForGroupOrContact(searchName, 20);
            contacts = new List<Contact>(lyncManager._SearchResultSubscription.Contacts);

            // create results for contacts
            foreach (Contact contact in contacts)
            {
                results.Add(new Result()
                {
                    Title = contact.GetContactInformation(ContactInformationType.DisplayName).ToString() 
                            + ", " + contact.GetContactInformation(ContactInformationType.Department).ToString(),
                    SubTitle = contact.GetContactInformation(ContactInformationType.Activity).ToString(),
                    IcoPath = "Images\\app.png",
                    Action = e =>
                    {
                        try
                        {
                            lyncManager.StartConversation(contact, mode);
                        }
                        catch
                        {
                            return false;
                        }
                        return true;
                    }
                });
            }
            return results;
        }

        /// <summary>
        ///  
        /// </summary>
        /// <param name="context"></param>
        public void Init(PluginInitContext context)
        {
        }
    }
}
