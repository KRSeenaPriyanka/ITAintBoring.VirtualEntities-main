using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Data.Exceptions;
using Microsoft.Xrm.Sdk.Extensions;
using Microsoft.Xrm.Sdk.Query;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace VirtualEntityProvider
{
    public class RetrieveOtherOrgData : IPlugin
    {
        //set these values for your D365 instance, user credentials and Azure AD clientid/token endpoint
        string crmorg = "https://online24x7uat.crm8.dynamics.com";
        string clientid = "XXXXXXXXX";
        string username = "lucasalexander@XXXXXX.onmicrosoft.com";
        string userpassword = "XXXXXXXXXXXX";
        string tokenendpoint = "https://login.microsoftonline.com/XXXXXXXXXXX/oauth2/token";

        //relative path to web api endpoint
        string crmwebapi = "/api/data/v8.2";

        //web api query to execute - in this case all accounts that start with "F"
        string crmwebapipath = "/accounts?$select=name,accountid&$filter=startswith(name,'F')";

        public void Execute(IServiceProvider serviceProvider)
        {
            //basic plugin set-up stuff
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory servicefactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = servicefactory.CreateOrganizationService(context.UserId);
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

            try
            {
                //instantiate a new entity collection to hold the records we'll return later
                EntityCollection results = new EntityCollection();

                //build the authorization request for Azure AD
                var reqstring = "client_id=" + clientid;
                reqstring += "&resource=" + Uri.EscapeUriString(crmorg);
                reqstring += "&username=" + Uri.EscapeUriString(username);
                reqstring += "&password=" + Uri.EscapeUriString(userpassword);
                reqstring += "&grant_type=password";

                //make the Azure AD authentication request
                WebRequest req = WebRequest.Create(tokenendpoint);
                req.ContentType = "application/x-www-form-urlencoded";
                req.Method = "POST";
                byte[] bytes = System.Text.Encoding.ASCII.GetBytes(reqstring);
                req.ContentLength = bytes.Length;
                System.IO.Stream os = req.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);
                os.Close();

                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                StreamReader tokenreader = new StreamReader(resp.GetResponseStream());
                string responseBody = tokenreader.ReadToEnd();
                tokenreader.Close();

                //deserialize the Azure AD token response and get the access token to supply with the web api query
                var tokenresponse = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(responseBody);
                var token = tokenresponse["access_token"];

                //make the web api query
                WebRequest crmreq = WebRequest.Create(crmorg + crmwebapi + crmwebapipath);
                crmreq.Headers = new WebHeaderCollection();

                //use the access token from earlier as the authorization header bearer value
                crmreq.Headers.Add("Authorization", "Bearer " + token);
                crmreq.Headers.Add("OData-MaxVersion", "4.0");
                crmreq.Headers.Add("OData-Version", "4.0");
                crmreq.Headers.Add("Prefer", "odata.maxpagesize=500");
                crmreq.Headers.Add("Prefer", "odata.include-annotations=OData.Community.Display.V1.FormattedValue");
                crmreq.ContentType = "application/json; charset=utf-8";
                crmreq.Method = "GET";

                HttpWebResponse crmresp = (HttpWebResponse)crmreq.GetResponse();
                StreamReader crmreader = new StreamReader(crmresp.GetResponseStream());
                string crmresponseBody = crmreader.ReadToEnd();
                crmreader.Close();

                //deserialize the response
                var crmresponseobj = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(crmresponseBody);

                //loop through the response values
                foreach (var row in crmresponseobj["value"].Children())
                {
                    //create a new virtual entity of type lpa_demove
                    Entity verow = new Entity("lpa_otheraccount");
                    //verow["lpa_otheraccountid"] = Guid.NewGuid();
                    //verow["lpa_name"] = ((Newtonsoft.Json.Linq.JValue)row["name"]).Value.ToString();
                    verow["lpa_otheraccountid"] = (Guid)row["accountid"];
                    verow["lpa_name"] = (string)row["name"];

                    //add it to the collection
                    results.Entities.Add(verow);
                }

                //return the results
                context.OutputParameters["BusinessEntityCollection"] = results;
            }
            catch (Exception e)
            {
                tracingService.Trace($"{e.Message} {e.StackTrace}");
                if (e.InnerException != null)
                    tracingService.Trace($"{e.InnerException.Message} {e.InnerException.StackTrace}");

                throw new InvalidPluginExecutionException(e.Message);
            }
        }
    }
}