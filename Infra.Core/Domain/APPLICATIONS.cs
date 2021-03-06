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
    
    public partial class APPLICATIONS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public APPLICATIONS()
        {
            this.PERMISSIONS = new HashSet<PERMISSIONS>();
            this.ROLES = new HashSet<ROLES>();
            this.REFRESHTOKEN = new HashSet<REFRESHTOKEN>();
        }
    
        public System.Guid APPLICATIONID { get; set; }
        public string APPLICATIONNAME { get; set; }
        public string DESCRIPTION { get; set; }
        public bool ISDELETED { get; set; }
        public System.Guid APPLICAITONSECRET { get; set; }
        public string REDIRECTURL { get; set; }
        public int REFRESHLIFETIME { get; set; }
        public bool ISNATIVE { get; set; }
        public string ALLOWEDORIGIN { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PERMISSIONS> PERMISSIONS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<ROLES> ROLES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<REFRESHTOKEN> REFRESHTOKEN { get; set; }
    }
}
