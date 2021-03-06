﻿using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Web.Models;
using Clc.Authorization;
using Clc.Controllers;
using Clc.Works;
using System.Threading.Tasks;
using Clc.DoorRecords;
using System;
using Clc.Web.MessageHandlers;

namespace Clc.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Monitor, PermissionNames.Pages_Arrange)]
    public class OpenDoorsController : ClcControllerBase
    {
        private readonly IWorkAppService _workAppService;
        private readonly IDoorRecordAppService _doorRecordAppService;

        public OpenDoorsController(IDoorRecordAppService doorRecordAppService, IWorkAppService workAppService)
        {
            _doorRecordAppService = doorRecordAppService;
            _workAppService = workAppService;
        }

        public ActionResult EmergOpenDoor()
        {
            return View();
        }
        public ActionResult AskOpenDoor()
        {
            return View();
        }

        public ActionResult RecordQuery(int depotId = 0)
        {
            return View(depotId);
        }
        
        [HttpPost]
        [DontWrapResult]
        public JsonResult NotifyAskWorkers(int doorRecordId)
        {
            if (doorRecordId == 0) return Json( new { Message = "直接开门方式" });
            
            var ret = _doorRecordAppService.GetNotifyInfo(doorRecordId);

            WeixinUtils.SendMessage("App01", ret.Item1, $"你申请的{ret.Item2}已打开");
            return Json( new { Message = "已通知" });
        }

        [DontWrapResult]
        public async Task<JsonResult> GridDataDoor(int depotId = 0)
        {
            var output = await _doorRecordAppService.GetDoorsAsync(depotId);
            return Json( new { rows = output });
        }

        [DontWrapResult]
        public async Task<JsonResult> GridDataAskDoor(DateTime dt)
        {
            var output = await _doorRecordAppService.GetAskDoorsAsync(dt);
            return Json( new { rows = output });
        }

        [DontWrapResult]
        public async Task<JsonResult> GridDataEmergDoor(DateTime dt)
        {
            var output = await _doorRecordAppService.GetEmergDoorsAsync(dt);
            return Json( new { rows = output });
        }

        [DontWrapResult]
        public async Task<JsonResult> GridDataAskDoorRecord(int workplaceId)
        {
            var output = await _doorRecordAppService.GetAskDoorRecordsAsync(workplaceId, GetPagedInput());
            return Json( new { total = output.TotalCount, rows = output.Items });
        }

        [DontWrapResult]
        public async Task<JsonResult> GridDataEmergDoorRecord(int workplaceId)
        {
            var output = await _doorRecordAppService.GetEmergDoorRecordsAsync(workplaceId, GetPagedInput());
            return Json( new { total = output.TotalCount, rows = output.Items });
        }

    }
}