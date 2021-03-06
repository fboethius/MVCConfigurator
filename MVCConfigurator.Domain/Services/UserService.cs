﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using MVCConfigurator.Domain.Repositories;
using MVCConfigurator.Domain.Handlers;
using MVCConfigurator.Domain.Models;

namespace MVCConfigurator.Domain.Services
{
    public class UserService : IUserService
    {
        private IPasswordHandler _passwordHandler;
        private IUserRepository _userRepository;
        public UserService(IUserRepository userRepository, IPasswordHandler passwordHandler)
        {
            _userRepository = userRepository;
            _passwordHandler = passwordHandler;
        }
        public Response<User> Login(string username, string password)
        {
            var response = new Response<User>();
            var user = _userRepository.GetByUsername(username);

            if(user ==null)
            {
                response.Error=ErrorCode.InvalidLogin;
                return response;
            }

            if(!_passwordHandler.Validate(password,user.Salt,user.Hash))
            {
                response.Error=ErrorCode.InvalidLogin;
                return response;
            }

            response.Entity = user;

            return response;

            //var IsCorrect = _passwordHandler.Validate(password, user.Salt, user.Hash);

            //if(IsCorrect)
            //    _adapter.DoAuth(username);

            //return IsCorrect;

        }

        public Response<User> RegisterUser(string username, string password, string confirmPassword, UserDetails userDetails)
        {
            
            var response = new Response<User>();

            if(_userRepository.GetByUsername(username)!= null)
            {
                response.Error = ErrorCode.DuplicateEntity;
            }
            if(password!=confirmPassword || password.Length<6)
            {
                response.Error = ErrorCode.InvalidState;
            }
            if(response.Success)
            {
                byte[] salt;
                byte[] hash;

                _passwordHandler.SaltAndHash(password, out salt, out hash);

                var user = _userRepository.CreateUser(username, salt, hash, userDetails);
                response.Entity = user;
            }

            return response;

        }
        public Response<User> Get(string userName)
        {
            return new Response<User> { Entity = _userRepository.GetByUsername(userName) };
        }


        public Response<User> UpdateUser(User user)
        {
           return new Response<User> { Entity = _userRepository.UpdateUser(user) };
        }


        public IList<User> GetAllUsers()
        {
            return _userRepository.GetAllUsers();
        }
    }
}
