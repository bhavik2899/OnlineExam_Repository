//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace OnlineExamApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class StudentFeeSchedule
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StudentFeeSchedule()
        {
            this.StudentFeeCollections = new HashSet<StudentFeeCollection>();
            this.StudentFeeCollectionHeads = new HashSet<StudentFeeCollectionHead>();
        }
    
        public long PK_StudentFeeScheduleId { get; set; }
        public Nullable<decimal> TotalFeeAmount { get; set; }
        public System.DateTime ScheduleDate { get; set; }
        public Nullable<int> DiscountAmountPercentage { get; set; }
        public Nullable<decimal> TotalPayableAmount { get; set; }
        public long FK_StudentID { get; set; }
        public Nullable<long> FK_FeeStructurId { get; set; }
    
        public virtual FeeStructur FeeStructur { get; set; }
        public virtual Student Student { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentFeeCollection> StudentFeeCollections { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentFeeCollectionHead> StudentFeeCollectionHeads { get; set; }
    }
}
