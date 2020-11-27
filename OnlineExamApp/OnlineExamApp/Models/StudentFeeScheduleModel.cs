using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExamApp.Models
{
    public class StudentFeeScheduleModel
    {
        [Required(ErrorMessage = "Please Select FeeStructure.")]
        public long FeeStructurId { get; set; }
        public IEnumerable<SelectListItem> FeeStructureList { get; set; }
        public decimal TotalFee { get; set; }
        public List<StudentsByFeeStructureModel> StudentsByFeeStructure { get; set; }
    }

    public class StudentsByFeeStructureModel
    {
        public long StudentID { get; set; }
        public string Name { get; set; }
        public long? StudentFeeScheduleId { get; set; }

        [Range(0, 100,ErrorMessage = "You can not give Discount more then 100%.")]
        public int? DiscountAmountPercentage { get; set; }
        public decimal? TotalPayableAmount { get; set; }
        public bool IsInclude { get; set; }
        public bool UnPaidFee { get; set; }
    }
}