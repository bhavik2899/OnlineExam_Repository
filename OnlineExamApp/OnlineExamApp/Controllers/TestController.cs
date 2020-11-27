using Newtonsoft.Json;
using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Xml.Linq;

namespace OnlineExamApp.Controllers
{
	public class TestController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();

		// GET: Test
		public ActionResult TestList()
		{
			var errMsg = TempData["ErrorMessage"] as string;
			var paperList = new List<ShortPaperModel>();
			var papers = _Context.sp_GetPaperList().ToList();

			foreach (var item in papers)
			{
				var paper = new ShortPaperModel();
				paper.PaperId = item.PK_PaperMasterId;
				paper.Title = item.Title;
				paper.StartTime = item.StartTime;
				paper.EndTime = item.EndTime;
				try
				{
					paper.ExamDate = Convert.ToDateTime(item.ExamDate);
				}
				catch { }
				paperList.Add(paper);
			}

			return View(paperList);
		}

		public ActionResult TestConduct(int id = 0)
		{
			var testmodel = new TestModel();
			var time = DateTime.Now.TimeOfDay;

			var timeMatchPaperId = _Context.sp_PaperCompareTime(id, time).FirstOrDefault();
			if (timeMatchPaperId == null)
			{
				TempData["ErrorMessage"] = "Please check paper Start Time.";
				return RedirectToAction("TestList");
			}
			testmodel.ConductDateTime = DateTime.Now;
			testmodel.PaperID = id;

			string DATA = _Context.sp_GetQueByPaperID(id).Last();

			testmodel.Questions = JsonConvert.DeserializeObject<List<QuewithOptionModel>>(DATA);

			return View(testmodel);
		}

		[HttpPost]
		public ActionResult TestConduct(TestModel tm, FormCollection fc)
		{
			TryUpdateModel(tm);
			XDocument questionAns = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
			 new XElement("Paper",
				 from queList in tm.Questions
				 select new XElement("Questions",
				 new XElement("OptionsList",
				 from opList in queList.qom
				 select new XElement("Option",
				 new XElement("QueId", queList.PK_QueMasterId),
				 new XElement("OpId", opList.PK_QueOptionMasterId),
				 new XElement("IsSelected", fc["oprion_" + queList.PK_QueMasterId] != null ? fc["oprion_" + queList.PK_QueMasterId].ToString().IndexOf(opList.PK_QueOptionMasterId.ToString()) >= 0 ? true : false : false
				 ))))));

			_Context.sp_InsertTestConduct(tm.UserName, tm.Age, tm.ConductDateTime,tm.PaperID, questionAns.ToString());
			return RedirectToAction("TestList");
		}

		public ActionResult Error()
		{
			return View();
		}
	}
}