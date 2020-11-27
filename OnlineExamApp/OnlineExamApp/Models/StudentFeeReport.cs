using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExamApp.Models
{
    public class StudentFeeDetailsModel
    {
        public long StudentID { get; set; }
        public string StudentName { get; set; }
        public List<FeeStructreModel> FeeStructureModelList { get; set; }
    }

    public class FeeStructreModel
    {
        public string StructurName { get; set; }
        public List<HeadModel> HeadModelList { get; set; }
    }

    public class HeadModel
    {
        public string HeadName { get; set; }
        public decimal TotalHeadAmount { get; set; }
        public decimal PaidHeadAmount { get; set; }
        public decimal PayableHeadAmount { get; set; }
    }
}