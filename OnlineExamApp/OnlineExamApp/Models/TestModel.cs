using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExamApp.Models
{
	public class TestModel
	{
		public long TestCoductId { get; set; }

		[Required(ErrorMessage = "UserName is Required.")]
		[DisplayName("UserName :")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Age is Required.")]
		[DisplayName("Age :")]
		public Nullable<int> Age { get; set; }
		public List<QuewithOptionModel> Questions { get; set; }

		public System.DateTime? ConductDateTime { get; set; }

		public long? PaperID { get; set; }
	}

	public class ShortPaperModel
	{
		public long PaperId { get; set; }
		public string Title { get; set; }
		public Nullable<System.TimeSpan> StartTime { get; set; }
		public Nullable<System.TimeSpan> EndTime { get; set; }
		public DateTime ExamDate { get; set; }
	}

	public class QuewithOptionModel
	{
		public long? QuestionOrder { get; set; }
		public long? OptionOrder { get; set; }
		public long? PK_QueMasterId { get; set; }
		public string Title { get; set; }
		public bool IsMultichoice { get; set; }
		public string PK_QueOptionMasterId { get; set; }
		public List<OptionOFQuestionModel> qom { get; set; }
	}

	public class OptionOFQuestionModel
	{
		public long? PK_QueOptionMasterId { get; set; }

		public bool IsSelected { get; set; }

		public string OptionTitle { get; set; }

		public bool IsCorrect { get; set; }
	}
}