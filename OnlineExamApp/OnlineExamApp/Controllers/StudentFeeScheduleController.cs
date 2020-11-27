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
	public class StudentFeeScheduleController : Controller
	{
		OnlineExamEntities _Context = new OnlineExamEntities();
		// GET: StudentFeeSchedule
		public ActionResult StudentFeeSchedule(int id = 0)
		{
			var sfsmodel = new StudentFeeScheduleModel();
			var sbfsmodelList = new List<StudentsByFeeStructureModel>();

			sfsmodel.FeeStructureList = _Context.FeeStructurs.Where(x => x.IsActive == true).Select(x => new SelectListItem
			{
				Value = x.PK_FeeStructurId.ToString(),
				Text = x.StructurName
			});

			sfsmodel.FeeStructurId = id;
			sfsmodel.TotalFee = Convert.ToDecimal(_Context.FeeStructurs.Where(x => x.PK_FeeStructurId == id && x.IsActive == true).Select(x => x.TotalFee).FirstOrDefault());
			var details = _Context.Sp_GetStudentFeeScheduleByFeeStructureID(id).ToList();
			var groupByStudent = details.GroupBy(x => x.PK_StudentId).ToList();

			foreach (var item in groupByStudent)
			{
				var sbfsmodel = new StudentsByFeeStructureModel();
				sbfsmodel.StudentID = item.Select(x => x.PK_StudentId).FirstOrDefault();
				sbfsmodel.Name = item.Select(x => x.Name).FirstOrDefault();
				sbfsmodel.StudentFeeScheduleId = item.Select(x => x.PK_StudentFeeScheduleId).FirstOrDefault();
				sbfsmodel.DiscountAmountPercentage = item.Select(x => x.DiscountAmountPercentage).FirstOrDefault();
				sbfsmodel.TotalPayableAmount = item.Select(x => x.TotalPayableAmount).FirstOrDefault();
				sbfsmodel.UnPaidFee = Convert.ToBoolean(item.Select(x => x.UnPaid).FirstOrDefault());
				if(item.Select(x => x.FK_FeeStructurId).FirstOrDefault() != null) { sbfsmodel.IsInclude = true; }

				sbfsmodelList.Add(sbfsmodel);
			}
			sfsmodel.StudentsByFeeStructure = sbfsmodelList;
			return View(sfsmodel);
		}

		[HttpPost]
		public ActionResult StudentFeeSchedule(StudentFeeScheduleModel sfsmodel)
		{
			var scheduleDate = DateTime.Now;
			XDocument studentFeeSchedules = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
				new XElement("StudentFeeScheduleList",
					from sfsList in sfsmodel.StudentsByFeeStructure
					where sfsList.UnPaidFee == false
					select new XElement("StudentFeeSchedule",
					new XElement("FeeStructurId", sfsmodel.FeeStructurId),					
					new XElement("scheduleDate", scheduleDate),
					new XElement("DiscountAmountPercentage", sfsList.DiscountAmountPercentage),
					new XElement("TotalPayableAmount", sfsList.TotalPayableAmount),
					new XElement("StudentID", sfsList.StudentID),
					new XElement("IsInclude", sfsList.IsInclude)
			)));

			_Context.Sp_InsertUpdateStudentFeeSchedule(studentFeeSchedules.ToString());

			return RedirectToAction("StudentFeeSchedule", new RouteValueDictionary(
				new { controller = "StudentFeeSchedule", action = "StudentFeeSchedule", id = 0 }));
		}

		public ActionResult StudentFeeCollection(int id = 0)
		{
			var sfcmodel = new StudentFeeCollectionModel();
			var feeScheduleList = new List<FeeScheduleModel>();

			sfcmodel.StudentID = id;
			sfcmodel.StudentList = _Context.Students.Where(x => x.IsActive == true).Select(x => new SelectListItem
			{
				Value = x.PK_StudentId.ToString(),
				Text = x.Name
			});

			var sfcDetails = _Context.Sp_GetStudentFeeCollectionByStudent(id);

			var groupByFeeStructure = sfcDetails.GroupBy(x => x.PK_FeeStructurId).ToList();
			foreach(var item in groupByFeeStructure)
			{
				var feeSchedule = new FeeScheduleModel();

				feeSchedule.FeeStructureId = item.Select(x => x.PK_FeeStructurId).FirstOrDefault();
				feeSchedule.StructurName = item.Select(x => x.StructurName).FirstOrDefault();
				feeSchedule.StudentFeeScheduleId = item.Select(x => x.PK_StudentFeeScheduleId).FirstOrDefault();
				feeSchedule.TotalFeeAmount = Convert.ToDecimal(item.Select(x => x.TotalFeeAmount).FirstOrDefault());
				feeSchedule.TotalPayableAmount = Convert.ToDecimal(item.Select(x => x.TotalPayableAmount).FirstOrDefault());
				
				feeScheduleList.Add(feeSchedule);
			}
			sfcmodel.FeeScheduleModelList = feeScheduleList;

			return View(sfcmodel);
		}
		
		public ActionResult StudentFeeCollectionPartial(int id = 0 , int feeStructureId = 0)
		{
			var sfcmodel = new StudentFeeCollectionModel();
			var feeScheduleList = new List<FeeScheduleModel>();

			sfcmodel.StudentID = id;

			var sfcDetails = _Context.Sp_GetStudentFeeCollectionByFeeStructure(id, feeStructureId);

			var groupByFeeStructure = sfcDetails.GroupBy(x => x.PK_FeeStructurId).ToList();
			foreach (var item in groupByFeeStructure)
			{
				var feeSchedule = new FeeScheduleModel();
				var feeHeadModelList = new List<FeeHeadModel>();

				feeSchedule.FeeStructureId = item.Select(x => x.PK_FeeStructurId).FirstOrDefault();
				feeSchedule.StructurName = item.Select(x => x.StructurName).FirstOrDefault();
				feeSchedule.StudentFeeScheduleId = item.Select(x => x.PK_StudentFeeScheduleId).FirstOrDefault();
				feeSchedule.TotalFeeAmount = Convert.ToDecimal(item.Select(x => x.TotalFeeAmount).FirstOrDefault());
				feeSchedule.PaidAmount = item.Select(x => x.PaidAmount).FirstOrDefault();
				feeSchedule.TotalPayableAmount = Convert.ToDecimal(item.Select(x => x.TotalPayableAmount).FirstOrDefault());

				var groupByFeeHead = item.GroupBy(x => x.PK_FeeHeadId).ToList();
				foreach (var item1 in groupByFeeHead)
				{
					var feeHeadModel = new FeeHeadModel();

					feeHeadModel.FeeHeadId = item1.Select(x => x.PK_FeeHeadId).FirstOrDefault();
					feeHeadModel.IsOptional = item1.Select(x => x.IsOptional).FirstOrDefault();
					feeHeadModel.HeadName = item1.Select(x => x.HeadName).FirstOrDefault();
					feeHeadModel.AmountByHead = Convert.ToDecimal(item1.Select(x => x.AmountByHead).FirstOrDefault());
					
					var paidHeadAmounts = item1.Select(x => x.PaidHeadAmount).ToList();
					decimal totalPaidHeadAmount = 0;
					foreach(var item2 in paidHeadAmounts)
					{
						totalPaidHeadAmount = totalPaidHeadAmount + Convert.ToDecimal(item2);
					}
					feeHeadModel.PaidHeadAmount = totalPaidHeadAmount;

					feeHeadModelList.Add(feeHeadModel);
				}
				feeSchedule.feeHeadModelList = feeHeadModelList;
				feeScheduleList.Add(feeSchedule);
			}
			sfcmodel.FeeScheduleModelList = feeScheduleList;

			return PartialView(sfcmodel);
		}

		[HttpPost]
		public ActionResult StudentFeeCollectionPartial(StudentFeeCollectionModel sfcmodel)
		{
			var feeStructureId = sfcmodel.FeeScheduleModelList[0].FeeStructureId;
			var studentFeeScheduleId = sfcmodel.FeeScheduleModelList[0].StudentFeeScheduleId;
			var paidDate = DateTime.Now;

			XDocument studentFeeCollection = new XDocument(new XDeclaration("1.0", "UTF - 8", "yes"),
				new XElement("StudentFeeCollectionList",
					from sfcList in sfcmodel.FeeScheduleModelList
					select new XElement("StudentFeeCollectionByHeadList",
					from sfchList in sfcList.feeHeadModelList					
					select new XElement("StudentFeeCollectionByHead",
					new XElement("FeeHeadId", sfchList.FeeHeadId),
					new XElement("IsOptional", sfchList.IsOptional),
					new XElement("PayHeadAmount", sfchList.PayableHeadAmount)
			))));

			_Context.Sp_InsertUpdateStudentFeeCollection(sfcmodel.StudentID, feeStructureId, studentFeeScheduleId, paidDate, studentFeeCollection.ToString());
			return RedirectToAction("StudentFeeCollection");
		}
	}
}
