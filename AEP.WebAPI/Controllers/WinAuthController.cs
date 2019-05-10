using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AEP.WebAPI.Models;
using System.Threading.Tasks;
using System.Web.Http.Cors;
using System.IO;
using AEP.DataModel;
using AEP.BusinessServices;

namespace AEP.WebAPI.Controllers
{

    [Authorize]
    [RoutePrefix("auth")]
    public class WinAuthController : ApiController
    {
        [HttpGet]
        [Route("getADNameList")]
        public async Task<IHttpActionResult> GetADNameList(string SamAccountName)
        {
            //Debug.Write($"AuthenticationType: {User.Identity.AuthenticationType}");
            //Debug.Write($"IsAuthenticated: {User.Identity.IsAuthenticated}");
            //Debug.Write($"Name: {User.Identity.Name}");


            SamAccountName = SamAccountName + "*";
            List<UserADInfo> lstPrincipal = UserADManager.GetAccounts(SamAccountName);
            return Ok(lstPrincipal);
        }
        [HttpGet]
        [Route("login")]
        public async Task<IHttpActionResult> Authenticate()
        {
            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    AppUser myAppUser = new AppUser();
                    myAppUser.DomainName = ADUserFactory.GetUserName();
                    myAppUser.Role = "Users";
                    myAppUser.MyColor = ADUserFactory.GetMyColor();

                    if (ADUserFactory.IsAdmin())
                    {
                        myAppUser.Role = "Admin";
                        return Ok(myAppUser);
                    }
                    else if (ADUserFactory.IsAEPUsers())
                    {
                        return Ok(myAppUser);
                    }
                    else
                    {
                        return BadRequest(myAppUser.DomainName);
                    }

                    //return new AppUser { DomainName = "domain\\yourusername", Role = "Admin" };
                }
                else
                {
                    return BadRequest("Not Authorized");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
       
        
        [HttpGet]
        [Route("saveColor")]
        public async Task<IHttpActionResult> SaveColor(string color)
        {
            try
            {
                ADUserFactory.SaveMyColor(color);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
