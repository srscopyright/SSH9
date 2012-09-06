// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebServices.Copyright.WebApplication.Models
{
    public class Copyright
    {

        private string _repositoryUrl="";
        private bool _activeStatus=false;
        private string _copyrightText = "Copyright 2007 - [currentYear] Service Repair Solutions, Inc.";
        private string _projectName="";
        private string _repositoryId="";
        private string _repositoryName="";
        private string _forceMessage="";
        private string _errorMessage="";
        private bool _defaultActiveStatus=false;
        private string _runningStatus = "";
        private string _controlType="GIT";
        private string _emailreport = "";
        public Copyright()
        {
            this.ProjectName = "";
            this.RepositoryId = "";
            this.ActiveStatus = false;
            this.CopyrightText = "Copyright 2007 - [currentYear] Service Repair Solutions, Inc.";
            this.DefaultActiveStatus = false;
            this.ErrorMessage = "";
            this.ForceMessage = "";
            this.RepositoryName = "";
            this.RepositoryUrl = "";
            this.RunningStatus = "";
        }

        [Required(ErrorMessage = "Repository Url is required")]
        public string RepositoryUrl
        {
            get { return _repositoryUrl; }
            set { _repositoryUrl = value; }
        }

        public bool ActiveStatus
        {
            get { return _activeStatus; }
            set { _activeStatus = value; }
        }
         [Required(ErrorMessage = "Copyright text is required")]
         [DataType(DataType.MultilineText)]
        public string CopyrightText
        {
            get { return _copyrightText; }
            set { _copyrightText = value; }
        }

        public string ProjectName
        {
            get { return _projectName; }
            set { _projectName = value; }
        }

        public string RepositoryId
        {
            get { return _repositoryId; }
            set { _repositoryId = value; }
        }

        public string RepositoryName
        {
            get { return _repositoryName; }
            set { _repositoryName = value; }
        }

        public bool DefaultActiveStatus
        {
            get { return _defaultActiveStatus; }
            set { _defaultActiveStatus = value; }
        }

        public string ForceMessage
        {
            get { return _forceMessage; }
            set { _forceMessage = value; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        public string RunningStatus
        {
            get { return _runningStatus; }
            set { _runningStatus = value; }
        }

        public string ControlType
        {
            get { return _controlType; }
            set { _controlType = value; }
        }

        public string Emailreport
        {
            get { return _emailreport; }
            set { _emailreport = value; }
        }
    }
}
