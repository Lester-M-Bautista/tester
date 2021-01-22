namespace Infra.Core.Domain
{
    public class IdentityDBContext
    {
        public static InfraEntities Create()
        {
            return new InfraEntities();
        }
    }
}
