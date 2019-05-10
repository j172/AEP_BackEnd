using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using AEP.DataModel;
using AEP.BusinessServices;

namespace AEPWebAPI.Controllers
{
    [Authorize]
    [RoutePrefix("Group")]
    public class GroupController : ApiController
    {
        [HttpGet]
        [Route("Group")]
        public List<GroupModel> GetGroupList()
        {
            List<GroupModel> lstGroup = GroupFactory.GetGroupList();
            return lstGroup;
        }

        [HttpGet]
        [Route("GetGroupMember")]
        public List<GroupUserModel> GetGroupMember(Int64 GroupID)
        {
            List<GroupUserModel> lstGroupUser = GroupFactory.GetGroupMember(GroupID);
            return lstGroupUser;
        }

        [HttpGet]
        [Route("GroupWithName")]
        public List<GroupModel> GetGroupList(string GroupName)
        {
            List<GroupModel> lstGroup = GroupFactory.GetGroupList(GroupName);
            return lstGroup;
        }

        [HttpGet]
        [Route("CreateGroup")]
        public long CreateGroup(string GroupName)
        {
            if (GroupName == "null")
                return -1;
            bool result = GroupFactory.IsOccupiedGroupName(GroupName);
            if (result)
            {
                return GroupFactory.CreateGroup(GroupName);
            }
            else
            {
                return 0;
            }
        }

        [HttpGet]
        [Route("DeleteGroup")]
        public IHttpActionResult DeleteGroup(long GroupID)
        {
            try
            {
                GroupFactory.DeleteGroup(GroupID);
                return Ok("OK");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpGet]
        [Route("AddGroupUser")]
        public IHttpActionResult AddGroupUser(long GroupID, string UserAD)
        {
            try
            {
                string result = GroupFactory.AddGroupUser(GroupID, UserAD);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpGet]
        [Route("DelGroupUser")]
        public IHttpActionResult DelGroupUser(long GroupID, string UserAD)
        {
            try
            {
                string result = GroupFactory.DelGroupUser(GroupID, UserAD);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("SaveGroup")]
        public IHttpActionResult SaveGroup(GroupModel myGroup)
        {
            try
            {
                GroupFactory.SaveGroup(myGroup);
                return Ok("OK");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("Upload")]
        public IHttpActionResult UploadMemberList(List<GroupUserModel> lstGroupUser)
        {
            try
            {
                //GroupFactory.GroupJoinMember(GroupID, lstGroupUser);
                string result = GroupFactory.UploadMemberList(lstGroupUser);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [Route("CheckADListIsExist")]
        public IHttpActionResult CheckADListIsExist(List<string> lstUser)
        {
            try
            {
                List<string> myValidUser = GroupFactory.CheckADListIsExist(lstUser);
                return Ok(myValidUser);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

    }
}