using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface ILogsBL : IEntity<Logs, Guid>
    {
        #region Synchronous Methods
        void Log(string level, string action, object dto, string message);
        Task<List<Logs>> GetLogs(DateTime datefrom, DateTime dateto);
        Task<Logs> LogAsync(string level, string action, object dto, string message);


        Logs GetById(Guid id);
        List<Logs> List();
        List<Logs> List(int page, int size);
        List<Logs> List(int page, int size, Logs filter);
        Logs Delete(Logs item);
        #endregion

        #region Asynchronous Methods
        Task<Logs> GetByIdAsync(Guid id);
        Task<List<Logs>> ListAsync();
        Task<List<Logs>> ListAsync(int page, int size);
        Task<List<Logs>> ListAsync(int page, int size, Logs filter);
        //  Task<Log> DeleteAsync(Log item);
        #endregion
    }
}

