//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Infra.Core.Domain
{
    using System;
    using System.Collections.Generic;
    
    public partial class ROLES
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public ROLES()
        {
            this.PERMISSIONSOFROLE = new HashSet<PERMISSIONSOFROLE>();
            this.USERSINROLE = new HashSet<USERSINROLE>();
        }
    
        public System.Guid APPLICATIONID { get; set; }
        public System.Guid ROLEID { get; set; }
        public string ROLENAME { get; set; }
        public string DESCRIPTION { get; set; }
        public decimal ISDELETED { get; set; }
    
        public virtual APPLICATIONS APPLICATIONS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PERMISSIONSOFROLE> PERMISSIONSOFROLE { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<USERSINROLE> USERSINROLE { get; set; }
    }
}
