﻿using eTRIKS.Commons.Core.Application.AccountManagement;
using eTRIKS.Commons.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.EntityFrameworkCore;

namespace eTRIKS.Commons.Service.Services.UserManagement
{
    public class UserStore : IUserStore<UserAccount>, IUserPasswordStore<UserAccount>
    {
        private readonly IServiceUoW _unitOfWork;
        private readonly IUserRepository _userRepository;
        private readonly IUserAccountRepository _accountRepository;
        public IdentityErrorDescriber ErrorDescriber { get; set; }
        private bool _disposed;

        public UserStore(IServiceUoW uoW, IdentityErrorDescriber describer = null)
        {
            _unitOfWork = uoW;
            _userRepository = uoW.GetUserRepository();
            _accountRepository = uoW.GetUserAccountRepository();
            ErrorDescriber = describer ?? new IdentityErrorDescriber();
        }

        public Task SetPasswordHashAsync(UserAccount account, string passwordHash, CancellationToken cancellationToken)
        {
            account.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(UserAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
                throw new ArgumentNullException("UserAccount");
            return Task.FromResult<string>(account.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(UserAccount user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task<string> GetUserIdAsync(UserAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
                throw new ArgumentNullException("UserAccount");
            return Task.FromResult<string>(account.UserId.ToString());
        }

        public Task<string> GetUserNameAsync(UserAccount account, CancellationToken cancellationToken)
        {
            if (account == null)
                throw new ArgumentNullException("UserAccount");
            return Task.FromResult<string>(account.UserName);
        }

        public Task SetUserNameAsync(UserAccount user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.FromResult(true);
        }

        public Task<string> GetNormalizedUserNameAsync(UserAccount user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(UserAccount user, string normalizedName, CancellationToken cancellationToken)
        {
            user.UserName = normalizedName;
            return Task.FromResult(true);
        }

        public Task<IdentityResult> CreateAsync(UserAccount userAccount, CancellationToken cancellationToken)
        {
            _userRepository.Insert(userAccount.User);
            _accountRepository.Insert(userAccount);
            _unitOfWork.SaveChangesAsync();
            return Task.FromResult(IdentityResult.Success);
        }

        public async Task<IdentityResult> UpdateAsync(UserAccount user, CancellationToken cancellationToken)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            try
            {
                 _accountRepository.Update(user);
                 await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            return IdentityResult.Success;
        }

        public Task<IdentityResult> DeleteAsync(UserAccount user, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<UserAccount> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<UserAccount> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return await _accountRepository.FindByUserNameAsync(normalizedUserName);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
                //handle.Dispose();
            _disposed = true;
        }

        //public void Dispose()
        //{
        //    throw new NotImplementedException();
        //}
        //public Task CreateAsync(UserAccount userAccount)
        //{
        //    _userRepository.Insert(userAccount.Account.User);
        //    _accountRepository.Insert(userAccount.Account);
        //    return _unitOfWork.SaveChangesAsync();
        //}

        //public Task DeleteAsync(UserAccount user)
        //{
        //    throw new NotImplementedException();
        //}

        //public void Dispose()
        //{
        //    //DI takes care of that
        //}

        //public async Task<UserAccount> FindByIdAsync(Guid userId)
        //{
        //    Account appUser = await _accountRepository.GetAsync(userId);
        //    return appUser != null ? new UserAccount(appUser) : null;
        //}

        //public async Task<UserAccount> FindByNameAsync(string userName)
        //{
        //    Account appuser = await _accountRepository.FindByUserNameAsync(userName);
        //    return appuser!=null ? new UserAccount(appuser) : null;
        //}

        //public Task UpdateAsync(UserAccount user)
        //{
        //    throw new NotImplementedException();
        //}

        //public Task SetPasswordHashAsync(UserAccount user, string passwordHash)
        //{
        //    user.Account.PasswordHash = passwordHash;
        //    return Task.FromResult(0);
        //}

        //public Task<string> GetPasswordHashAsync(UserAccount user)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException("user");
        //    return Task.FromResult<string>(user.Account.PasswordHash);
        //}

        //public Task<bool> HasPasswordAsync(UserAccount user)
        //{
        //    if (user == null)
        //        throw new ArgumentNullException("user");
        //    return Task.FromResult<bool>(!string.IsNullOrWhiteSpace(user.Account.PasswordHash));
        //}



        //private Account GetAccount(UserAccount userAccount)
        //{
        //    return new Account()
        //    {
        //        AdminApproved = userAccount.AdminApproved,
        //        JoinDate = userAccount.JoinDate,
        //        PasswordHash = userAccount.PasswordHash,
        //        UserName = userAccount.UserName,
        //        PSK = userAccount.PSK,
        //        SecurityStamp = userAccount.SecurityStamp,
        //        EmailConfirmed = userAccount.EmailConfirmed,
        //        TwoFactorEnabled = userAccount.TwoFactorEnabled,
        //        UserId = userAccount.UserId,
        //        Claims = userAccount.Claims,
        //        User = userAccount.User
        //    };
        //}
    }
}
