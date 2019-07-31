using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;

namespace Clc.Fields.Entities
{
    /// <summary>
    /// Depot Entity
    /// </summary>
    [Description("运行中心")]
    public class Depot : Entity, IMustHaveTenant
    {        
        public const int MaxCnLength = 2;
        public const int MaxNameLength = 8;
        // Impement of IMustHaveTenant
        public int TenantId { get; set; }

        /// <summary>
        /// 编号
        /// </summary>
        [Required]
        [StringLength(MaxCnLength)]
        public string Cn { get; set; }

        /// <summary>
        /// 运行中心名称
        /// </summary>
        [Required]
        [StringLength(MaxNameLength)]
        public string Name { get; set; }

        /// <summary>
        /// 经度
        /// </summary>
        public double? Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public double? Latitude { get; set; }

        /// <summary>
        /// 签到半径(米)，如空采用缺省的
        /// </summary>
        public int? Radius { get; set; }
    
        /// <summary>
        /// 激活线路是否需要全部签到
        /// </summary>
        public bool activeRouteNeedCheckin { get; set; }

        /// <summary>
        /// 利用其它运行中心的库
        /// </summary>
        public int? RelyDepotId { get; set; }
        public Depot RelyDepot { get; set; }

    }
}
