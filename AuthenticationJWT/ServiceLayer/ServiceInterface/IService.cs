﻿using ServiceLayer.DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceInterface
{
    public interface IService
    {
        Task<bool> IsEmailExist(string email);
        Task<string> ConfirmValidCredential(UserDTO user);
        Task<bool> RegisterNewUser(UserDTO user);
        Task<bool> EditUserDetails(string token, UserDetailsDTO userDTO, bool savePassword=false);
        Task<bool> DeleteAccount(string token, Guid id);
        Task<UserDetailsDTO> GetUserDetail(string token, Guid userId, bool isOnlyCredential=false);
    }
}
