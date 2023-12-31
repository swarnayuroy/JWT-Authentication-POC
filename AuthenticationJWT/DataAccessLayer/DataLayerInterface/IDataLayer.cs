﻿using DataAccessLayer.DataLayer.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.DataLayerInterface
{
    public interface IDataLayer
    {
        Task<string> IsValidCredential(User user);
        Task<bool> IsEmailExist(string email);
        Task<bool> RegisterUser(User cred);
        Task<bool> EditUserDetails(string token, User user, bool savePassword);
        Task<bool> DeleteUserAccount(string token, Guid id);
        Task<User> GetUserDetail(string token, Guid userId, bool isOnlyCredential);
    }
}
