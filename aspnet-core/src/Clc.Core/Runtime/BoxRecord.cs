﻿using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Abp.Domain.Entities;
using Clc.Affairs;
using Clc.Clients;
using Clc.Fields;
using Clc.Routes;

namespace Clc.Runtime
{
    /// <summary>
    /// BoxRecord Entity
    /// </summary>
    [Description("尾箱进出记录")]
    public class BoxRecord : Entity, IMustHaveTenant
    {
        public const int MaxWorkersLength = 64;
        // Implement of IMustHaveTenant
        public int TenantId { get; set; }

        /// <summary>
        /// 外键：尾箱
        /// </summary>
        [Required]
        public int BoxId { get; set; }
        public virtual Box Box { get; set; }

        /// <summary>
        /// 进时间
        /// </summary>
        public DateTime? InTime { get; set; }

        /// <summary>
        /// 出时间
        /// </summary>
        public DateTime? OutTime { get; set; }

        [StringLength(MaxWorkersLength)]
        public string InWorkers { get; set; }
        
        [StringLength(MaxWorkersLength)]
        public string OutWorkers { get; set; }    

        public DateTime? PickupTime { get; set; }
        public DateTime? DeliverTime { get; set; }
        public int? InRouteTaskId { get; set; }
        public virtual RouteTask InRouteTask { get; set; }
        public int? OutRouteTaskId { get; set; }
        public virtual RouteTask OutRouteTask { get; set; }
    }
}

