﻿using AuthenticationJWT.Models;
using ServiceLayer.DTO;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuthenticationJWT.Utils
{
    public interface IMap
    {
        User GetUserCredential(LoginDetails credential);
        UserDetails GetUserDetails(UserDetailsDTO userDetail);
        User GetOnlyUserCred(UserDetailsDTO userDetail);
        UserDetailsDTO GetUserDetailsDTO(UserDetails userDetail);
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

        public UserDetails GetUserDetails(UserDetailsDTO userDetail)
        {
            Mapper.CreateMap<UserDetailsDTO, UserDetails>();
            UserDetails user = Mapper.Map<UserDetailsDTO, UserDetails>(userDetail);
            return user;
        }

        public User GetOnlyUserCred(UserDetailsDTO userDetail)
        {
            Mapper.CreateMap<UserDetailsDTO, User>();
            User user = Mapper.Map<UserDetailsDTO, User>(userDetail);
            return user;
        }

        public UserDetailsDTO GetUserDetailsDTO(UserDetails userDetail)
        {
            Mapper.CreateMap<UserDetails, UserDetailsDTO>();
            UserDetailsDTO user = Mapper.Map<UserDetails, UserDetailsDTO>(userDetail);
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