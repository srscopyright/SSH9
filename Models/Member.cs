// =====COPYRIGHT=====
// Copyright 2007 - 2012 Service Repair Solutions, Inc.
// =====COPYRIGHT=====
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebServices.Copyright.WebApplication.Models
{

    public class Member
    {
        private string _memberId;
        private string _memberName;
        private string _teamName;
        private string _teamId;
        private GitHubAPI.TeamPermission _permission;

        public string MemberId
        {
            get { return _memberId; }
            set { _memberId = value; }
        }

        public string MemberName
        {
            get { return _memberName; }
            set { _memberName = value; }
        }

        public string TeamName
        {
            get { return _teamName; }
            set { _teamName = value; }
        }

        public string TeamId
        {
            get { return _teamId; }
            set { _teamId = value; }
        }

        public GitHubAPI.TeamPermission Permission
        {
            get { return _permission; }
            set { _permission = value; }
        }
    }
}
