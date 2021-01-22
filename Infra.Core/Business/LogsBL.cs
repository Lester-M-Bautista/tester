using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Infra.Core.Domain;
using LinqKit;
using log4net;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Infra.Core.Business
{
    public class LogsBL : BaseBL<Logs, LOGS, Guid>, ILogsBL
    {
        private readonly InfraEntities _ctx;
        private readonly ILog _logger;
        private readonly int _maxPageSize;
        public LogsBL(InfraEntities ctx)
        {
            _ctx = ctx;
            _logger = LogManager.GetLogger("Logger");
            _maxPageSize = string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("MaxPageSize")) ? 100
                : int.Parse(ConfigurationManager.AppSettings.Get("MaxPageSize").ToString());
        }

        public Task<Logs> DeleteAsync(Logs item)
        {
            throw new NotImplementedException();
        }

        public async Task<Logs> GetByKeyAsync(Guid key)
        {
            LOGS item = await _ctx.LOGS.AsNoTracking().FirstOrDefaultAsync(x => x.LOGID == key);
            return MapToDTO(item);
        }

        public async Task<DataList<Logs>> List(Logs filter, PageConfig config)
        {
            IQueryable<LOGS> query = FilteredEntities(filter);

            string resolved_sort = config.SortBy ?? "Logid";
            bool resolved_isAscending = (config.IsAscending) ? config.IsAscending : false;

            int resolved_size = config.Size ?? _maxPageSize;
            if (resolved_size > _maxPageSize) resolved_size = _maxPageSize;
            int resolved_index = config.Index ?? 1;

            query = OrderEntities(query, resolved_sort, resolved_isAscending);
            var paged = PagedQuery(query, resolved_size, resolved_index);
            return new DataList<Logs>
            {
                Count = await query.CountAsync(),
                Items = await QueryToDTO(paged).ToListAsync()
            };
        }

        public void Log(string level, string action, object dto, string message)
        {

            Macrin.Common.BaseDTO trans = (Macrin.Common.BaseDTO)dto;
            GlobalContext.Properties["Category"] = level;
            GlobalContext.Properties["ApplicationName"] = trans.TransContext == null ? string.Empty : trans.TransContext.ApplicationName;
            GlobalContext.Properties["ApplicationVersion"] = trans.TransContext == null ? string.Empty : trans.TransContext.ApplicationVersion;
            GlobalContext.Properties["ComputerName"] = trans.TransContext == null ? string.Empty : trans.TransContext.ComputerName;
            GlobalContext.Properties["Username"] = trans.TransContext == null ? string.Empty : trans.TransContext.Username;
            GlobalContext.Properties["Action"] = action;
            GlobalContext.Properties["Trace"] = ToXML(dto);

            switch (level.ToUpperInvariant())
            {
                case "DEBUG":
                    _logger.Debug(message);
                    break;
                case "INFO":
                    _logger.Info(message);
                    break;
                case "WARN":
                    _logger.Warn(message);
                    break;
                case "ERROR":
                    _logger.Error(message);
                    break;
                case "FATAL":
                    _logger.Fatal(message);
                    break;
                default:
                    _logger.Error(message);
                    break;
            }
        }

        public Task<Logs> LogAsync(string level, string action, object dto, string message)
        {
            throw new NotImplementedException();
        }

        public Task<Logs> SaveAsync(Logs item)
        {
            throw new NotImplementedException();
        }

        protected override IQueryable<LOGS> FilteredEntities(Logs filter, IQueryable<LOGS> custom_query = null, bool strict = false)
        {
            var predicate = PredicateBuilder.New<LOGS>(true);
            if (!string.IsNullOrEmpty(filter.ApplicationName)) predicate = (strict)
                        ? predicate.And(x => x.APPLICATIONNAME.ToLower() == filter.ApplicationName.ToLower())
                        : predicate.And(x => x.APPLICATIONNAME.ToLower().Contains(filter.ApplicationName.ToLower()));

            if (!string.IsNullOrEmpty(filter.ComputerName)) predicate = (strict)
                            ? predicate.And(x => x.COMPUTERNAME.ToLower() == filter.ComputerName.ToLower())
                            : predicate.And(x => x.COMPUTERNAME.ToLower().Contains(filter.ComputerName.ToLower()));
            var query = custom_query ?? _ctx.LOGS;
            return query.Where(predicate);
        }

        protected override Guid GenerateId(string sequenceName = null)
        {
            throw new NotImplementedException();
        }

        protected override Task<Logs> Insert(Logs item)
        {
            throw new NotImplementedException();
        }

        protected override Logs MapToDTO(LOGS item)
        {
            if (item == null) return new Logs();
            return new Logs
            {
              Action = item.ACTION,
              Actor = item.ACTOR,
              ApplicationName = item.APPLICATIONNAME,
              ApplicationVersion = item.APPLICATIONVERSION,
              Category = item.CATEGORY,
              ComputerName = item.COMPUTERNAME,
              Logid = item.LOGID,
              Message = item.MESSAGE,
              Timestamp = item.TIMESTAMP,
              Trace = item.TRACE,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            };
        }

        protected override LOGS MapToEntity(Logs item)
        {
            if (item == null) return new LOGS();
            return new LOGS
            {
                ACTION = item.Action,
                ACTOR = item.Actor,
                APPLICATIONNAME = item.ApplicationName,
                APPLICATIONVERSION = item.ApplicationVersion,
                CATEGORY = item.Category,
                COMPUTERNAME = item.ComputerName,
                LOGID = item.Logid,
                MESSAGE = item.Message,
                TIMESTAMP = item.Timestamp,
                TRACE = item.Trace,
            };
        }

        protected override IQueryable<LOGS> OrderEntities(IQueryable<LOGS> query, string sortOrder, bool isAscending)
        {
            switch (sortOrder.ToUpper())
            {
                case "ACTION":
                    query = isAscending ? query.OrderBy(x => x.ACTION) : query.OrderByDescending(x => x.ACTION);
                    break;
                case "ACTOR":
                    query = isAscending ? query.OrderBy(x => x.ACTOR) : query.OrderByDescending(x => x.ACTOR);
                    break;
                case "APPLICATIONNAME":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONNAME) : query.OrderByDescending(x => x.APPLICATIONNAME);
                    break;
                case "APPLICATIONVERSION":
                    query = isAscending ? query.OrderBy(x => x.APPLICATIONVERSION) : query.OrderByDescending(x => x.APPLICATIONVERSION);
                    break;
                case "CATEGORY":
                    query = isAscending ? query.OrderBy(x => x.CATEGORY) : query.OrderByDescending(x => x.CATEGORY);
                    break;
                case "COMPUTERNAME":
                    query = isAscending ? query.OrderBy(x => x.COMPUTERNAME) : query.OrderByDescending(x => x.COMPUTERNAME);
                    break;
                case "MESSAGE":
                    query = isAscending ? query.OrderBy(x => x.MESSAGE) : query.OrderByDescending(x => x.MESSAGE);
                    break;
                case "TIMESTAMP":
                    query = isAscending ? query.OrderBy(x => x.TIMESTAMP) : query.OrderByDescending(x => x.TIMESTAMP);
                    break;
                case "TRACE":
                    query = isAscending ? query.OrderBy(x => x.TRACE) : query.OrderByDescending(x => x.TRACE);
                    break;
                default:
                    query = query.OrderBy(x => x.TIMESTAMP);
                    break;
            }
            return query;
        }

        protected override IQueryable<Logs> QueryToDTO(IQueryable<LOGS> query)
        {
            return query.Select(x => new Logs
            {
                Action = x.ACTION,
                Actor = x.ACTOR,
                ApplicationName = x.APPLICATIONNAME,
                ApplicationVersion = x.APPLICATIONVERSION,
                Category = x.CATEGORY,
                ComputerName = x.COMPUTERNAME,
                Logid = x.LOGID,
                Message = x.MESSAGE,
                Timestamp = x.TIMESTAMP,
                Trace = x.TRACE,
                TransContext = new Macrin.Common.TransactionContext(),
                Validation = new ValidationResult()
            });
        }

        protected override Task<Logs> Update(Logs item)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Logs>> GetLogs(DateTime datefrom, DateTime dateto)
        {
            return await _ctx.LOGS.AsNoTracking()
                 .Where(x => DbFunctions.TruncateTime(x.TIMESTAMP) >= DbFunctions.TruncateTime(datefrom)
                 && DbFunctions.TruncateTime(x.TIMESTAMP) <= DbFunctions.TruncateTime(dateto)
                 )
                 .OrderByDescending(z => z.TIMESTAMP)
               .Select(item => new Logs
               {
                   Action = item.ACTION,
                   Actor = item.ACTOR,
                   ApplicationName = item.APPLICATIONNAME,
                   ApplicationVersion = item.APPLICATIONVERSION,
                   Category = item.CATEGORY,
                   ComputerName = item.COMPUTERNAME,
                   Logid = item.LOGID,
                   Message = item.MESSAGE,
                   Timestamp = item.TIMESTAMP,
                   Trace = item.TRACE,

               }).ToListAsync();
        }

        private string ToXML(Object oObject)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlSerializer xmlSerializer = new XmlSerializer(oObject.GetType());
            using (MemoryStream xmlStream = new MemoryStream())
            {
                xmlSerializer.Serialize(xmlStream, oObject);
                xmlStream.Position = 0;
                xmlDoc.Load(xmlStream);
                return xmlDoc.InnerXml;
            }
        }

        public Logs GetById(Guid id)
        {
            throw new NotImplementedException();
        }

        public List<Logs> List()
        {
            throw new NotImplementedException();
        }

        public List<Logs> List(int page, int size)
        {
            throw new NotImplementedException();
        }

        public List<Logs> List(int page, int size, Logs filter)
        {
            throw new NotImplementedException();
        }

        public Logs Delete(Logs item)
        {
            throw new NotImplementedException();
        }

        public Task<Logs> GetByIdAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Logs>> ListAsync()
        {
            throw new NotImplementedException();
        }

        public Task<List<Logs>> ListAsync(int page, int size)
        {
            throw new NotImplementedException();
        }

        public Task<List<Logs>> ListAsync(int page, int size, Logs filter)
        {
            throw new NotImplementedException();
        }
    }
}
