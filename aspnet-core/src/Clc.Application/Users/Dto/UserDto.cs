using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Abp.Runtime.Validation;
using Clc.Authorization.Users;

namespace Clc.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>, IShouldNormalize
    {
        // [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        //[Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        // [Required]
        // [StringLength(AbpUserBase.MaxSurnameLength)]
        // public string Surname { get; set; }

        // [Required]
        // [EmailAddress]
        // [StringLength(AbpUserBase.MaxEmailAddressLength)]
        // public string EmailAddress { get; set; }

        // public bool IsActive { get; set; }

        // public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string[] RoleNames { get; set; }

        public void Normalize()
        {
            if (RoleNames == null)
            {
                RoleNames = new string[0];
            }
        }
       
    }
}
