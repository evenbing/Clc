﻿using System.Collections.Generic;
using Abp.Configuration;
using Abp.Localization;
using Clc.Authorization.Users;

namespace Clc.Configuration
{
    public class AppSettingProvider : SettingProvider
    {
        public override IEnumerable<SettingDefinition> GetSettingDefinitions(SettingDefinitionProviderContext context)
        {
            List<SettingDefinition> lst = new List<SettingDefinition>();
            lst.AddRange(GetVISettingDefinitions(context));
            lst.AddRange(GetConstSettingDefinitions(context));
            lst.AddRange(GetRuleSettingDefinitions(context));
            lst.AddRange(GetTimeRuleSettingDefinitions(context));
            // return new[]
            // {
            //     new SettingDefinition(AppSettingNames.UiTheme, "red", scopes: SettingScopes.Application | SettingScopes.Tenant | SettingScopes.User, isVisibleToClients: true)
            // };
            return lst;
        }

        private IEnumerable<SettingDefinition> GetVISettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new List<SettingDefinition>
            {
                new SettingDefinition(
                    AppSettingNames.VI.CompanyName, 
                    "XX押运", 
                    new FixedLocalizableString("公司名"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.VI.CompanyImageName, 
                    "company.jpg", 
                    new FixedLocalizableString("公司图标名"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.VI.DepotTitleName, 
                    "中心", 
                    new FixedLocalizableString("物流中心外显名称"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                )
            };
        }
        private IEnumerable<SettingDefinition> GetConstSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new List<SettingDefinition>
            {
                new SettingDefinition(
                    AppSettingNames.Const.FingerThreshold, 
                    "30", 
                    new FixedLocalizableString("指纹通过阀值"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Const.IdentifyEmergencyPassword, 
                    User.UserDefaultPassword, 
                    new FixedLocalizableString("身份确认紧急密码"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    AppSettingNames.Const.CameraPassword, 
                    User.UserDefaultPassword, 
                    new FixedLocalizableString("摄像头密码"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    AppSettingNames.Const.WorkerRfidLength, 
                    "5", 
                    new FixedLocalizableString("员工RFID卡编码长度"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    AppSettingNames.Const.ArticleRfidLength, 
                    "3", 
                    new FixedLocalizableString("物品RFID卡编码长度"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    AppSettingNames.Const.BoxRfidLength, 
                    "8", 
                    new FixedLocalizableString("尾箱RFID卡编码长度"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
            };
        }
        private IEnumerable<SettingDefinition> GetRuleSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new List<SettingDefinition>
            {
                new SettingDefinition(
                    AppSettingNames.Rule.LoginIpList, 
                    "", 
                    new FixedLocalizableString("登录IP绑定"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.VerifyLogin, 
                    "false", 
                    new FixedLocalizableString("校验码登录"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.Radius, 
                    "900", 
                    new FixedLocalizableString("半径(米)"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.DoubleArticleRoles, 
                    "车长|持枪员", 
                    new FixedLocalizableString("双人领物角色"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.MinWorkersOnDuty, 
                    "2", 
                    new FixedLocalizableString("最少当班人数"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.EnableDynEmergPassword, 
                    "false", 
                    new FixedLocalizableString("动态应急开门密码"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.AltCheckinDepots, 
                    "调度",
                    new FixedLocalizableString("代验入大队列表"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.EditWorkerDepots, 
                    "", 
                    new FixedLocalizableString("允许编辑人员的大队列表"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.Rule.BulletCabinets, 
                    "01 192.168.2.5", 
                    new FixedLocalizableString("独立枪柜IP"),
                    scopes: SettingScopes.Tenant,
                    isVisibleToClients: true
                ),
            };
        }
        private IEnumerable<SettingDefinition> GetTimeRuleSettingDefinitions(SettingDefinitionProviderContext context)
        {
            return new List<SettingDefinition>
            {
                new SettingDefinition(
                    AppSettingNames.TimeRule.DaysChangeReadonly, 
                    "2", 
                    new FixedLocalizableString("变动保留天数"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                   AppSettingNames.TimeRule.RecheckInterval, 
                   "0",            // 0: 不需要重新验证
                   new FixedLocalizableString("重新验证间隔(分钟)"),
                   scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.TimeRule.ReturnDeadline, 
                    "120", 
                    new FixedLocalizableString("还物截止时间(分)"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.TimeRule.AskOpenInterval, 
                    "60", 
                    new FixedLocalizableString("申请开门间隔(秒)"),
                    scopes: SettingScopes.Tenant
                ),
                new SettingDefinition(
                    AppSettingNames.TimeRule.ActivateTime, 
                    "05:00", 
                    new FixedLocalizableString("激活时点"),
                    scopes: SettingScopes.Tenant
                ),
            };
        }
    }
}
