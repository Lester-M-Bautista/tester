namespace Infra.Common
{
    public class ValidationErrorMessage
    {
        public const string RequiredField = "{0} is a required field.";
        public const string StringLengthExceeded = "{0} exceed {1} characters.";
        public const string InvalidInput = "{0} is not valid. Valid values are {1}.";

        public const string DuplicateError = "{0} already exists.";
        public const string NotExistError = "{0} does not exist.";
        public const string FKViolationError = "{0} is in use in table {1}.";

        public const string GenericDBSaveError = "Provider Error. Saving {0} Failed.";
        public const string GenericDBDeleteError = "Provider Error. Deleting {0} Failed.";

        public const string GenericDBCustomMsg = "{0}";
        public const string MappedToNullDTO = "Result mapped to null {0} DTO.";
    }
}
