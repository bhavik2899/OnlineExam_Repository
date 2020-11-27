using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExamApp.Models
{
	public class PaperModel
	{
        public long PaperId { get; set; }

        [Required(ErrorMessage = "Please Enter Title.")]
        public string Title { get; set; }
        public string Details { get; set; }
        public Nullable<System.TimeSpan> StartTime { get; set; }
        public Nullable<System.TimeSpan> EndTime { get; set; }
        public string ExamDate { get; set; }
        public Nullable<int> NoOfQuestion { get; set; }
        public Nullable<int> TotalMarks { get; set; }
        public Nullable<int> PassingMarks { get; set; }

        public List<ShortQueModel> QuestionList { get; set; }
    }

    public class ShortQueModel
    {
        public long? QueId { get; set; }

        public bool IsSelected { get; set; }

        public string Title { get; set; }

        public int? OrderNo { get; set; }
    }
}