﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
using Abp.Domain.Repositories;
using Clc.Routes;
using Clc.Works;
using Clc.Weixin.Dto;
using Clc.Fields;
using Clc.Runtime.Cache;

namespace Clc.Weixin
{
    public class WeixinAppService : ClcAppServiceBase, IWeixinAppService
    {
        public WorkManager WorkManager { get; set; }
        private readonly IRouteAppService _routeAppService;
        private readonly IRepository<Worker> _workerRepository;

        private readonly IOutletCache _outletCache;
        private readonly ITaskTypeCache _taskTypeCache;
        private readonly IVehicleCache _vehicleCache;

        public WeixinAppService(IRouteAppService routeAppService, IRepository<Worker> workerRepository,
            IOutletCache outletCache, ITaskTypeCache taskTypeCache, IVehicleCache vehicleCache)
        {
            _routeAppService = routeAppService;
            _workerRepository = workerRepository;
            _outletCache = outletCache;
            _taskTypeCache = taskTypeCache;
            _vehicleCache = vehicleCache;
        }

        public WxIdentifyDto Login(string workerCn, string password, string deviceId)
        {
            WxIdentifyDto dto = new WxIdentifyDto();

            var worker = WorkManager.GetWorkerByCn(workerCn);
            if (worker == null || password != worker.Password) {
                dto.ErrorMessage = "用户名或密码错误";
                return dto;
            }

            // judge right routeRole and subworker
            var ret = FindRoute(worker, dto);
            if (ret.Item1 == null) {
                dto.ErrorMessage = "找不到合适的线路";
                return dto;
            }
            if (ret.Item2 == null) 
                dto.ErrorMessage = "需要辅助交接员";
            
            return dto;
        }

        public void SetIdentifyTime(int taskId)
        {
            _routeAppService.SetIdentifyTime(taskId);
        }

        private (Route, Worker) FindRoute(Worker main, WxIdentifyDto dto)
        {
            var ret = _routeAppService.FindRouteForIdentify(main.DepotId, main.Id);
            if (ret.Item1 == null) return (null, null);
            if (ret.Item2 == 0) return (ret.Item1, null);
            
            var sub = WorkManager.GetWorker(ret.Item2);
            dto.Workers.Add(new WeixinWorkerDto() 
                { 
                    Cn = main.Cn, Name = main.Name, AdditiveInfo = main.AdditiveInfo, Photo = main.Photo == null ? null : Convert.ToBase64String(main.Photo)   
                });
            dto.Workers.Add(new WeixinWorkerDto()
                { 
                    Cn = sub.Cn, Name = sub.Name, AdditiveInfo = sub.AdditiveInfo, Photo = sub.Photo == null ? null : Convert.ToBase64String(sub.Photo)   
                });


            if (ret.Item1.VehicleId.HasValue) 
            {
                var v = _vehicleCache[ret.Item1.VehicleId.Value];
                dto.Vehicle = new WeixinVehicleDto() {Cn = v.Cn, License = v.License, Photo = v.Photo == null ? null : Convert.ToBase64String(v.Photo) };
            }

            dto.RouteId = ret.Item1.Id;
            dto.RouteName = ret.Item1.RouteName;

            foreach (RouteTask task in ret.Item1.Tasks) 
            {
                var outlet = _outletCache[task.OutletId];
                var type = _taskTypeCache[task.TaskTypeId];
                string tm = task.IdentifyTime.HasValue ? task.IdentifyTime.Value.ToString("HH:mm") : "";
                dto.Tasks.Add(new WeixinTaskDto(
                    task.Id, task.ArriveTime, type.Name, outlet.Id, outlet.Cn, outlet.Name, tm, task.Remark));
            }
            return (ret.Item1, sub);
        }
    }
}