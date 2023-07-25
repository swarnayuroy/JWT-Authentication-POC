﻿using AutoMapper;
using ServiceLayer.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticationJWT.Models.Mapping
{
    public interface IMap
    {
        User GetUserCredential(LoginDetails credential);
        UserDTO GetUserDTO(User usrCred);
    }
    public class MapEntity: IMap
    {
        public User GetUserCredential(LoginDetails credential)
        {
            Mapper.CreateMap<LoginDetails, User>();
            User user = Mapper.Map<LoginDetails, User>(credential);
            return user;
        }
        public UserDTO GetUserDTO(User usrCred)
        {
            Mapper.CreateMap<User, UserDTO>();
            UserDTO usrDTO = Mapper.Map<User, UserDTO>(usrCred);
            return usrDTO;
        }
    }
}