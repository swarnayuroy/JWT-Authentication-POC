﻿using AutoMapper;
using DataAccessLayer.DataLayer.Entity;
using DataAccessLayer.DataLayerInterface;
using ServiceLayer.DTO;
using ServiceLayer.ServiceInterface;
using ServiceLayer.StartUp;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceLayer.ServiceDomain
{
    public class Service : IService
    {
        #region Declaration and Initialization
        private IDataLayer _dataLayer;
        private MapEntity _map;
        public Service()
        {
            _dataLayer = new Config().DataLayerConfig;
            _map = new MapEntity();
        }
        #endregion

        #region Services
        public bool ConfirmValidCredential(UserDTO userDTO, out string usrName)
        {
            try
            {
                User user = _map.GetUserEntity(userDTO);
                if (_dataLayer.IsValidCredential(user, out usrName))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }

        public bool RegisterNewUser(UserDTO userDTO)
        {
            try
            {
                User user = _map.GetUserEntity(userDTO);
                if (_dataLayer.RegisterUser(user))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false;
        }
        #endregion
    }
}
