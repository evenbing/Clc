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
    [AbpMvcAuthorize(PermissionNames.Pages_Monitor)]
    public class MonitorController : ClcControllerBase
    {
        // private readonly IWorkAppService _workAppService;

        public MonitorController()
        {
        }

       
        public ActionResult KeyPoints()
        {
            return RedirectToAction("KeyPoints", "Routes", new { Seld = 1});
        } 

        public ActionResult AffairQuery()
        {
            return RedirectToAction("RQuery", "Affairs", new { Seld = 1});
        } 


        public ActionResult RouteQuery()
        {
            return RedirectToAction("RQuery", "Routes", new { Seld = 1});
        } 

    }
}