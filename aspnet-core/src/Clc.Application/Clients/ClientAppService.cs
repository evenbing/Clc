using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Clc.Runtime.Cache;

namespace Clc.Clients
{
    [AbpAuthorize]
    public class ClientAppService : ClcAppServiceBase, IClientAppService
    {
        private readonly ICustomerCache _customerCache;

        private readonly IOutletCache _outletCache;

        public ClientAppService(ICustomerCache customerCache, IOutletCache outletCache)
        {
            _customerCache = customerCache;
            _outletCache = outletCache;
        }

        public Task<List<ComboboxItemDto>> GetComboItems(string name)
        {
            var lst = new List<ComboboxItemDto>();
            switch (name) 
            {
               case "CustomerWithCn":
                    foreach (Customer t in _customerCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = string.Format("{0} {1}", t.Cn, t.Name) });
                    break;
               case "OutletWithCn":
                    foreach (Outlet t in _outletCache.GetList())
                        lst.Add(new ComboboxItemDto { Value = t.Id.ToString(), DisplayText = string.Format("{0} {1}", t.Cn, t.Name) });
                    break;
                default:
                    break;
            }
            return Task.FromResult<List<ComboboxItemDto>>(lst);
        }        
    }
}