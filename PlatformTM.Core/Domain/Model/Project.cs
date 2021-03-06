﻿using System;
using System.Collections.Generic;
using PlatformTM.Core.Domain.Model.Base;
using PlatformTM.Core.Domain.Model.DatasetModel;
using PlatformTM.Core.Domain.Model.Users;
using PlatformTM.Core.JoinEntities;

namespace PlatformTM.Core.Domain.Model
{
    public class Project : Identifiable<int>
    {
        public string Name { get; set;}
        public string Description { get; set;}
        public string Accession { get; set;}
        public string Title { get; set; }
        public string Type { get; set; }
        public ICollection<Study> Studies { get; set;}
        public ICollection<DataFile> DataFiles { get; set; } 

        public Project()
        {
            Studies = new List<Study>();
            Users = new List<ProjectUser>();
        }

        public ICollection<Activity> Activities { get; set; }
        //public IList<User> Users { get; set; }
        public IList<ProjectUser> Users { get; set; }
        public User Owner { get; set; }
        public Guid OwnerId { get; set; }
        public bool IsPublic { get; set; }
    }
}
