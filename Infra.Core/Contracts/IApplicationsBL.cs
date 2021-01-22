using Infra.Common.DTO;
using Macrin.Common;
using System;
using System.Threading.Tasks;

namespace Infra.Core.Contract
{
    public interface IApplicationsBL : IEntity<Applications, Guid>
    {
        Task<Applications> GetByIdAsync(Guid id);
        Task<Applications> GetByNameAsync(string name);
        Task<bool> ValidateAsync(string application, Guid secret);
    }
}
