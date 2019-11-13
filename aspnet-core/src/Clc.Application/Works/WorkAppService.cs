using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq;
using Clc.Affairs;
using Clc.Configuration;
using Clc.Fields;
using Clc.Routes;
using Clc.Runtime;
using Clc.Runtime.Cache;
using Clc.Types;
using Clc.Works.Dto;

namespace Clc.Works
{

    [AbpAuthorize]
    public class WorkAppService : ClcAppServiceBase, IWorkAppService
    {
        public WorkManager WorkManager { get; set; }

        public IAsyncQueryableExecuter AsyncQueryableExecuter { get; set; }

        private readonly IRepository<Signin> _signinRepository;
        private readonly IRepository<Route> _routeRepository;
        private readonly IRepository<RouteArticle> _routeArticleRepository;
        private readonly IRepository<RouteTask> _routeTaskRepository;
        private readonly IRepository<Depot> _depotRepository;
        private readonly IRouteCache _routeCache;
        private readonly IWorkRoleCache _workRoleCache;
        private readonly IRouteTypeCache _routeTypeCache;

        private readonly IArticleCache _articleCache;
        private readonly IArticleTypeCache _articleTypeCache;
        private readonly IBoxCache _boxCache;
        private readonly IOutletCache _outletCache;

        public WorkAppService(IRepository<Signin> signinRepository,
            IRepository<Route> routeRepository,
            IRepository<RouteArticle> routeArticleRepository,
            IRepository<Depot> depotRepository,
            IRouteCache routeCache,
            IWorkRoleCache workRoleCache,
            IRouteTypeCache routeTypeCache,
            IArticleCache articleCache,
            IArticleTypeCache articleTypeCache,
            IBoxCache boxCache,
            IOutletCache outletCache)
        {
            _signinRepository = signinRepository;
            _routeRepository = routeRepository;
            _routeArticleRepository = routeArticleRepository;
            _depotRepository = depotRepository;

            _routeCache = routeCache;
            _workRoleCache = workRoleCache;
            _routeTypeCache = routeTypeCache;
            _articleCache = articleCache;
            _articleTypeCache = articleTypeCache;
            _boxCache = boxCache;
            _outletCache = outletCache;
        }

        public bool VerifyUnlockPassword(string password)
        {
            var depotId = WorkManager.GetWorkerDepotId(GetCurrentUserWorkerIdAsync().Result);
            var unlockPassword = WorkManager.GetUnlockPassword(depotId); // Get DepotId and WorkCn
            return unlockPassword == password;
        }

        public bool AllowCardWhenCheckin()
        {
            int depotId = WorkManager.GetWorkerDepotId(GetCurrentUserWorkerIdAsync().Result);
            return WorkManager.GetDepot(depotId).AllowCardWhenCheckin;
        }

        public (string, string) GetMe()
        {
            int workerId = GetCurrentUserWorkerIdAsync().Result; 
            if (workerId == 0) 
                return ("system", "");
            
            if (WorkManager.IsCaptain(workerId))
                workerId = WorkManager.GetCaptainOrAgentId(workerId);

            var worker = WorkManager.GetWorker(workerId);
            return (worker.LoginRoleNames, worker.Cn);
        }
        
        public MyAffairWorkDto GetMyAffairWork()
        {
            var dto = new MyAffairWorkDto();
            dto.Now = DateTime.Now;
            dto.Today = DateTime.Now.Date.ToString("yyyy-MM-dd");
            int workerId = GetCurrentUserWorkerIdAsync().Result; 
            dto.DepotId = WorkManager.GetWorkerDepotId(workerId);         

            var aw = WorkManager.GetCacheAffairWorker(DateTime.Now.Date, dto.DepotId, workerId);
            if (aw.Item2 != null)
            {
                dto.AffairId = aw.Item1.Id;
                dto.StartTime = ClcUtils.GetDateTime(aw.Item1.StartTime);
                dto.EndTime = ClcUtils.GetDateTime(aw.Item1.EndTime);
            }
            return dto;
        }

        public List<RouteCacheItem> GetActiveRoutes(DateTime carryoutDate, int depotId, int affairId)
        {
            var lst = _routeCache.Get(carryoutDate, depotId);
            lst.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return lst;
        }

        public string GetReportToManagers()
        {
            int depotId = WorkManager.GetWorkerDepotId(GetCurrentUserWorkerIdAsync().Result);
            return WorkManager.GetReportToManagers(depotId);
        }

        #region Agent
        public string GetAgentString()
        {
            int workerId = GetCurrentUserWorkerIdAsync().Result; 
            int depotId = WorkManager.GetWorkerDepotId(workerId);
            string agentCn = WorkManager.GetDepot(depotId).AgentCn;
            var worker = WorkManager.GetWorkerByCn(agentCn);
            if (worker == null) return null;
            return string.Format("{0} {1}", worker.Cn, worker.Name);
        }
        public async Task SetAgent(int workerId)
        {
            int depotId = WorkManager.GetWorkerDepotId(await GetCurrentUserWorkerIdAsync());
            var depot = _depotRepository.Get(depotId);
            depot.AgentCn = WorkManager.GetWorker(workerId).Cn;
        }

        public async Task ResetAgent()
        {
            int depotId = WorkManager.GetWorkerDepotId(await GetCurrentUserWorkerIdAsync());
            var depot = _depotRepository.Get(depotId);
            depot.AgentCn = null;
        }
        #endregion

        #region signin

        public async Task<List<SigninDto>> GetSigninsAsync(DateTime carryoutDate)
        {
            int depotId = WorkManager.GetWorkerDepotId(await GetCurrentUserWorkerIdAsync());
            var query = _signinRepository.GetAllIncluding(x => x.Worker);
            query = query.Where(x => x.CarryoutDate == carryoutDate && x.DepotId == depotId);
            var entities = await AsyncQueryableExecuter.ToListAsync(query);
            entities.Sort((a, b) => a.Worker.Cn.CompareTo(b.Worker.Cn));
            // var list = entities.Distinct(new LambdaEqualityComparer<Signin>((a, b) => a.Worker.Cn == b.Worker.Cn)).ToList();
            return ObjectMapper.Map<List<SigninDto>>(entities);
        }

        public (bool, string) SigninByRfid(string rfid) 
        {
            int depotId = WorkManager.GetWorkerDepotId(GetCurrentUserWorkerIdAsync().Result);
            Worker worker = WorkManager.GetWorkerByRfid(rfid);

            if (worker == null) return (false, "找不到此人");
            if (worker.DepotId != depotId) return (false, "此人不应在此运作中心签到");
            
            return WorkManager.DoSignin(depotId, worker.Id, "刷卡");
        }

        public (bool, string) SigninByFinger(string finger) 
        {
            int depotId = WorkManager.GetWorkerDepotId(GetCurrentUserWorkerIdAsync().Result);
            int score = 0;
            string index = null;
            Worker worker = WorkManager.GetWorkerByFinger(depotId, finger, ref index, ref score);
            if (worker == null) return (false, "找不到此人");
            
            var ret = WorkManager.DoSignin(depotId, worker.Id, "指纹");
            if (ret.Item1)
                return (true, ret.Item2 + string.Format("({0},得分{1})", index, score));
            else
                return ret;
        }
        public (bool, string) CheckinByRfid(string rfid, DateTime carryoutDate, int depotId, int affairId) 
        {
            Worker worker =  WorkManager.GetWorkerByRfid(rfid);
            if (worker == null) return (false, "找不到此人");
            var aw = WorkManager.GetCacheAffairWorker(carryoutDate, depotId, worker.Id);
            if (aw.Item1.Id != affairId) return (false, "一个人不能同时安排在同时段任务中");
            if (aw.Item2 == null) return (false, "此人没被安排在此任务中");
            WorkManager.DoCheckin(aw.Item1, aw.Item2, worker.Id);
            return (true, "刷卡验入成功");
        }

        public (bool, string) CheckinByFinger(string finger, int workerId, DateTime carryoutDate, int depotId, int affairId) 
        {
            var mR = WorkManager.MatchFinger(finger, workerId);
            if (!mR.Item1) return mR;

            var aw = WorkManager.GetCacheAffairWorker(carryoutDate, depotId, workerId);
            if (aw.Item1.Id != affairId) return (false, "一个人不能同时安排在同时段任务中");
            WorkManager.DoCheckin(aw.Item1, aw.Item2, workerId);
            return (true, "验入成功!" + mR.Item2);
        }
 
        #endregion

        #region Door
        public (bool, string) AskOpenDoor(DateTime carryoutDate, int depotId, int affairId)
        {
            return (false, "To do");
        }
        public (bool, string) AskOpenDoorTask(DateTime carryoutDate, int depotId, int affairTaskId)
        {
            return (false, "To do");

        }
        #endregion
        

        #region Article

        public RouteWorkerMatchResult MatchWorkerForArticle(bool isLend, DateTime carryoutDate, int depotId, int affairId, string rfid)
        {
            var depots = WorkManager.GetShareDepods(carryoutDate, depotId, affairId);
            var result = new RouteWorkerMatchResult();
            (RouteCacheItem, RouteWorkerCacheItem, RouteWorkerCacheItem) found = (null, null, null);
            foreach (var dopotId in depots)
            {
                found = FindEqualRfidWorker(_routeCache.Get(carryoutDate, depotId), rfid);
                if (found.Item1 != null) break;
            }

            if (found.Item1 == null) 
            {
                result.Message = "还未安排在激活的任务中";
                return result;
            }
                
            // RULE JUDGE
            var rt = _routeTypeCache[found.Item1.RouteTypeId];
            if (string.IsNullOrEmpty(_workRoleCache[found.Item2.WorkerId].ArticleTypeList)) {
               result.Message = "此人不需要领物";
               return result;
            }

            if (isLend && !ClcUtils.NowInTimeZone(found.Item1.StartTime, rt.LendArticleLead, rt.LendArticleDeadline)) {
               result.Message = "不在领物时间段";
               return result;
            }

            result.RouteMatched = new MatchedRouteDto(found.Item1);
            var w = found.Item2;
            result.WorkerMatched = new MatchedWorkerDto(w.Id, WorkManager.GetWorker(w.WorkerId), _workRoleCache[w.WorkRoleId]);
            result.Articles = GetArticles(w.Id);
            w = found.Item3;
            if (w != null)
            {
                result.WorkerMatched2 = new MatchedWorkerDto(w.Id, WorkManager.GetWorker(w.WorkerId), _workRoleCache[w.WorkRoleId]);
                result.Articles2 = GetArticles(w.Id);
            }
            return result;
        }
        
        public (string, RouteArticleCDto) MatchArticleForLend(string workerCn, string vehicleCn, string routeName, string articleTypeList, string rfid)
        {
            var article = _articleCache.GetList().FirstOrDefault(x => x.Rfid == rfid);
            if (article == null) return ("此Rfid没有对应的物品", null);

            var type = _articleTypeCache[article.ArticleTypeId];
            if (!articleTypeList.Contains(type.Cn)) return ("不允许领用此物品类型", null);

            if (!string.IsNullOrEmpty(article.BindInfo))
            {
                switch (type.BindStyle) {
                case "人":
                    if (!article.BindInfo.Contains(workerCn)) return ("此物品未绑定到此人", null);
                    break;
                case "车":
                    if (!article.BindInfo.Contains(vehicleCn)) return ("此物品未绑定到此车", null);
                    break;
                case "线":
                    if (!article.BindInfo.Contains(routeName)) return ("此物品未绑定到此线路", null);
                    break;
                default:
                    throw new InvalidOperationException("无此绑定类型");
                }
            }
            return ("", new RouteArticleCDto(article));
        }

        public (string, RouteArticleCDto) MatchArticleForReturn(string rfid)
        {
            var article = _articleCache.GetList().FirstOrDefault(x => x.Rfid == rfid);
            if (article == null) return ("此Rfid没有对应的物品", null);
            if (!article.ArticleRecordId.HasValue) return ("此物品需要先领用", null);
            
            return ("", new RouteArticleCDto(article));
        }

        #endregion

        #region Box

        public RouteWorkerMatchResult MatchWorkerForBox(DateTime carryoutDate, int depotId, int affairId, string rfid)
        {
            var depots = WorkManager.GetShareDepods(carryoutDate, depotId, affairId);
            var result = new RouteWorkerMatchResult();
            (RouteCacheItem, RouteWorkerCacheItem, RouteWorkerCacheItem) found = (null, null, null);
            foreach (var dopotId in depots)
            {
                found = FindEqualRfidWorker(_routeCache.Get(carryoutDate, depotId), rfid);
                if (found.Item1 != null) break;
            }

            if (found.Item1 == null) 
            {
                result.Message = "还未安排在激活的任务中";
                return result;
            }
                
            // RULE JUDGE
            var wr = _workRoleCache[found.Item2.WorkRoleId];
            if (string.IsNullOrEmpty(wr.Duties) || !wr.Duties.Contains("尾箱")) {
               result.Message = "此人非尾箱交接人员";
               return result;
            }

            result.RouteMatched = new MatchedRouteDto(found.Item1);
            var w = found.Item2;
            result.WorkerMatched = new MatchedWorkerDto(w.Id, WorkManager.GetWorker(w.WorkerId), _workRoleCache[w.WorkRoleId]);
            result.Boxes = new List<RouteBoxCDto>();
            return result;
        }

        public (string, RouteBoxCDto) MatchInBox(DateTime carryoutDate, int affairId, int routeId, string rfid)
        {
            var box = _boxCache.GetList().FirstOrDefault(x => x.Cn == rfid);
            if (box == null) return ("没有对应的尾箱", null);
            return (null, null);
        }

        public (string, RouteBoxCDto) MatchOutBox(DateTime carryoutDate, int affairId, int routeId, string rfid)
        {
            var box = _boxCache.GetList().FirstOrDefault(x => x.Cn == rfid);
            if (box == null) return ("没有对应的尾箱", null);
            if (!box.BoxRecordId.HasValue) return ("此尾箱需要先入库", null);

            return (null, null);

            // var outlet = _outletCache[box.OutletId];
            // foreach (var route in _routeCache.Get(carryoutDate, affairId, "Box"))
            //     foreach (var t in route.Tasks)
            //         if (t.OutletId == box.OutletId && t.TaskTypeId == 1) 
            //         {
            //             var dto = new RouteBoxCDto() { 
            //                 TaskId = t.Id,
            //                 OutletId = outlet.Id,
            //                 Outlet = $"{outlet.Cn} {outlet.Name}",
            //                 BoxId = box.Id, 
            //                 DisplayText = box.Name,
            //                 RecordId = box.BoxRecordId.Value
            //             };
            //             return ("", dto);
            //         }
            // return ("此尾箱所属不在任务列表中", null);
        }

        #endregion

        #region private

        private (RouteCacheItem, RouteWorkerCacheItem, RouteWorkerCacheItem) FindEqualRfidWorker(List<RouteCacheItem> routes, string rfid)
        {           
            foreach (var route in routes)
            {
                foreach (var w in route.Workers) 
                {
                    var worker = WorkManager.GetWorker(w.WorkerId);
                    if (worker.Rfid == rfid) {
                        RouteWorkerCacheItem w2 = FindAnotherRouteWorker(route.Workers, _workRoleCache[w.WorkRoleId]);
                        return (route, w, w2);
                    }
                }
            }
            return (null, null, null);
        }

        private RouteWorkerCacheItem FindAnotherRouteWorker(List<RouteWorkerCacheItem> workers, WorkRole workRole)
        {
            string anotherRole = GetAnotherRoleName(workRole);
            if ( anotherRole!= null )
            {
                foreach (var w in workers)
                {
                    var role = _workRoleCache[w.WorkRoleId];
                    if (role.Name == anotherRole)
                        return w;
                }
            }
            return null;
        }

        private string GetAnotherRoleName(WorkRole workRole)
        {
            string doubleRoles = SettingManager.GetSettingValueAsync(AppSettingNames.Rule.DoubleArticleRoles).Result;
            string[] array = doubleRoles.Split('|');
            if (array[0] == workRole.Name) return array[1];
            if (array[1] == workRole.Name) return array[0];
            return null;
        }

        private List<RouteArticleCDto> GetArticles(int workerId)
        {
            List<RouteArticleCDto> ret = new List<RouteArticleCDto>();
            var articles = _routeArticleRepository.GetAllIncluding(x => x.ArticleRecord).Where(x => x.RouteWorkerId == workerId).ToList();
            foreach (var a in articles)
            {
                var article = WorkManager.GetArticle(a.ArticleRecord.ArticleId);
                ret.Add(new RouteArticleCDto(article, a.ArticleRecordId, a.ArticleRecord.ReturnTime.HasValue));
            }
            return ret;
        }

        private string GetWorkersInfo(AffairCacheItem affair)
        {
            string info = null;
            foreach (var aw in affair.Workers)
            {
                info += string.Format("{0} ", WorkManager.GetWorker(aw.WorkerId).Name);
            }
            return info;
        }

        #endregion
    }
}