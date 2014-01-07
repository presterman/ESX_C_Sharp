using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;





namespace WpfApplication1
{
    
    class xml2db
    {

        public const int CSV = 0;
        public const int XML = 1;

      

        public string loadResultsasCSV(string buf)
        {
          XmlReader reader = XmlReader.Create(new System.IO.StringReader(buf)) ;
          string temp="";
          bool have_element = false;

          /*reader.Name gets the name of an opening and closing element name, so name get <this> and </this>
                 reader.Value get sdavid, which is between <this>sdavid</this>
        */ 
            while( reader.Read()) //read the next node
            {
             switch (reader.NodeType)
             {
                 
                 case XmlNodeType.Element:
                     string str = "";
                    if (have_element)
                     {
                        temp += ",  ," + reader.Depth + "\n"; ;

                     }

                      if(reader.HasAttributes)
                      {
                         
                          for (int i = 0; i < reader.AttributeCount; i++)
                          {
                              str += reader.GetAttribute(i) + " " ;

                          }

                      }
                      temp+=reader.Name+","+str;
                      have_element = true;
                      break;


                 case XmlNodeType.Text:

                     temp+=","+reader.Value.Replace(","," ") +","+reader.Depth+"\n";
                     have_element = false;
                     break;
              }

            }

            string ret=this.writeOut(temp,CSV); 
            return ret;
        }

        public string writeOut(string output, short style)
        {

            string times = System.Diagnostics.Stopwatch.GetTimestamp().ToString();
            string out_file = "";
            switch (style)
            {
                case CSV:
                 //   System.IO.File.WriteAllText(@"c:\aaa_HOST_new.csv", output); 
                    out_file = @"c:\" + times + ".csv";
                    System.IO.File.WriteAllText(out_file, output); 
                   break;
                case XML:
                   out_file = @"c:\" + times + ".xml";
                  System.IO.File.WriteAllText(@"c:\"+times+".xml", output);
                  break;
 
            }
          

            return out_file;

           
        }

        public bool login_success(string response)
        {

            bool r = false;
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(response));

            try
            {

                while (reader.Read()) //read the next node
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "key")
                                r = true;
                            break;
                    }

                }


            }

            catch(Exception ex)
            {

                ex.ToString();

            }

            return r;


        }

        public string getVMNames(string buf)
        {
          string ret="";

          XmlReader reader = XmlReader.Create(new System.IO.StringReader(buf));
             
          try
          {
              while (reader.Read()) //read the next node
              {
                  switch (reader.NodeType)
                  {
                      case XmlNodeType.Element:
                          if (reader.Name == "val")
                          {
                              if(reader.GetAttribute(0)=="VirtualMachineSummary")
                              { 
                                 ret += getVMData(reader.ReadOuterXml());
                              }
                          }

                         break;
                  }

              }


          }

          catch (Exception ex)
          {

              ret=ex.ToString();

          }

        


          return ret;

           

        }

        public string getServerTime(string buf)
        {
            string ret = "Time not found";

            XmlReader reader = XmlReader.Create(new System.IO.StringReader(buf));

            try
            {
                while (reader.Read()) //read the next node
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (reader.Name == "returnval")
                            {

                                ret = "Server time: "+reader.ReadInnerXml();
                                
                            }

                            break;
                    }

                }


            }

            catch (Exception ex)
            {

                ret = ex.ToString();

            }




            return ret;



        }



        private string getVMData(string xmlfrag)
        {
            string ret="";
            XmlReader reader = XmlReader.Create(new System.IO.StringReader(xmlfrag));

            try
            {
                while (reader.Read()) //read the next node
                {
                    switch (reader.NodeType)
                    {
                        case XmlNodeType.Element:
                              if (reader.Name == "vm")
                            {

                                ret += reader.ReadInnerXml() + ","; ;
                                
                            }

                            else if (reader.Name == "name")
                            {

                                ret += reader.ReadInnerXml() + ","; ;
                                
                            }
                            else if (reader.Name=="powerState")
                            {

                                ret += reader.ReadInnerXml() + ",";

                            }

                            break;
                    }

                }


            }

            catch (Exception ex)
            {

                ret = ex.ToString();

            }



            ret += ";";
            return ret;



        }



    }
}
