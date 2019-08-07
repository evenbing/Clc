using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Clc.Runtime.Cache;
using Clc.Types.Entities;

namespace Clc.Types
{
    [AbpAuthorize]
    public class TypeAppService : ClcAppServiceBase, ITypeAppService
    {
        private readonly DutyProvider _dutyProvider;
        private readonly IAffairTypeCache _affairTypeCache;
        private readonly IArticleTypeCache _articleTypeCache;
        private readonly IPostCache _postCache;
        private readonly IRouteTypeCache _routeTypeCache;
        private readonly ITaskTypeCache _taskTypeCache;
        private readonly IWorkRoleCache _workRoleCache;
        private readonly List<string> _bindStyleItems = new List<string>() {"人", "车", "线路" };

        public TypeAppService(DutyProvider dutyProvider, 
            IAffairTypeCache affairTypeCache,
            IArticleTypeCache articleTypeCache,
            IPostCache postCache,
            IRouteTypeCache routeTypeCache,
            ITaskTypeCache taskTypeCache,
            IWorkRoleCache workRoleCache)
        {
            _dutyProvider = dutyProvider;
            _affairTypeCache = affairTypeCache;
            _articleTypeCache = articleTypeCache;
            _postCache = postCache;
            _routeTypeCache = routeTypeCache;
            _taskTypeCache = taskTypeCache;
            _workRoleCache = workRoleCache;
        }

       
        public Task<List<ComboboxItemDto>> GetDutyCategories()
        {
            List<ComboboxItemDto> lst = new List<ComboboxItemDto>();
            foreach (var c in _dutyProvider.GetDutyCategory()) 
            {
                lst.Add(new ComboboxItemDto() { Value = c, DisplayText = c});
            }
            return Task.FromResult<List<ComboboxItemDto>>(lst);            
        }
        
        public Task<List<ComboboxItemDto>> GetComboItems(string typeName)
        {
            var lst = new List<ComboboxItemDto>();
            switch (typeName) 
            {
                case "AffairType":
                    foreach (AffairType t in _affairTypeCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = t.Name });
                    break;
                case "ArticleType":
                    foreach (ArticleType t in _articleTypeCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = string.Format("{0} {1}", t.Cn, t.Name) });
                    break;
                case "Post":
                    foreach (Post t in _postCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = t.Name });
                    break;
                case "RouteType":
                    foreach (RouteType t in _routeTypeCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = t.Name });
                    break;
                case "TaskType":
                    foreach (TaskType t in _taskTypeCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = t.Name });
                    break;
                case "WorkRole":
                    foreach (WorkRole t in _workRoleCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = t.Name });
                    break;
                case "BindStyle":
                    foreach (string t in _bindStyleItems)
                        lst.Add(new ComboboxItemDto { Value = t, DisplayText = t });
                    break;
                default:
                    break;
            }
             return Task.FromResult<List<ComboboxItemDto>>(lst);
        }        
    }
}