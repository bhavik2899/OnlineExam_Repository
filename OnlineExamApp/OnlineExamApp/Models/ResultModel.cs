using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExamApp.Models
{
	public class ResultModel
	{
		[DisplayName("Enter Date:")]
		public DateTime? PaperDate { get; set; }

		[DisplayName("Select Paper:")]
		public long PaperId { get; set; }

		public List<sp_ResultReport_Result> ResultReport { get; set; }

	}

	public class TestSolutionModel
	{
		[DisplayName("Select Paper:")]
		public long PaperId { get; set; }
		public IEnumerable<SelectListItem> TestList { get; set; }

		public List<QuewithOptionModel> Questions { get; set; }
	}

	public class StudentSolutionModel
	{
		[DisplayName("Select Paper:")]
		public long PaperId { get; set; }
		public IEnumerable<SelectListItem> TestList { get; set; }

		public List<SingleStudentSolutionModel> Students { get; set; }
	}

	public class SingleStudentSolutionModel
	{
		public string UserName { get; set; }
		public int? TotalMarks { get; set; }
		public int? PassingMarks { get; set; }
		public int? ResultMarks { get; set; }
		public string Result { get; set; }

		public List<QuewithOptionModel> Questions { get; set; }

	}
}