using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Clc.Fields;

namespace Clc.ArticleRecords.Dto
{
    [AutoMapFrom(typeof(Article))]
    public class ArticleRecordDto : EntityDto
    {
        public string Cn { get; set; }

        /// <summary>
        /// 物品名
        /// </summary>
        public string Name { get; set; }
        public int ArticleTypeId { get; set; }
        public string ArticleTypeName { get; set; }

        /// <summary>
        /// Rfid 
        /// </summary>
        public string Rfid { get; set; }

        /// <summary>
        /// 绑定信息 
        /// </summary>
        public string BindInfo { get; set; }

        public int? ArticleRecordId { get; set; }
        public string ArticleRecordInfo { get; set; }
        
    }
}