using Newtonsoft.Json;
using OnlineExamApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineExamApp.Controllers
{
    public class ResultController : Controller
    {
        OnlineExamEntities _Context = new OnlineExamEntities();

        // GET: Result
        public ActionResult ResultReport()
        {
            var rm = new ResultModel();
            return View(rm);
        }
        [HttpPost]
        public ActionResult ResultReport(ResultModel rm)
        {
            rm.ResultReport = _Context.sp_ResultReport(rm.PaperId).ToList();
            return View(rm);
        }

        public ActionResult TestSolution(int id = 0)
        {
            var tsm = new TestSolutionModel();

            tsm.TestList = _Context.sp_PaperListByDate(null).Select(c => new SelectListItem
            {
                Value = c.PK_PaperMasterId.ToString(),
                Text = c.Title
            });
            tsm.PaperId = id;

            try {
                string DATA = _Context.sp_TestSolution(id).Last();

                tsm.Questions = JsonConvert.DeserializeObject<List<QuewithOptionModel>>(DATA);
            }
            catch { }

            return View(tsm);
        }

        public ActionResult StudentSolution(int id = 8)
        {
            var stm = new StudentSolutionModel();

            stm.TestList = _Context.sp_PaperListByDate(null).Select(c => new SelectListItem
            {
                Value = c.PK_PaperMasterId.ToString(),
                Text = c.Title
            });
            stm.PaperId = id;

            string DATA = _Context.sp_StudentTestSolution(id).Last();

            stm.Students = JsonConvert.DeserializeObject<List<SingleStudentSolutionModel>>(DATA);

            return View(stm);
        }

        public ActionResult StudentSolution2(int id = 0)
        {
            var stm = new StudentSolutionModel();

            stm.TestList = _Context.sp_PaperListByDate(null).Select(c => new SelectListItem
            {
                Value = c.PK_PaperMasterId.ToString(),
                Text = c.Title
            });
            stm.PaperId = id;

            var stdSolutionData = _Context.sp_StudentTestSolution2(id).ToList();

            var stdList = new List<SingleStudentSolutionModel>();
            var groupStudent = stdSolutionData.GroupBy(m => m.UserName).ToList();

            foreach(var item in groupStudent)
            {
                var std = new SingleStudentSolutionModel();
                std.UserName = item.Select(x => x.UserName).FirstOrDefault();
                std.TotalMarks = item.Select(x => x.TotalMarks).FirstOrDefault();
                std.PassingMarks = item.Select(x => x.PassingMarks).FirstOrDefault();
                std.ResultMarks = Convert.ToInt32(item.Select(x => x.ResultMarks).FirstOrDefault());
                std.Result = item.Select(x => x.Result).FirstOrDefault();

                var queList = new List<QuewithOptionModel>();
                var groupQuestion = item.GroupBy(m => m.PK_QueMasterId).ToList();
                foreach(var item1 in groupQuestion)
                {
                    var que = new QuewithOptionModel();
                    que.PK_QueMasterId = item1.Select(m => m.PK_QueMasterId).FirstOrDefault();
                    que.Title = item1.Select(m => m.Title).FirstOrDefault();
                    que.IsMultichoice = item1.Select(m => m.IsMultichoice).FirstOrDefault();

                    var opList = new List<OptionOFQuestionModel>();
                    var groupOptions = item1.GroupBy(m => m.PK_QueOptionMasterId).ToList();
                    foreach(var item2 in groupOptions)
                    {
                        var op = new OptionOFQuestionModel();
                        op.PK_QueOptionMasterId = item2.Select(m => m.PK_QueOptionMasterId).FirstOrDefault();
                        op.IsSelected = Convert.ToBoolean(item2.Select(m => m.IsSelected).FirstOrDefault());
                        op.IsCorrect = item2.Select(m => m.IsCorrect).FirstOrDefault();
                        op.OptionTitle = item2.Select(m => m.OptionTitle).FirstOrDefault();
                        opList.Add(op);
                    }
                    que.qom = opList;
                    queList.Add(que);
                }
                std.Questions = queList;
                stdList.Add(std);
            }
            stm.Students = stdList;
            return View(stm);
        }

        public ActionResult GetPaperByDate(DateTime testDate)
        {
            return Json(_Context.sp_PaperListByDate(testDate).Select(x => new
            {
                PaperId = x.PK_PaperMasterId,
                PaperName = x.Title
            }).ToList(), JsonRequestBehavior.AllowGet);
        }
    }
}

