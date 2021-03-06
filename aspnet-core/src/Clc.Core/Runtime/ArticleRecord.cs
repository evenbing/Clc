﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Abp.Domain.Entities;
using Clc.Fields;
using Clc.Routes;

namespace Clc.Runtime
{
    /// <summary>
    /// ArticleRecord Entity
    /// </summary>
    [Description("物品领用记录")]
    public class ArticleRecord : Entity, IMustHaveTenant
    {
        public const int MaxWorkersLength = 64;
        // Implement of IMustHaveTenant
        public int TenantId { get; set; }

        /// <summary>
        /// 外键：物品
        /// </summary>
        [Required]
        public int ArticleId { get; set; }
        public virtual Article Article { get; set; }

        /// <summary>
        /// 外键：领用人
        /// </summary>
        [Required]
        public int RouteWorkerId { get; set; }
        public virtual RouteWorker RouteWorker { get; set; }

        /// <summary>
        /// 领出时间
        /// </summary>
        [Required]
        public DateTime LendTime { get; set; }

        /// <summary>
        /// 归还时间
        /// </summary>
        public DateTime? ReturnTime { get; set; }

        [Required]
        [StringLength(MaxWorkersLength)]
        public string LendWorkers { get; set; }
        
        [StringLength(MaxWorkersLength)]
        public string ReturnWorkers { get; set; }
    }
}

