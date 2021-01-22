using Macrin.Common;

namespace Infra.Common.DTO
{
    public class Logs : Macrin.Common.BaseDTO
    {
        public System.Guid Logid { get; set; }
        public string Category { get; set; }
        public string ApplicationName { get; set; }
        public string ApplicationVersion { get; set; }
        public string ComputerName { get; set; }
        public string Actor { get; set; }
        public string Action { get; set; }
        public string Message { get; set; }
        public string Trace { get; set; }
        public System.DateTime Timestamp { get; set; }
    }
}
