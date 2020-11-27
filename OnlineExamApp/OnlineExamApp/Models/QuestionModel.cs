using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExamApp.Models
{
	public class QuestionModel
	{
        public long QueId { get; set; }

        [Required(ErrorMessage = "Please Enter Title.")]
        public string Title { get; set; }

        public  int NoOfOption { get; set; }

        public List<OptionModel> OptionList = new List<OptionModel>();

        public string Details { get; set; }

        public string CorrectAnsId { get; set; }

        public bool IsMultichoice { get; set; }

        public string OptionRowID { get; set; }
    }

    public class OptionModel
    {
        public long OptionId { get; set; }

        [Required(ErrorMessage = "Please Enter Title.")]
        public string Title { get; set; }
        public int? OrderNo { get; set; }
        public string Details { get; set; }
        public bool IsCorrect { get; set; }
    }
}