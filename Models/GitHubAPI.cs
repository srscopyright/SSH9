// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Web;

namespace WebServices.Copyright.WebApplication.Models
{
    public static class GitHubAPI
    {
        public enum TeamPermission
        {
            Read,
            Write
        }
        private static string _UserName = "";
        private static string _Password = "";
        public static void setCredential(string Username,string Password)
        {
            _UserName = Username;
            _Password = Password;
        }
        public static string CreateTeam(string teamName, string orgName, TeamPermission permission)
        {
            string id = "";
            try
            {
                string api = "https://api.github.com/orgs/" + orgName + "/teams";
                var webClient = new System.Net.WebClient();
                var cc = new CredentialCache();
                cc.Add(new Uri(api),
                       "Basic",
                       new NetworkCredential(_UserName, _Password));
                webClient.Credentials = cc;
                string data = "{\"name\":\""+teamName+"\",\"permission\":\""+(permission==TeamPermission.Read?"pull":"push")+"\"}";
                string res = webClient.UploadString(api, data);
                id = res.Substring(res.LastIndexOf(":") + 1).Replace("}", "");
                return id;
            }
            catch 
            {

            }
            return id;
        }
        public static void AddRepoToTeam(string teamID,string orgName,string repoName)
        {
           
            try
            {
                string api = "https://github.com/api/v2/json/teams/" + teamID + "/repositories?login=hnlam1986&token=1d095a1acc34a9bce4833f46551579e4";
                var webClient = new System.Net.WebClient();
                NameValueCollection FormValues = new NameValueCollection();
                FormValues.Add("name", orgName+"/"+repoName);
                byte[] res = webClient.UploadValues(api, "POST", FormValues);
                
            }
            catch
            {

            }
        }
    }
}
