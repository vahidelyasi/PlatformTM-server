﻿using System;
using PlatformTM.Core.Domain.Model.Base;

namespace PlatformTM.Core.Application.AccountManagement
{
    public class UserClaim : Identifiable<int>
    {
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public Guid UserAccountId { get; set; }
        private UserAccount _userAccount;
        public UserAccount UserAccount
        {
            get { return _userAccount; }
            set
            {
                _userAccount = value;
                UserAccountId = value.Id;
            }
        }
    }
}
