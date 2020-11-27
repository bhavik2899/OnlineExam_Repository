using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace OnlineExamApp.Controllers
{
	public class PaperController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();

		// GET: Paper
		public ActionResult PaperList()
		{
			var paperList = new List<PaperModel>();
			var papers = _Context.PaperMasters.Where(x => x.IsActive == true).ToList();
			foreach (var paper in papers)
			{
				var pm = new PaperModel();
				pm.Title = paper.Title;
				pm.PaperId = paper.PK_PaperMasterId;
				pm.StartTime = paper.StartTime;
				pm.EndTime = paper.EndTime;
				if (paper.ExamDate.HasValue) { pm.ExamDate = paper.ExamDate.Value.ToShortDateString(); }
				pm.NoOfQuestion = paper.NoOfQuestion;
				pm.TotalMarks = paper.TotalMarks;

				paperList.Add(pm);
			}
			return View(paperList);
		}

		public ActionResult AddEditPaper(int id = 0)
		{
			var matchPaperID = _Context.TestConductMasters.Any(x => x.FK_PaperMasterId == id);
			if (matchPaperID == true)
			{
				TempData["ErrorMessage"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}

			var paperModel = new PaperModel();
			var queList = new List<ShortQueModel>();

			var questions = _Context.sp_GetQuestionByPaperID(id).ToList();
			var paper = _Context.sp_GetPaperByPrimaryKey(id).FirstOrDefault();

			foreach (var item in questions)
			{
				var que = new ShortQueModel();
				que.QueId = item.PK_QueMasterId;
				que.Title = item.Title;
				que.OrderNo = item.OrderNo;
				que.IsSelected = Convert.ToBoolean(item.IsSelected);
				queList.Add(que);
			}
			paperModel.QuestionList = queList;

			try
			{
				paperModel.PaperId = paper.PK_PaperMasterId;
				paperModel.Title = paper.Title;
				paperModel.Details = paper.Details;
				paperModel.StartTime = paper.StartTime;
				paperModel.EndTime = paper.EndTime;
				if (paper.ExamDate.HasValue) { paperModel.ExamDate = paper.ExamDate.Value.ToShortDateString(); }
				paperModel.NoOfQuestion = paper.NoOfQuestion;
				paperModel.TotalMarks = paper.TotalMarks;
				paperModel.PassingMarks = paper.PassingMarks;
			}
			catch { }

			return View(paperModel);
		}

		[HttpPost]
		public ActionResult AddEditPaper(PaperModel pm)
		{
			var questionList = new List<ShortQueModel>();

			XDocument questions = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
			 new XElement("QuestionList",
				 from QList in pm.QuestionList
				 select new XElement("Question",
				 new XElement("QueId", QList.QueId),
				 new XElement("IsIncluded", QList.IsSelected),
				 new XElement("OrderNo", QList.OrderNo)))
			 );

			DateTime? examDate = null;
			if (pm.ExamDate != null)
			{
				try
				{
					examDate = Convert.ToDateTime(pm.ExamDate);
				}
				catch { }
			}

			_Context.sp_InsertUpdatePaper(pm.PaperId, pm.Title, pm.Details, examDate, pm.StartTime, pm.EndTime, pm.TotalMarks, pm.PassingMarks, questions.ToString());

			return RedirectToAction("PaperList");
		}

		public ActionResult DeletePaper(int id = 0)
		{
			var matchPaperID = _Context.TestConductMasters.Any(x => x.FK_PaperMasterId == id);
			if (matchPaperID == true)
			{
				TempData["ErrorMessage"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}
			try
			{
				var deletePaperQue = _Context.PaperQueMasters.Where(x => x.FK_PaperMasterId == id).ToList();
				foreach (var item in deletePaperQue)
				{
					_Context.PaperQueMasters.Remove(item);
				}
				_Context.SaveChanges();
			}
			catch { }

			var deletePaper = _Context.PaperMasters.Where(x => (x.IsActive == true) && (x.PK_PaperMasterId == id)).SingleOrDefault();
			deletePaper.IsActive = false;
			_Context.SaveChanges();
			return RedirectToAction("PaperList");
		}

		public ActionResult Error()
		{
			TempData["errMsg"] = TempData["ErrorMessage"] as string;
			return View();
		}
	}
}