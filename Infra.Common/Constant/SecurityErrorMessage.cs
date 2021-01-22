namespace Infra.Common
{
    public class SecurityErrorMessage
    {
        public const string UserDoesNotExist = "Username does not exist.";
        public const string UserIsLockedOut = "User is locked-Out.";
        public const string UserIsNotActive = "User is not active.";
        public const string UserAccessIsExpired = "User access has expired.";
        public const string InvalidPasswordFormat = "Password format not supported.";

        public const string AuthenticationFailed = "Authentication failed.";
        public const string AuthorizationFailed = "Authorization failed.";
    }
}
