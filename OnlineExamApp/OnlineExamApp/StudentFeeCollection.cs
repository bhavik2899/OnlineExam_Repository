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
    
    public partial class StudentFeeCollection
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public StudentFeeCollection()
        {
            this.StudentFeeCollectionHeads = new HashSet<StudentFeeCollectionHead>();
        }
    
        public long PK_StudentFeeCollectionId { get; set; }
        public long FK_StudentId { get; set; }
        public long FK_StudentFeeScheduleId { get; set; }
        public long FK_FeeStructurId { get; set; }
        public decimal PaidAmount { get; set; }
        public System.DateTime FeePaidDate { get; set; }
        public Nullable<System.DateTime> FeeCancellationDate { get; set; }
        public Nullable<int> SeqNo { get; set; }
        public Nullable<long> ReceiptNO { get; set; }
    
        public virtual FeeStructur FeeStructur { get; set; }
        public virtual Student Student { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<StudentFeeCollectionHead> StudentFeeCollectionHeads { get; set; }
        public virtual StudentFeeSchedule StudentFeeSchedule { get; set; }
    }
}
