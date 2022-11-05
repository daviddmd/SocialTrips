using System;
using System.ComponentModel.DataAnnotations;

namespace BackendAPI.Models.User
{
    public class UserSearchModel
    {
        public string NameOrEmail { get; set; }
    }
}