﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.IdentityFramework;
using Abp.Linq.Extensions;
using Abp.MultiTenancy;
using Clc.Authorization;
using Clc.Authorization.Roles;
using Clc.Authorization.Users;
using Clc.MultiTenancy;
using Clc.Roles.Dto;

namespace Clc.Roles
{
    [AbpAuthorize(PermissionNames.Pages_Host)]
    public class RoleAppService : AsyncCrudAppService<Role, RoleDto, int, PagedRoleResultRequestDto, CreateRoleDto, RoleDto>, IRoleAppService
    {
        private readonly RoleManager _roleManager;
        private readonly UserManager _userManager;
        private readonly TenantManager _tenantManager;

        public RoleAppService(IRepository<Role> repository, RoleManager roleManager, UserManager userManager, TenantManager tenantManager)
            : base(repository)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _tenantManager = tenantManager;
        }

        public async Task<ListResultDto<RoleListDto>> GetTenantRoles(string tenancyName)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            if (tenant == null) 
                return null;

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                return new ListResultDto<RoleListDto>(
                    ObjectMapper.Map<List<RoleListDto>>(
                        _roleManager.Roles.OrderBy(t => t.Name).ToList()
                    )
                );
            }
        }
        public async Task<List<string>> GetRolePermissionNames(string tenancyName, int roleId)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            List<string> lst = new List<string>();
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                var permissions = await _roleManager.GetGrantedPermissionsAsync(roleId);
                foreach (Permission p in permissions)
                    lst.Add(p.Name);
            }
            return lst;
        }

        public override async Task<RoleDto> Create(CreateRoleDto input)
        {
            CheckCreatePermission();

            var role = ObjectMapper.Map<Role>(input);
            role.SetNormalizedName();

            CheckErrors(await _roleManager.CreateAsync(role));

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.Permissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return MapToEntityDto(role);
        }

        public async Task<ListResultDto<RoleListDto>> GetRolesAsync(GetRolesInput input)
        {
            var roles = await _roleManager
                .Roles
                .WhereIf(
                    !input.Permission.IsNullOrWhiteSpace(),
                    r => r.Permissions.Any(rp => rp.Name == input.Permission && rp.IsGranted)
                )
                .ToListAsync();

            return new ListResultDto<RoleListDto>(ObjectMapper.Map<List<RoleListDto>>(roles));
        }

        public override async Task<RoleDto> Update(RoleDto input)
        {
            CheckUpdatePermission();

            var role = await _roleManager.GetRoleByIdAsync(input.Id);

            ObjectMapper.Map(input, role);

            CheckErrors(await _roleManager.UpdateAsync(role));

            var grantedPermissions = PermissionManager
                .GetAllPermissions()
                .Where(p => input.Permissions.Contains(p.Name))
                .ToList();

            await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);

            return MapToEntityDto(role);
        }

        public override async Task Delete(EntityDto<int> input)
        {
            CheckDeletePermission();

            var role = await _roleManager.FindByIdAsync(input.Id.ToString());
            var users = await _userManager.GetUsersInRoleAsync(role.NormalizedName);

            foreach (var user in users)
            {
                CheckErrors(await _userManager.RemoveFromRoleAsync(user, role.NormalizedName));
            }

            CheckErrors(await _roleManager.DeleteAsync(role));
        }

        public Task<ListResultDto<PermissionDto>> GetAllPermissions()
        {
            var permissions = PermissionManager.GetAllPermissions();

            return Task.FromResult(new ListResultDto<PermissionDto>(
                ObjectMapper.Map<List<PermissionDto>>(permissions.Where(x => x.MultiTenancySides != MultiTenancySides.Host))
            ));
        }

        protected override IQueryable<Role> CreateFilteredQuery(PagedRoleResultRequestDto input)
        {
            return Repository.GetAllIncluding(x => x.Permissions)
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Name.Contains(input.Keyword)
                || x.DisplayName.Contains(input.Keyword)
                || x.Description.Contains(input.Keyword));
        }

        protected override async Task<Role> GetEntityByIdAsync(int id)
        {
            return await Repository.GetAllIncluding(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
        }

        protected override IQueryable<Role> ApplySorting(IQueryable<Role> query, PagedRoleResultRequestDto input)
        {
            return query.OrderBy(r => r.DisplayName);
        }

        protected virtual void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }

        public async Task<GetRoleForEditOutput> GetRoleForEdit(EntityDto input)
        {
            var permissions = PermissionManager.GetAllPermissions();
            var role = await _roleManager.GetRoleByIdAsync(input.Id);
            var grantedPermissions = (await _roleManager.GetGrantedPermissionsAsync(role)).ToArray();
            var roleEditDto = ObjectMapper.Map<RoleEditDto>(role);

            return new GetRoleForEditOutput
            {
                Role = roleEditDto,
                Permissions = ObjectMapper.Map<List<FlatPermissionDto>>(permissions).OrderBy(p => p.DisplayName).ToList(),
                GrantedPermissionNames = grantedPermissions.Select(p => p.Name).ToList()
            };
        }

        #region Host Tenancy Manager
        public async Task CreateTenantRole(string tenancyName, CreateRoleDto input)
        {
            CheckCreatePermission();

            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                //Create role
                var role = ObjectMapper.Map<Role>(input);
                // role.IsStatic = true;
                await _roleManager.CreateAsync(role);
            }
        }

        public async Task UpdateRolePermissions(string tenancyName, UpdateRolePermissionsInput input)
        {
            var tenant = await _tenantManager.FindByTenancyNameAsync(tenancyName);
            if (tenant == null) 
                return;

            using (CurrentUnitOfWork.SetTenantId(tenant.Id))
            {
                var role = await _roleManager.GetRoleByIdAsync(input.RoleId);
                await _roleManager.ResetAllPermissionsAsync(role);
                var grantedPermissions = PermissionManager
                    .GetAllPermissions()
                    .Where(p => input.GrantedPermissionNames.Contains(p.Name))
                    .ToList();

                await _roleManager.SetGrantedPermissionsAsync(role, grantedPermissions);
            }
        }

        #endregion
    }
}

