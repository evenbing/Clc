using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Abp.Dependency;
using Abp.Runtime.Validation;
using Clc.Works;
using Clc.Affairs;

namespace Clc.Affairs.Dto
{
    /// <summary>
    /// AffairDto
    /// </summary>
    [AutoMap(typeof(Affair))]
    public class AffairDto : EntityDto, IShouldNormalize, ICustomValidate
    {
        public DateTime CarryoutDate { get; set; }

        /// <summary>
        /// Workplace
        /// </summary>
        // [Required]
        public int WorkplaceId { get; set; }

        /// <summary>
        /// 状态（生成，活动，结束, 日结）
        /// </summary>
        [StringLength(2)]
        public string Status { get; set; }

        /// <summary>
        /// 开始时间
        /// </summary>
        [Required]
        [StringLength(ClcConsts.TimeLength)]
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [Required]
        [StringLength(ClcConsts.TimeLength)]
        public string EndTime { get; set; }
        public string IsTomorrow { get; set; }

        [StringLength(Affair.MaxRemarkLength)]
        public string Remark { get; set; }

        public string WorkplaceName { get; set; }
        public string CreateWorkerName { get; set; }

        #region Inteface
        public void Normalize()
        {
            if (string.IsNullOrEmpty(Status)) 
                Status = "安排";
        }       

        public void AddValidationErrors(CustomValidationContext context)
        {
            WorkManager workManager = IocManager.Instance.Resolve<WorkManager>();

            var workplace = workManager.GetWorkplace(WorkplaceId);           
            string result = workplace.CheckTimeZone(StartTime, EndTime, IsTomorrow == "on" ? true : false);
            if (!string.IsNullOrEmpty(result))
                 context.Results.Add(new ValidationResult(result));
        }

        #endregion      

    }
}

