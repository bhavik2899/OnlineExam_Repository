using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExamApp.Controllers
{
	public class StudentFeeReportController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();
		// GET: StudentFeeReport
		public ActionResult StudentFeeReport()
		{
			DataTable table = new DataTable();
			try {
				using (var con = new SqlConnection(@"Data Source=DESKTOP-CLVM9J2;Initial Catalog=OnlineExam;Integrated Security=True;MultipleActiveResultSets=True;Application Name=EntityFramework"))
				using (var cmd = new SqlCommand("Sp_Report_Student_FeeCollection", con))
				using (var da = new SqlDataAdapter(cmd))
				{
					cmd.CommandType = CommandType.StoredProcedure;
					da.Fill(table);
				}
			} catch(Exception ex) { }
			
			

			return View(table);
		}
	}
}