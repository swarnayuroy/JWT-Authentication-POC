﻿using AutoMapper;
using DataAccessLayer.DataLayer.Entity;
using DataAccessLayer.DataLayerInterface;
using ServiceLayer.DTO;
using ServiceLayer.ServiceInterface;
using ServiceLayer.StartUp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.ServiceDomain
{
    public class Service : IService
    {
        #region Declaration and Initialization
        private readonly IDataLayer _dataLayer;
        private readonly IMap _mapper;
        public Service()
        {
            _dataLayer = new Config().DataLayerService;
            _mapper = new Config().MapperService;
        }
        #endregion

        #region Services
        public async Task<bool> ConfirmValidCredential(UserDTO userDTO)
        {
            bool status = false;
            try
            {
                User user = _mapper.GetUserEntity(userDTO);
                status = await Task.Run(() => _dataLayer.IsValidCredential(user));
                if (status)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return status;
        }

        public async Task<bool> RegisterNewUser(UserDTO userDTO)
        {
            bool status = false;
            try
            {
                User user = _mapper.GetUserEntity(userDTO);
                status = await Task.Run(()=> _dataLayer.RegisterUser(user));
                if (status)
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return status;
        }
        #endregion
    }
}
