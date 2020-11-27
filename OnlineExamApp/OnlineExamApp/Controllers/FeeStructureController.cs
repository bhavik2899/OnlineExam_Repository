using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;

namespace OnlineExamApp.Controllers
{
	public class FeeStructureController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();
		// GET: FeeStructure
		public ActionResult FeeStructureList()
		{
			var fsmList = new List<FeeStructureModel>();
			var fsDeatils = _Context.FeeStructurs.Where(x => x.IsActive == true).Select(x => new { x.PK_FeeStructurId, x.StructurName, x.TotalFee, x.StartDate, x.EndDate }).ToList();
			foreach(var item in fsDeatils)
			{
				var fsm = new FeeStructureModel();
				fsm.FeeStructurId = item.PK_FeeStructurId;
				fsm.StructurName = item.StructurName;
				fsm.TotalFee = Convert.ToDecimal(item.TotalFee);
				if (item.StartDate.HasValue) { fsm.StartDate = item.StartDate.Value.ToShortDateString().ToString(); }
				if (item.EndDate.HasValue) { fsm.EndDate = item.EndDate.Value.ToShortDateString().ToString(); }
				fsmList.Add(fsm);
			}
			return View(fsmList);
		}

		public ActionResult AddEditFeeStructure(int id = 0)
		{
			var fsm = new FeeStructureModel();
			var feeHeadClassList = new List<FeeHeadClass>();

			var matchFeeStructureId = _Context.StudentFeeCollections.Any(x => x.FK_FeeStructurId == id && x.FeeCancellationDate == null);
			if (matchFeeStructureId == true)
			{
				TempData["errMsg"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}

			var matchFeeStructureIdInFeeSchedule = _Context.StudentFeeSchedules.Any(x => x.FK_FeeStructurId == id);
			if (matchFeeStructureIdInFeeSchedule == true)
			{
				TempData["errMsg"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}

			var feeStructureDetails = _Context.Sp_GetWholeFeeStructureByPK(id).ToList();
			var feeeStructureId = feeStructureDetails.Select(x => x.PK_FeeStructurId).FirstOrDefault();
			var cnt = 1;
			foreach(var item in feeStructureDetails)
			{
				var feeHeadClass = new FeeHeadClass();
				feeHeadClass.FeeHeadId = item.PK_FeeHeadId;
				feeHeadClass.HeadName = item.HeadName;
				feeHeadClass.IsOptional = item.IsOptional;
				if(feeeStructureId != null)
				{
					feeHeadClass.Amount = Convert.ToDecimal(item.Amount);
					if(item.Amount > 0) { feeHeadClass.IsInclude = true; }
					if(cnt <= 1)
					{
						fsm.FeeStructurId = Convert.ToInt64(item.PK_FeeStructurId);
						fsm.StructurName = item.StructurName;
						fsm.TotalFee = Convert.ToDecimal(item.TotalFee);
						if (item.StartDate.HasValue) { fsm.StartDate = item.StartDate.Value.ToShortDateString().ToString(); }
						if (item.EndDate.HasValue) { fsm.EndDate = item.EndDate.Value.ToShortDateString().ToString(); }
						cnt++;
					}
				}
				feeHeadClassList.Add(feeHeadClass);
			}
			fsm.feeheads = feeHeadClassList;
			
			return View(fsm);
		}

		[HttpPost]
		public ActionResult AddEditFeeStructure(FeeStructureModel fsm)
		{
			XDocument feeHeads = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
			 new XElement("FeeHeadList",
				 from HList in fsm.feeheads
				 where HList.IsInclude == true
				 select new XElement("FeeHead",
				 new XElement("FeeHeadId", HList.FeeHeadId),
				 new XElement("Amount", HList.Amount),
			  	new XElement("IsOptional", HList.IsOptional)))
			 );
			var startDate = Convert.ToDateTime(fsm.StartDate);
			var endDate = Convert.ToDateTime(fsm.EndDate);
			_Context.Sp_InsertUpdateFeeStructure(fsm.FeeStructurId, fsm.StructurName, startDate, endDate, feeHeads.ToString());
			return RedirectToAction("FeeStructureList");
		}

		public ActionResult DeleteFeeStructure(int id = 0)
		{
			var matchFeeStructureId = _Context.StudentFeeCollections.Any(x => x.FK_FeeStructurId == id && x.FeeCancellationDate == null);
			if (matchFeeStructureId == true)
			{
				TempData["errMsg"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}

			var matchFeeStructureIdInFeeSchedule = _Context.StudentFeeSchedules.Any(x => x.FK_FeeStructurId == id);
			if (matchFeeStructureIdInFeeSchedule == true)
			{
				TempData["errMsg"] = "You can not perform this operation.";
				return RedirectToAction("Error");
			}

			try
			{
				var deleteFeeStructureHead = _Context.FeeStructurHeads.Where(x => x.FK_FeeStructurId == id).ToList();
				foreach (var item in deleteFeeStructureHead)
				{
					_Context.FeeStructurHeads.Remove(item);
				}
				_Context.SaveChanges();
			}
			catch { }

			var deleteFeeStructure = _Context.FeeStructurs.Where(x => x.PK_FeeStructurId == id).SingleOrDefault();
			deleteFeeStructure.IsActive = false;
			_Context.SaveChanges();
			return RedirectToAction("FeeStructureList");
		}

		public ActionResult Error()
		{
			TempData["errMsg"] = TempData["ErrorMessage"] as string;
			return View();
		}
	}
}