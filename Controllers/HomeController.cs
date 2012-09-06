// =====COPYRIGHT=====
// github ssh copyright text123
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Srs.WebPlatform.Common;
using WebServices.Copyright.WebApplication.CopyrightService;
using WebServices.Copyright.WebApplication.Models;


namespace WebServices.Copyright.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private static readonly string AssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        private static readonly string AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string RepoUrl { get; set; }
        public string CopyrightText { get; set; }
        public static string ProjectName = "";
        //
        // GET: /Home/
        /// <summary>
        /// Get copyright configuration information with specific projectID
        /// </summary>
        /// <param name="id">ProjectID</param>
        /// <returns></returns>
        public ActionResult Index(string project)
        {
            try
            {
                ProjectName = project;
                ViewData["control_type"] = new SelectList(new List<SelectListItem>
                                               {
                                                   new SelectListItem()
                                                       {Selected = false, Text = "GitHub", Value = "GIT"},
                                                   new SelectListItem()
                                                       {
                                                           Selected = false,
                                                           Text = "Team Foundation System",
                                                           Value = "TFS"
                                                       }
                                               }, "Value", "Text", "GIT");
                if (!string.IsNullOrEmpty(project))
                {
                    Models.Copyright copy = Dal.CopyrightDb.LoadDbCopyrightConfigByProjectId(project);
                    if (copy == null)
                    {
                        copy = new Models.Copyright();
                    }
                    else
                    {
                        ViewData["control_type"] = new SelectList(new List<SelectListItem>
                                               {
                                                   new SelectListItem()
                                                       {
                                                           Selected =false, 
                                                           Text = "GitHub",
                                                           Value = "GIT"
                                                       },
                                                   new SelectListItem()
                                                       {
                                                           Selected =false,
                                                           Text = "Team Foundation System",
                                                           Value = "TFS"
                                                       }
                                               }, "Value", "Text", copy.ControlType.Trim());
                        CopyrightServiceV1Client copyrightService = new CopyrightServiceV1Client("BasicHttpBinding_ICopyrightServiceV1");
                        if (copyrightService.IsProcessing(copy.RepositoryUrl))
                        {
                            copy.ForceMessage = "Forcing";
                            copy.RunningStatus = copyrightService.GetWorkingItemStatus(copy.RepositoryUrl);
                        }
                    }
                    copy.ProjectName = project;
                    if (copy.RunningStatus == "")
                    {
                        copy.RunningStatus = " waiting for next run";
                    }
                    return View(copy);
                }
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }
            return View();
        }
        /// <summary>
        /// This method will handle for save and force copyright submit
        /// </summary>
        /// <param name="copyRight">Copyright entity object</param>
        /// <param name="btnForce"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult Index(Copyright.WebApplication.Models.Copyright copyRight, string btnForce, string btnSave, string ddlSourceType, string Emailreport)
        {
            CopyrightServiceV1Client copyrightService = new CopyrightServiceV1Client("BasicHttpBinding_ICopyrightServiceV1");
            try
            {

                copyRight.ControlType = ddlSourceType;
                if (!string.IsNullOrEmpty(btnSave))
                {
                    if (ModelState.IsValid)
                    {
                        if (copyRight.ControlType == "GIT")
                        {
                            if (copyrightService.IsExistRepository(copyRight.RepositoryUrl).Id == 1)//copyrightService.IsExistRepository(copyRight.RepositoryUrl)
                            {
                               
                                Dal.CopyrightDb.SaveDbCopyrightConfig(copyRight);
                                return JavaScript("SaveSuccessfullMsg();");
                            }
                            else
                            {
                                copyrightService.SendEmailReport(Emailreport, "Unable to access the Git repository URL: " + copyRight.RepositoryUrl + "<br/>Please verify that the specified URL is accurate. ");
                                return JavaScript("SaveUnSuccessfullMsg();");
                            }
                        }
                        else
                        {
                            if (copyrightService.IsExistSourcePath(copyRight.RepositoryUrl))
                            {
                                Dal.CopyrightDb.SaveDbCopyrightConfig(copyRight);
                                return JavaScript("SaveSuccessfullMsg();");
                            }
                            else
                            {
                                copyrightService.SendEmailReport(Emailreport, "Unable to access the TFS Path: " + copyRight.RepositoryUrl + "<br/>Please verify that the specified path is accurate. ");
                                return JavaScript("SaveUnSuccessfullMsg();");
                            }
                        }

                    }
                }
                else if (!string.IsNullOrEmpty(btnForce))
                {
                    if (!copyrightService.IsProcessing(copyRight.RepositoryUrl))
                    {
                        ObjectResponseV1 response = new ObjectResponseV1();
                        bool isExist = false;
                        if (copyRight.ControlType == "GIT")
                        {
                            response = copyrightService.IsExistRepository(copyRight.RepositoryUrl);
                        }
                        else
                        {
                            isExist = copyrightService.IsExistSourcePath(copyRight.RepositoryUrl);
                        }
                        if (isExist || response.Id == 1)
                        {
                            if (copyRight.ControlType == "GIT")
                            {
                                RepoUrl = copyRight.RepositoryUrl;
                                CopyrightText = copyRight.CopyrightText;
                                Thread thread = new Thread(unused => StartThread(copyRight.RepositoryUrl, copyRight.CopyrightText, Emailreport));
                                thread.Start();
                            }
                            else
                            {
                                RepoUrl = copyRight.RepositoryUrl;
                                CopyrightText = copyRight.CopyrightText;
                                Thread thread = new Thread(unused => StartTfsThread(copyRight.ProjectName, copyRight.RepositoryUrl, copyRight.CopyrightText, Emailreport));
                                thread.Start();
                            }
                        }
                        else
                        {
                            if (copyRight.ControlType == "GIT")
                            {
                                copyrightService.SendEmailReport(Emailreport,"Unable to access the Git repository URL: " +copyRight.RepositoryUrl +"<br/>Please verify that the specified URL is accurate. ");
                            }
                            else
                            {
                                copyrightService.SendEmailReport(Emailreport, "Unable to access the TFS Path: " + copyRight.RepositoryUrl + "<br/>Please verify that the specified path is accurate. ");
                            }
                            return JavaScript("ForceFailMsg('" + response.Description + "');");
                        }
                    }
                    else
                    {
                        return JavaScript("IsRunningMsg();");
                    }
                    copyRight.ForceMessage = "Forcing";
                    return JavaScript("window.location.reload();");
                }
            }
            catch (Exception ex)
            {
                copyrightService.SendEmailReport(Emailreport, ex.Message + ex.StackTrace);
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
                return JavaScript("ForceFailMsg('" + ex.Message + "');");
            }
            return View(copyRight);
        }

        private void StartThread(string pRepoUrl, string pCopyrightText, string pEmail)
        {
            CopyrightServiceV1Client copyrightService = new CopyrightServiceV1Client("BasicHttpBinding_ICopyrightServiceV1");
            copyrightService.CallFileHeaderEnforcement(pRepoUrl, pCopyrightText, pEmail);
        }
        private void StartTfsThread(string pProjectName, string pRepoUrl, string pCopyrightText, string pEmail)
        {
            CopyrightServiceV1Client copyrightService = new CopyrightServiceV1Client("BasicHttpBinding_ICopyrightServiceV1");
            copyrightService.CallFileHeaderTfs(pProjectName, pRepoUrl, pCopyrightText, pEmail);
        }

        private void StartActiveRepo()
        {
            List<Models.Copyright> lstCopyright = Dal.CopyrightDb.LoadActiveRepo();
            CopyrightServiceV1Client copyrightService = new CopyrightServiceV1Client("BasicHttpBinding_ICopyrightServiceV1");
            foreach (var copyright in lstCopyright)
            {
                if (!copyrightService.IsProcessing(copyright.RepositoryUrl))
                {
                    copyrightService.CallFileHeaderEnforcement(copyright.RepositoryUrl, copyright.CopyrightText, copyright.Emailreport);
                }
                //copyright.ForceMessage = "Forcing";
            }
        }
        /// <summary>
        /// This method will check from data for all active repository and call force copyright to all ones
        /// </summary>
        /// <returns></returns>
        public ActionResult ForceAllActiveRepo()
        {
            try
            {
                Thread thread = new Thread(StartActiveRepo);
                thread.Start();
            }
            catch (Exception ex)
            {
                LoggerHelperV1.Error(AssemblyName, AssemblyVersion, Environment.MachineName, ex.StackTrace, ex.Message + ex.StackTrace);
            }
            return View();
        }
    }
}
