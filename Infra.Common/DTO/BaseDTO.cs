using FluentValidation.Results;

namespace Infra.Common.DTO
{
    public class BaseDTO
    {
        public string Message { get; set; }
        public ValidationResult Validation { get; set; }
        public TransactionContext TransContext { get; set; }
        public string ErrorMessage { get; set; }
        public bool Succeed { get; set; }
    }

    public class SortParameter
    {
        public string ColumnName { get; set; }
        public bool IsAscending { get; set; }
    }
}
