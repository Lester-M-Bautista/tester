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
    
    public partial class PERMISSIONS
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public PERMISSIONS()
        {
            this.PERMISSIONSOFROLE = new HashSet<PERMISSIONSOFROLE>();
        }
    
        public System.Guid APPLICATIONID { get; set; }
        public System.Guid PERMISSIONID { get; set; }
        public string PERMISSIONNAME { get; set; }
        public string DESCRIPTION { get; set; }
    
        public virtual APPLICATIONS APPLICATIONS { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<PERMISSIONSOFROLE> PERMISSIONSOFROLE { get; set; }
    }
}
