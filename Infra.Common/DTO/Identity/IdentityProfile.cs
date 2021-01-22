using Macrin.Common;
using System;

namespace Infra.Common.DTO.Identity
{
    public class IdentityProfile : Macrin.Common.BaseDTO
    {
        public Guid Useraccountid { get; set; }
        public DateTime? Birthdate { get; set; }
        public string Gender { get; set; }
        public byte[] Profilepicture { get; set; }
    }

    public class IdentityProfileFull : IdentityProfile
    {
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public Guid Department { get; set; }
    }
}
