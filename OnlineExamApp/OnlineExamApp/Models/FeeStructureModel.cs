using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExamApp.Models
{
	public class FeeStructureModel
	{
        public long FeeStructurId { get; set; }
        public string StructurName { get; set; }
        public decimal TotalFee { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }

        public List<FeeHeadClass> feeheads { get; set; }
    }

    public class FeeHeadClass
    {
        public long FeeHeadId { get; set; }
        public string HeadName { get; set; }
        public bool IsOptional { get; set; }
        public decimal Amount { get; set; }
        public bool IsInclude { get; set; }
    }

}