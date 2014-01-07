using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.IO;
using System.Xml;
using System.Security.Cryptography.X509Certificates;




namespace WpfApplication1
{
    class Agent
    {
        public string vm_namespace = "vim25";
        public string soap_action = "urn:vim25/test";
        public string soap_header = "<?xml version='1.0' encoding='UTF-8'?><soapenv:Envelope xmlns:soapenv='http://schemas.xmlsoap.org/soap/envelope/' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance'><soapenv:Body>";
        public string soap_footer = "</soapenv:Body></soapenv:Envelope>";
        public string request_envelope = "";
        private string url = "";
        public Boolean loggedin = false;
        private System.Net.HttpWebRequest request;
        System.Net.CookieContainer cookie = new System.Net.CookieContainer();
        public Boolean reportProgress= false;
        private xml2db pk = new xml2db();
        public string vmdb;
        public string last_message = "";
        public string VMNames = "";
        
        public string login(string user, string pw, string host, System.ComponentModel.BackgroundWorker bw)
        {
            
            string ret = "Unknown error";
           // string ret1 = "";
            url = "https://" + host + "/sdk";
            string body_content = "<_this type='SessionManager'>ha-sessionmgr</_this><userName>" + user + "</userName><password>" + pw + "</password>";
            string request_envelope = soap_header + "<Login xmlns='urn:vim25'>" + body_content + "</Login>" + soap_footer;
            ret = sendRequest(request_envelope,bw);
            if (pk.login_success(ret))
            {
               // last_message="Logged in";
                loggedin = true;
                vmdb=getVM(bw);
               VMNames = pk.getVMNames(vmdb);
            }
            
             return "Logged in";

        }

        public string logout(System.ComponentModel.BackgroundWorker bw)
        {

            string ret = "Unknown error";
            string body_content = "<_this type='SessionManager'>ha-sessionmgr</_this>";
            string request_envelope = soap_header + "<Logout xmlns='urn:vim25'>" + body_content + "</Logout>" + soap_footer;
            ret = sendRequest(request_envelope,bw);
            //to do: check return actually IS logged OUT
            loggedin = false;
            last_message = "Logged out";
            return "Logged out";

        }

        public string  getHostInfo(System.ComponentModel.BackgroundWorker bw)
        {
              
             string msg = "Success";  
             string body_content="<_this type='PropertyCollector'>ha-property-collector</_this><specSet><propSet><type>HostSystem</type><all>1</all></propSet><objectSet><obj type='HostSystem'>ha-host</obj></objectSet></specSet>";
	         string request_envelope=soap_header +"<RetrieveProperties xmlns='urn:vim25'>" + body_content + "</RetrieveProperties>" + soap_footer;
             reportProgress = true;
             string ret=sendRequest(request_envelope,bw);
           //  pk.writeOut(ret, xml2db.XML);
             ret = pk.loadResultsasCSV(ret);
             last_message = "Copied host data to "+ ret;
             return msg;

        }

        public string getVM(System.ComponentModel.BackgroundWorker bw)
        {
           
            string ret = "No value";
          //  string msg = "Success"; 
            string xml_ret = retrieveProperties(bw);
            string body_content = "<_this type='PropertyCollector'>ha-property-collector</_this><specSet><propSet><type>VirtualMachine</type><all>1</all></propSet>";
            using (XmlReader reader = XmlReader.Create(new StringReader(xml_ret)))
            {
                while (reader.Read())
                {

                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Text:
                            body_content += "<objectSet><obj type='VirtualMachine'>" + reader.Value + "</obj></objectSet>";
                            break;
                    }

                }
              }

            body_content += "</specSet>";
            string request_envelope = soap_header + "<RetrieveProperties xmlns='urn:vim25'>" + body_content + "</RetrieveProperties>" + soap_footer;
            reportProgress = true;
            ret = sendRequest(request_envelope,bw);
            //last_message = ret;
          
            return ret;
        }


        public string getCurrentTime(System.ComponentModel.BackgroundWorker bw)
        {
             string ret = "No Value!";
             string body_content= "<_this type='ServiceInstance'>ServiceInstance</_this>";
  	         request_envelope=soap_header+"<CurrentTime xmlns='urn:vim25'>" + body_content + "</CurrentTime>" + soap_footer;
             ret = sendRequest(request_envelope,bw);
             string the_time = pk.getServerTime(ret);
             last_message = the_time;
             return the_time;
         }

        public string suspendVM(System.ComponentModel.BackgroundWorker bw)
        {
            string ret = "No Value!";
            int id = 7;
            string body_content = "<_this type='VirtualMachine'>"+id+"</_this>";
            request_envelope = soap_header + "<SuspendVM_Task xmlns='urn:vim25'>" + body_content + "</SuspendVM_Task>" + soap_footer;
            ret = sendRequest(request_envelope, bw);
            //  string ret1=pk.loadResults(ret);
            last_message = ret;
            return ret;
        }

        public string powerOnVM(System.ComponentModel.BackgroundWorker bw, string vmData)
        {
            string ret = "No Value!";

            string body_content = "<_this type='VirtualMachine'>" + vmData + "</_this>";
            request_envelope = soap_header + "<PowerOnVM_Task xmlns='urn:vim25'>" + body_content + "</PowerOnVM_Task>" + soap_footer;
            ret = sendRequest(request_envelope, bw);
            //  if success then do this
            ret="Powered On "+ vmData; 
            vmdb = getVM(bw);
            VMNames = pk.getVMNames(vmdb);
            last_message = ret;
            return ret;
        }

 

        private string retrieveProperties(System.ComponentModel.BackgroundWorker bw)
        {
            string ret = "";
            string body_content = "<_this type='PropertyCollector'>ha-property-collector</_this><specSet><propSet><type>VirtualMachine</type><all>0</all></propSet><objectSet><obj type='Folder'>ha-folder-root</obj><skip>0</skip><selectSet xsi:type='TraversalSpec'><name>folderTraversalSpec</name><type>Folder</type><path>childEntity</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet><selectSet><name>datacenterHostTraversalSpec</name></selectSet><selectSet><name>datacenterVmTraversalSpec</name></selectSet><selectSet><name>datacenterDatastoreTraversalSpec</name></selectSet><selectSet><name>datacenterNetworkTraversalSpec</name></selectSet><selectSet><name>computeResourceRpTraversalSpec</name></selectSet><selectSet><name>computeResourceHostTraversalSpec</name></selectSet><selectSet><name>hostVmTraversalSpec</name></selectSet><selectSet><name>resourcePoolVmTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>datacenterDatastoreTraversalSpec</name><type>Datacenter</type><path>datastoreFolder</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>datacenterNetworkTraversalSpec</name><type>Datacenter</type><path>networkFolder</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>datacenterVmTraversalSpec</name><type>Datacenter</type><path>vmFolder</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>datacenterHostTraversalSpec</name><type>Datacenter</type><path>hostFolder</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>computeResourceHostTraversalSpec</name><type>ComputeResource</type><path>host</path><skip>0</skip></selectSet><selectSet xsi:type='TraversalSpec'><name>computeResourceRpTraversalSpec</name><type>ComputeResource</type><path>resourcePool</path><skip>0</skip><selectSet><name>resourcePoolTraversalSpec</name></selectSet><selectSet><name>resourcePoolVmTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>resourcePoolTraversalSpec</name><type>ResourcePool</type><path>resourcePool</path><skip>0</skip><selectSet><name>resourcePoolTraversalSpec</name></selectSet><selectSet><name>resourcePoolVmTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>hostVmTraversalSpec</name><type>HostSystem</type><path>vm</path><skip>0</skip><selectSet><name>folderTraversalSpec</name></selectSet></selectSet><selectSet xsi:type='TraversalSpec'><name>resourcePoolVmTraversalSpec</name><type>ResourcePool</type><path>vm</path><skip>0</skip></selectSet></objectSet></specSet>";
            request_envelope = soap_header + "<RetrieveProperties xmlns='urn:vim25'>" + body_content + "</RetrieveProperties>" + soap_footer;
            ret = sendRequest(request_envelope,bw);
            return ret;
        }


       private string sendRequest(string req_xml, System.ComponentModel.BackgroundWorker bw)
        {
               int counter = 0; 
               string ret = "Unknown Error";
                XmlDocument soap_env = new XmlDocument();
                soap_env.LoadXml(req_xml);
                request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                request.Headers.Add("SOAPAction", soap_action);
                request.ContentType = "text/xml;charset=\"utf-8\"";
                request.Method = "POST";
                request.CookieContainer = cookie;
                request.KeepAlive = false; //true
                request.UserAgent = "Small Craft C#";

             //   Ignore SSL Error if using https
           System.Net.ServicePointManager.ServerCertificateValidationCallback = delegate(
                Object obj, X509Certificate certificate, X509Chain chain,
                System.Net.Security.SslPolicyErrors errors)
                {
                    return (true);
                };
         
                Stream stream = request.GetRequestStream();
                soap_env.Save(stream);

                try
                {
                    string buf = "";
                  
                    System.Net.WebResponse response = request.GetResponse();
                    Stream s = response.GetResponseStream();
                    StreamReader sr = new StreamReader(s);
                  
                    while (!sr.EndOfStream)
                    {
                        buf += sr.ReadLine();
                        counter++;
                        if (reportProgress) { 
                            bw.ReportProgress(counter); 
                        }
                    }

                    ret = buf;
                }

                catch (System.Net.WebException webExc)
                {
                    ret = webExc.ToString();
                }

            return ret;
        }




    }

    
}
