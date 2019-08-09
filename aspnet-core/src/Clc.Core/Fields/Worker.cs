using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Clc.Types;

namespace Clc.Fields
{
    /// <summary>
    /// Worker Entity
    /// </summary>
    [Description("工作人员")]
    public class Worker : Entity, IMustHaveTenant, IPassivable
    {
        // Impement of IMustHaveTenant
        public int TenantId { get; set; }

        public const int MaxCnLength = 8;
        public const int MaxNameLength = 8;
        public const int MaxPasswordLength = 10;
        public const string PasswordRegex = "^[0-9]{6}$";
        public const int RfidMaxLength = 18;
        public const int MaxDeviceId = 50;
        public const int MaxAdditiveInfoLength = 20;
 
        /// <summary>
        /// 所属中心
        /// </summary>
        public int DepotId { get; set; }
        public Depot Depot { get; set; }
        
        /// <summary>
        /// 编号
        /// </summary>
        [Required]
        [StringLength(MaxCnLength)]
        public string Cn { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

         /// <summary>
        /// 岗位
        /// </summary>
        [Required]
        public int PostId { get; set; }
        public Post Post { get; set; }

        /// <summary>
        /// 登录密码
        /// </summary>
        [StringLength(MaxPasswordLength)]
        public string Password { get; set; }

        /// <summary>
        /// IDCardNo 
        /// </summary>
        [StringLength(RfidMaxLength)]
        public string Rfid { get; set; }

        /// <summary>
        /// 照片
        /// </summary>
        public byte[] Photo { get; set; }

        /// <summary>
        /// 微信设备Id
        /// </summary>
        [StringLength(MaxDeviceId)]
        public string DeviceId { get; set; }

        // 附加认证信息
        [StringLength(MaxAdditiveInfoLength)]
        public string AdditiveInfo { get; set; }

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// 暂时借用到
        /// </summary>
        public int? LoanDepotId { get; set; }
        public Depot LoanDepot { get; set; }
    }
}
