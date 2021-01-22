using Macrin.Common;
using System;
using System.Runtime.Serialization;

namespace Infra.Common.DTO
{
    public class Usersinrole : Macrin.Common.BaseDTO
    {
        public Guid Roleid { get; set; }
        public Guid Userid { get; set; }
        public Guid Userinroleid { get; set; }
        [IgnoreDataMember]
        public string testproperty { get; set; }
    }
}
