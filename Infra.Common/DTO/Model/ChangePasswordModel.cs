
using Infra.Common.DTO;

namespace Infra.Common
{
    public class ChangePasswordModel : Macrin.Common.BaseDTO
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
