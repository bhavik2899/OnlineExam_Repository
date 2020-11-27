  using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExamApp.Models
{
	public class StudentFeeCollectionModel
	{
		public long StudentID { get; set; }
        public string StudentName { get; set; }
		public IEnumerable<SelectListItem> StudentList { get; set; }
        public List<FeeScheduleModel> FeeScheduleModelList { get; set; }
    }

    public class FeeScheduleModel
    {
        public long FeeStructureId { get; set; }
        public string StructurName { get; set; }
        public long StudentFeeScheduleId { get; set; }
        public decimal TotalFeeAmount { get; set; }
        public decimal TotalPayableAmount { get; set; }
        public decimal? PaidAmount { get; set; }
        public List<FeeHeadModel> feeHeadModelList { get; set; }
    }

    public class FeeHeadModel
    {
        public long FeeHeadId { get; set; }
        public bool IsOptional { get; set; }
        public string HeadName { get; set; }
        public decimal AmountByHead { get; set; }
        public decimal? PaidHeadAmount { get; set; }
        public decimal PayableHeadAmount { get; set; }
    }
}


