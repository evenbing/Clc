﻿using System;
using Microsoft.AspNetCore.Mvc;
using Abp.AspNetCore.Mvc.Authorization;
using Abp.Web.Models;
using Clc.Authorization;
using Clc.Controllers;
using Clc.Affairs;
using System.Threading.Tasks;

namespace Clc.Web.Controllers
{
    [AbpMvcAuthorize(PermissionNames.Pages_Arrange)]
    public class AffairsController : ClcControllerBase
    {
        private readonly IAffairAppService _affairAppService;

        public AffairsController(IAffairAppService affairAppService)
        {
            _affairAppService = affairAppService;
       }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult WhAffairWorkersStat()
        {
            return View();
        }
        
        [DontWrapResult]
        public async Task<JsonResult> GridData(DateTime carryoutDate)
        {
            var output = await _affairAppService.GetAffairsAsync(carryoutDate, GetSorting());
            return Json( new { rows = output });
        }

        [DontWrapResult]
        public JsonResult WorkersGridData(int id)
        {
            var output = _affairAppService.GetAffairWorkers(id, GetSorting());
            return Json( new { rows = output });
        }
    }
}