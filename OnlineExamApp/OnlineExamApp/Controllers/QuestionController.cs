using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Xml.Linq;

namespace OnlineExamApp.Controllers
{
	public class QuestionController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();

		// GET: Question
		public ActionResult QuestionList()
		{
			var quemodelList = new List<QuestionModel>();
			var questions = _Context.QueMasters.Include("QueOptionMaster").Where(x=>x.IsActive == true).Select(x => new { x.PK_QueMasterId, x.Title, x.QueOptionMasters.Count }).ToList();
			foreach (var que in questions)
			{
				var qm = new QuestionModel();
				qm.Title = que.Title;
				qm.QueId = que.PK_QueMasterId;
				qm.NoOfOption = que.Count;

				quemodelList.Add(qm);
			}
			return View(quemodelList);
		}

		public ActionResult AddEditQuestion(int id = 0)
		{
			var quemodel = new QuestionModel();
			if (id != 0)
			{

				var matchQuestionID = _Context.PaperQueMasters.Any(x=>x.FK_QueMasterId == id);
				if(matchQuestionID != true)
				{
					var question = _Context.QueMasters.Where(x => (x.IsActive == true) && (x.PK_QueMasterId == id)).FirstOrDefault();

					quemodel.QueId = question.PK_QueMasterId;
					quemodel.Title = question.Title;
					quemodel.Details = question.Details;
					quemodel.IsMultichoice = question.IsMultichoice;
					foreach (var item in question.QueOptionMasters)
					{
						var options = new OptionModel();
						options.Title = item.Title;
						options.OptionId = item.PK_QueOptionMasterId;
						options.Details = item.Details;
						options.IsCorrect = item.IsCorrect;
						options.OrderNo = item.OrderNo;
						quemodel.OptionList.Add(options);
					}
					return View(quemodel);
				}
				else
				{
					TempData["ErrorMessage"] = "This Question is Selected in Paper,so you can not perform this operation.";
					return RedirectToAction("Error");
				}
			}
			else
			{
				return View(quemodel);
			}
		}

		[HttpPost]
		public ActionResult AddEditQuestion(QuestionModel qm, FormCollection fm)
		{

			var optionList = new List<OptionModel>();
			var optionRowIdArr = qm.OptionRowID.Split(',').Select(int.Parse).ToArray();

			foreach (var item in optionRowIdArr)
			{
				var option = new OptionModel();
				option.Title = fm["Title_"+item];
				var no = fm["OrderNo_" + item];
				if(no != "") { option.OrderNo = Convert.ToInt32(no); }
				option.Details = fm["Details_" + item];
				if (qm.IsMultichoice == true)
				{
					var check = fm["IsCorrect_" + item];
					if(check != null )
					{
						if (check != "false")
						{
							option.IsCorrect = true;
						}
					}
				}
				else
				{
					var check = fm["IsCorrect_" + item];
					if (check != null)
					{
						if (check != "false")
						{
							option.IsCorrect = true;
						}
					}
				}
				optionList.Add(option);
			}

			XDocument Options = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
			 new XElement("OptionList",
				 from OpList in optionList
				 select new XElement("Option",
				 new XElement("QueId", qm.QueId),
				 new XElement("Title", OpList.Title),
				 new XElement("OrderNo", OpList.OrderNo),
				 new XElement("Details", OpList.Details),
				 new XElement("IsCorrect", OpList.IsCorrect)))
			 );

			_Context.sp_InsertUpdateQuestion(qm.QueId, qm.Title, qm.Details, qm.IsMultichoice, Options.ToString());

			return RedirectToAction("QuestionList");
		}

		public ActionResult DeleteQuestion(int id = 0)
		{

			var matchQuestionID = _Context.PaperQueMasters.Any(x => x.FK_QueMasterId == id);
			if (matchQuestionID != true)
			{
				try
				{
					var deleteOptions = _Context.QueOptionMasters.Where(x => x.FK_QueMasterId == id).ToList();
					foreach (var item in deleteOptions)
					{
						_Context.QueOptionMasters.Remove(item);
					}
					_Context.SaveChanges();
				}
				catch { }

				var deleteQue = _Context.QueMasters.Where(x => x.PK_QueMasterId == id).SingleOrDefault();
				deleteQue.IsActive = false;
				_Context.SaveChanges();
				return RedirectToAction("QuestionList");
			}
			else
			{
				TempData["ErrorMessage"] = "This Question is Selected in Paper,so you can not perform this operation.";
				return RedirectToAction("Error");
			}
		}

		public ActionResult Error()
		{
			TempData["errMsg"] = TempData["ErrorMessage"] as string;
			return View();
		}
	}
}