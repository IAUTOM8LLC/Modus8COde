using IAutoM8.Controllers.Abstract;
using IAutoM8.Domain.Models.Vendor;
using IAutoM8.Global.Options;
using IAutoM8.Service.Company.Dto;
using IAutoM8.Service.Company.Interfaces;
using IAutoM8.Service.SendGrid.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IAutoM8.Controllers.api
{
    [Authorize]
    public class CompanyController : BaseController
    {
        private readonly ICompanyService _companyService;
        private readonly ISendGridService _sendGridService;
        private IHostingEnvironment _hostingEnvironment;
        private readonly EmailTemplates _emailTemplates;
        public CompanyController(ICompanyService companyService, ISendGridService sendGridService,
            IHostingEnvironment hostingEnvironment, IOptions<EmailTemplates> emailTemplates)
        {
            _companyService = companyService;
            _sendGridService = sendGridService;
            _hostingEnvironment = hostingEnvironment;
            _emailTemplates = emailTemplates.Value;

        }

        [HttpGet]
        [Route("company-performance-data")]
        public async Task<IActionResult> GetPerformanceDataForCompany()
        {
            var result = await _companyService.GetPerformanceDataForCompany(UserGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("company-user-details")]
        public async Task<IActionResult> GetCompanyUserDetails()
        {
            var result = await _companyService.GetCompanyUserDetails(UserGuid);

            List<CompanyUserData> companyUserData = new List<CompanyUserData>();

            //Getting Unique Company Wroker Id's
            var companyWorkersList = result.GroupBy(x => x.Id).Select(grp => grp.ToList()).ToList();

            if (companyWorkersList.Count == 0)
            {
               // companyUserData.Add(new CompanyUserData { FullName = string.Empty, Email = string.Empty, Profile = string.Empty, Role = string.Empty });
            }

            foreach (var list in companyWorkersList)
            {
                CompanyUserData companyUser = new CompanyUserData()
                {
                    Id = list.FirstOrDefault().Id,
                    FullName = list.FirstOrDefault().FullName,
                    Email = list.FirstOrDefault().Email,
                    Profile = string.Empty,
                    Role = list.FirstOrDefault().Role,
                    EmailConfirmed = list.FirstOrDefault().EmailConfirmed
                };


                //Getting Unique Formula Task Id's here
                var taskData = list.GroupBy(x => x.FormulaTaskId).Select(grp => grp.ToList()).ToList();

                #region Preparing User Task Data
                List<UsertaskData> usertaskData = new List<UsertaskData>();
                foreach (var task in taskData)
                {

                    if (task.FirstOrDefault().FormulaTaskId > 0)
                    {

                        UsertaskData usertask = new UsertaskData();
                        usertask.FormulaTaskId = task.FirstOrDefault().FormulaTaskId;
                        usertask.Formula = task.FirstOrDefault().Formula;
                        usertask.Team = task.FirstOrDefault().Team;
                        usertask.Skill = task.FirstOrDefault().Skill;
                        usertask.Task = task.FirstOrDefault().Task;
                        usertask.Price = task.FirstOrDefault().Price;
                        usertask.Reviews = task.FirstOrDefault().Reviews;
                        usertask.Rating = task.FirstOrDefault().Rating;
                        usertaskData.Add(usertask);
                    }
                }
                companyUser.UsertaskData = usertaskData;
                #endregion

                companyUserData.Add(companyUser);
            }
            return Ok(companyUserData);
        }

        //if (companyWorkersList.Count == 0)
        //{
        //    companyUserData.Add(new CompanyUserData { FullName = string.Empty, Email = string.Empty, Profile = string.Empty, Role = string.Empty });
        //}

        //foreach (var list in companyWorkersList)
        //{
        //    CompanyUserData companyUser = new CompanyUserData()
        //    {
        //        Id = list.FirstOrDefault().Id,
        //        FullName = list.FirstOrDefault().FullName,
        //        Email = list.FirstOrDefault().Email,
        //        Profile = string.Empty,
        //        Role = list.FirstOrDefault().Role
        //    };


        //    //Getting Unique Formula Task Id's here
        //    var taskData = list.GroupBy(x => x.FormulaTaskId).Select(grp => grp.ToList()).ToList();
        //    #region Preparing User Task Data
        //    List<UsertaskData> usertaskData = new List<UsertaskData>();
        //    foreach (var task in taskData)
        //    {

        //        if (task.FirstOrDefault().FormulaTaskId > 0)
        //        {

        //            UsertaskData usertask = new UsertaskData();
        //            usertask.FormulaTaskId = task.FirstOrDefault().FormulaTaskId;
        //            usertask.Formula = task.FirstOrDefault().Formula;
        //            usertask.Team = task.FirstOrDefault().Team;
        //            usertask.Skill = task.FirstOrDefault().Skill;
        //            usertask.Task = task.FirstOrDefault().Task;
        //            usertask.Price = task.FirstOrDefault().Price;
        //            usertask.Reviews = task.FirstOrDefault().Reviews;
        //            usertask.Rating = task.FirstOrDefault().Rating;
        //        }
        //    }
        //    companyUser.UsertaskData = ;
        //    #endregion

        //    companyUserData.Add(companyUser);

        //}
        //return Ok(companyUserData);

        [HttpGet]
        [Route("company-user-price/{formulaTaskId:int}")]
        public async Task<IActionResult> GetCompanyUserPrice(int formulaTaskId)
        {
            var result = await _companyService.GetCompanyUserPrice(UserGuid, formulaTaskId);
            return Ok(result);
        }

        [HttpPut]
        [Route("modify-price/{formulaTaskId:int}")]
        public async Task<IActionResult> ModifyFormulaTaskPrice(int formulaTaskId, [FromBody] List<FormulaTaskVendor> formulaTaskVendor)
        {
            await _companyService.UpdatePriceForFormulaTask(formulaTaskId, formulaTaskVendor);
            return Ok();
        }

        [HttpGet]
        [Route("company-formula-bids")]
        public async Task<IActionResult> GetCompanyFormulaBids()
        {
            var result = await _companyService.GetCompanyTaskBids(UserGuid);
            return Ok(result);
        }

        [HttpGet]
        [Route("company-worker-email")]
        public async Task<IActionResult> InviteCompanyWorkerEmail(string email, string CompanyName, string userId)
        {
            email = email.Replace(@"/", "").Trim();
            CompanyName = CompanyName.Replace(@"/", "").Trim();
            string CompanyWorkerName = CompanyName;
            CompanyName = CompanyName.Replace(" ", "%20");
            string domainName = $"{this.Request.Scheme}://{this.Request.Host}{this.Request.PathBase}";

            //string emailUrl = domainName + "/cvendor-signup?email=" + email;
            //Added New Code for Showing CompanyName..
            //string emailUrl = domainName + "/cvendor-signup?email=" + email + "&companyname=" + CompanyName;//Added below line of code
            string emailUrl = domainName + "/cvendor-signup?email=" + email + "&companyname=" + CompanyName +"&Id="+userId;          

            //await _sendGridService.SendMessage(email,
            //     $"Click here to Sign Up " + emailUrl, "Invitation Link for Company Worker Sign Up");
            //Added Below Invitation Mail Code as per client requirement on 19-04-2021
            await _sendGridService.SendMessage(email, _emailTemplates.CompanyWorkerInvitation, $"Invitation from {CompanyWorkerName} team",
               new Dictionary<string, string> { { "{{CompanyName}}", CompanyWorkerName },
                    {"{{CompanyWorkerSignupURL}}", emailUrl } });

            return Ok();
        }

        [HttpDelete]
        [Route("companyuser-delete-price/{userId}/{formulaTaskId:int}")]
        public async Task<IActionResult> CompanyUserDeletePriceForFormulaTask(Guid userId, int formulaTaskId)
        {
            await _companyService.CompanyUserDeletePriceForFormulaTask(userId, formulaTaskId);
            return Ok();
        }

        [HttpDelete]
        [Route("companyperformance-delete-price/{userId}/{formulaTaskId:int}")]
        public async Task<IActionResult> CompanyPerformanceDeletePriceForFormulaTask(Guid userId, int formulaTaskId)
        {
            await _companyService.CompanyPerformanceDeletePriceForFormulaTask(UserGuid, userId, formulaTaskId);
            return Ok();
        }
    }
}
