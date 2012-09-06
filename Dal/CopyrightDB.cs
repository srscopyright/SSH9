// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Microsoft.Practices.EnterpriseLibrary.Data;
using Srs.WebPlatform.Common;
using WebServices.Copyright.WebApplication.NifDBClient;
using WebServices.Copyright.WebApplication.SessionTokenClient;

namespace WebServices.Copyright.WebApplication.Dal
{
    public static class CopyrightDb
    {
        #region "Constances"
        private const string DatabaseConnectionString = "copyright_db";
        private const string StoreSaveCopyrightConfiguration = "SaveCopyrightConfiguration";
        private const string StoreLoadCopyrightConfigByProjectId = "LoadCopyrightConfigurationByProjectName";
        private const string StoreLoadActiveRepo = "LoadActiveRepo";
        private const string ContsProjectName = "projectName";
        private const string ContsActiveStatus = "activeStatus";
        private const string ContsCopyrightText = "copyrightText";
        private const string ContsRepositoryUrl = "repositoryURL";
        private const string ContsControlType = "controlType";
        private const string ContsEmailReport = "emailReport";
        private const string ContsNifDbKey = "CopyrightServiceNifDBKey";
        private const string ConstGet = "Get";
        #endregion
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        private static void WriteLogFile(string msg)
        {
            StreamWriter sr = new StreamWriter("C:\\CopyrightLog.txt");
            sr.NewLine = DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + ":" + msg + "/r/n";
            sr.WriteLine(DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss") + ":" + msg + "/r/n");
            sr.Flush();
            sr.Close();
        }
        /// <summary>
        /// Method to convert a custom Object to XML string
        /// </summary>
        /// <param name="lstcopyright">Object that is to be serialized to XML</param>
        /// <returns>XML string</returns>
        private static String SerializeObject(List<Models.Copyright> lstcopyright)
        {

            try
            {

                String xmlizedString = null;

                MemoryStream memoryStream = new MemoryStream();

                XmlSerializer xs = new XmlSerializer(typeof(List<Models.Copyright>));

                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);



                xs.Serialize(xmlTextWriter, lstcopyright);

                memoryStream = (MemoryStream)xmlTextWriter.BaseStream;

                xmlizedString = Utf8ByteArrayToString(memoryStream.ToArray());

                return xmlizedString;

            }

            catch (Exception e)
            {

                System.Console.WriteLine(e);

                return null;

            }

        }
        /// <summary>
        /// Method to reconstruct an Object from XML string
        /// </summary>
        /// <param name="pXmlizedString"></param>
        /// <returns></returns>
        private static List<Models.Copyright> DeserializeObject(String pXmlizedString)
        {
            XmlSerializer xs = new XmlSerializer(typeof(List<Models.Copyright>));
            MemoryStream memoryStream = new MemoryStream(StringToUtf8ByteArray(pXmlizedString));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return (List<Models.Copyright>)xs.Deserialize(memoryStream);
        }
        /// <summary>
        /// To convert a Byte Array of Unicode values (UTF-8 encoded) to a complete String.
        /// </summary>
        /// <param name="characters">Unicode Byte Array to be converted to String</param>
        /// <returns>String converted from Unicode Byte Array</returns>
        private static String Utf8ByteArrayToString(Byte[] characters)
        {

            UTF8Encoding encoding = new UTF8Encoding();

            String constructedString = encoding.GetString(characters);

            return (constructedString);

        }
        /// <summary>
        /// Converts the String to UTF8 Byte array and is used in De serialization
        /// </summary>
        /// <param name="pXmlString"></param>
        /// <returns></returns>
        private static Byte[] StringToUtf8ByteArray(String pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            Byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }

        private static string GetSessionTokenGuidId(string username, string password)
        {
            string id = "";
            try
            {
                string sessionUrl = System.Configuration.ConfigurationManager.AppSettings["SessionTokenAddress"];
                string requestUrl = sessionUrl + "/POX/ISessionTokenServiceV2/CreateSessionToken?userName=" + username +
                                    "&password=" + password;
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(requestUrl);
                webrequest.Method = ConstGet;
                HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();
                XDocument xmlDocument = null;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Stream stream = response.GetResponseStream();
                    if (stream != null)
                    {
                        string xml = new StreamReader(stream).ReadToEnd();

                        if (!string.IsNullOrEmpty(xml)) xmlDocument = XDocument.Parse(xml);
                    }
                }
                if (xmlDocument != null)
                    id = xmlDocument.ToString().Substring(xmlDocument.ToString().IndexOf("<a:AuthorizedSessionToken>"),
                                                         xmlDocument.ToString().IndexOf("</a:AuthorizedSessionToken>") -
                                                         xmlDocument.ToString().IndexOf("<a:AuthorizedSessionToken>")).
                            Replace("<a:AuthorizedSessionToken>", "");
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                throw ex;
            }
            return id;
        }

        /// <summary>
        /// Save Copyright Configuration
        /// </summary>
        /// <param name="copyright"></param>
        /// <returns></returns>
        public static int SaveCopyrightConfiguration(Models.Copyright copyright)
        {
            int res = 0;
            try
            {
                //check exist data with CopyrightServiceKey
                List<Models.Copyright> lstCopyright = GetAllCopyright();

                if (lstCopyright == null)
                {
                    lstCopyright = new List<Models.Copyright>();
                }
                int index = lstCopyright.FindIndex(p => p.ProjectName.ToLower() == copyright.ProjectName.ToLower());
                if (index < 0)
                {
                    lstCopyright.Add(copyright);
                }
                else
                {
                    lstCopyright[index] = copyright;
                }

                string result = SaveCopyrightToNifDb(lstCopyright);

            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                throw ex;
            }
            return res;
        }
        private static string SaveCopyrightToNifDb(List<Models.Copyright> lstCopyright)
        {
            string res = "";
            SessionTokenClient.SessionTokenServiceV2Client sessionToken = new SessionTokenServiceV2Client("BasicHttpBinding_ISessionTokenServiceV2");
            string username = System.Configuration.ConfigurationManager.AppSettings["SessionUserName"];
            string password = System.Configuration.ConfigurationManager.AppSettings["SessionPassword"];
            string bucketnamespace = System.Configuration.ConfigurationManager.AppSettings["BucketNameSpace"];
            string bucketpassword = System.Configuration.ConfigurationManager.AppSettings["BucketPassword"];
            string sessionTokenID = GetSessionTokenGuidId(username, password);
            NifDBClient.KeyBaseServiceV1Client NifDB = new KeyBaseServiceV1Client("BasicHttpBinding_IKeyBaseServiceV1");
            string xml = SerializeObject(lstCopyright);
            res = NifDB.SaveString(sessionTokenID, bucketnamespace, bucketpassword, ContsNifDbKey, xml);
            return res;
        }
        private static List<Models.Copyright> GetAllCopyright()
        {
            List<Models.Copyright> lstCopyright = new List<Models.Copyright>();
            try
            {
                SessionTokenClient.SessionTokenServiceV2Client sessionToken =
                    new SessionTokenServiceV2Client("BasicHttpBinding_ISessionTokenServiceV2");
                string username = System.Configuration.ConfigurationManager.AppSettings["SessionUserName"];
                string password = System.Configuration.ConfigurationManager.AppSettings["SessionPassword"];
                string bucketnamespace = System.Configuration.ConfigurationManager.AppSettings["BucketNameSpace"];
                string bucketpassword = System.Configuration.ConfigurationManager.AppSettings["BucketPassword"];
                string sessionTokenID = GetSessionTokenGuidId(username, password);
                NifDBClient.KeyBaseServiceV1Client NifDB =
                    new KeyBaseServiceV1Client("BasicHttpBinding_IKeyBaseServiceV1");

                string xml =
                    (string)NifDB.GetString(sessionTokenID, bucketnamespace, bucketpassword, ContsNifDbKey);
                if (xml != null)
                {
                    lstCopyright = DeserializeObject(xml);
                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                return null;
            }
            return lstCopyright;
        }
        /// <summary>
        /// Load Copyright Configuration by specific ProjectId
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static Models.Copyright LoadCopyrightConfigByProjectId(string projectName)
        {
            Models.Copyright copyright = null;
            try
            {
                //Get copyright data from NifDB
                List<Models.Copyright> lstCopyright = GetAllCopyright();
                if (lstCopyright != null && lstCopyright.Count > 0)
                {
                    copyright = (from item in lstCopyright
                                 where item.ProjectName.ToLower() == projectName.ToLower()
                                 select item).First();
                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                return null;
            }
            return copyright;
        }
        /// <summary>
        /// Load Copyright Configuration by specific ProjectId from Database
        /// </summary>
        /// <param name="projectName"></param>
        /// <returns></returns>
        public static Models.Copyright LoadDbCopyrightConfigByProjectId(string projectName)
        {
            Models.Copyright copyright = null;
            try
            {
                Database db = DatabaseFactory.CreateDatabase(DatabaseConnectionString);
                DbCommand dbCommand = db.GetStoredProcCommand(StoreLoadCopyrightConfigByProjectId);
                dbCommand.Parameters.Add(new SqlParameter("projectName", projectName));
                IDataReader dr = db.ExecuteReader(dbCommand);
                while (dr.Read())
                {
                    copyright = new Models.Copyright();
                    copyright.ProjectName = dr[ContsProjectName].ToString();
                    copyright.ActiveStatus = Convert.ToBoolean(dr[ContsActiveStatus]);
                    copyright.CopyrightText = dr[ContsCopyrightText].ToString();
                    copyright.RepositoryUrl = dr[ContsRepositoryUrl].ToString();
                    copyright.ControlType = dr[ContsControlType].ToString();
                    copyright.Emailreport = dr[ContsEmailReport].ToString();
                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                WriteLogFile(ex.Message + ex.StackTrace);
                return null;
            }
            return copyright;
        }
        /// <summary>
        /// Save Copyright Configuration to Database
        /// </summary>
        /// <param name="copyright"></param>
        /// <returns></returns>
        public static bool SaveDbCopyrightConfig(Models.Copyright copyright)
        {
            try
            {
                Database db = DatabaseFactory.CreateDatabase(DatabaseConnectionString);
                DbConnection connection = db.CreateConnection();
                DbCommand dbCommand = db.GetStoredProcCommand(StoreSaveCopyrightConfiguration);
                dbCommand.Connection = connection;
                dbCommand.Connection.Open();
                dbCommand.Parameters.Add(new SqlParameter("projectName", copyright.ProjectName));
                dbCommand.Parameters.Add(new SqlParameter("activeStatus", copyright.ActiveStatus));
                dbCommand.Parameters.Add(new SqlParameter("copyRightText", copyright.CopyrightText));
                dbCommand.Parameters.Add(new SqlParameter("repositoryURL", copyright.RepositoryUrl));
                dbCommand.Parameters.Add(new SqlParameter("controlType", copyright.ControlType));
                dbCommand.Parameters.Add(new SqlParameter("emailreport", copyright.Emailreport == null ? "" : copyright.Emailreport));
                dbCommand.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                WriteLogFile(ex.Message + ex.StackTrace);
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                WriteLogFile(ex.Message + ex.StackTrace);
                return false;
            }
        }
        /// <summary>
        /// Load all active copyright configuration
        /// </summary>
        /// <returns></returns>
        public static List<Models.Copyright> LoadActiveRepo()
        {
            List<Models.Copyright> lstCopyRight = new List<Models.Copyright>();
            try
            {
                Database db = DatabaseFactory.CreateDatabase(DatabaseConnectionString);
                DbCommand dbCommand = db.GetStoredProcCommand(StoreLoadActiveRepo);
                IDataReader dr = db.ExecuteReader(dbCommand);
                while (dr.Read())
                {
                    Models.Copyright copyright = new Models.Copyright();
                    //copyright.ProjectName = dr[ContsProjectName].ToString();
                    //copyright.ActiveStatus = Convert.ToBoolean(dr[ContsActiveStatus]);
                    copyright.CopyrightText = dr[ContsCopyrightText].ToString();
                    copyright.RepositoryUrl = dr[ContsRepositoryUrl].ToString();
                    lstCopyRight.Add(copyright);
                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                throw ex;
            }
            return lstCopyRight;
        }
    }
}
