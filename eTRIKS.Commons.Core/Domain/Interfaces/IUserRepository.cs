﻿using eTRIKS.Commons.Core.Domain.Model.Users;
using System;
using System.Threading.Tasks;
namespace eTRIKS.Commons.Core.Domain.Interfaces
{
    public interface IUserRepository : IRepository<User,Guid>
    {
        //Task<TEntity> FindUser(string userName, string password);
        //Task<TResult> RegisterUser(TEntity entity, string password);
        User FindByUserName(string username);
        Task<User> FindByUserNameAsync(string username);
        //Task<TEntity> FindByUserNameAsync(CancellationToken cancellationToken, string username);

        User FindByEmail(string email);
        Task<User> FindByEmailAsync(string email);
        //Task<User> FindByEmailAsync(CancellationToken cancellationToken, string email);
    }
}
