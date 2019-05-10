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
    [RoutePrefix("AEPFiles")]
    public class FilesController : ApiController
    {
         
        [HttpGet]
        [Route("dir")]
        public async Task<IHttpActionResult> GetDir()
        {
            List<AEPFolder> lstRetNode = FolderFactory.LoadHierarchyFolder();
            return Ok(lstRetNode);
        }
        [HttpGet]
        [Route("files")]
        public async Task<IHttpActionResult> GetFiles(Int64 FolderID)
        {
            List<AEPFile> lstNode = FileFactory.GetFiles(FolderID);
            return Ok(lstNode);
        }
        static void getDir(List<DirectoryInfo> lstDir, string Parent, AEPFolder MyNode, List<AEPFolder> lstNode)
        {
            var query = lstDir.Where(x => x.Parent.Name == Parent).ToList();
            if (query.Count == 0) return;
            foreach (var p in query)
            {
                AEPFolder MyChildrenNode = new AEPFolder();
                lstNode.Add(MyChildrenNode);
                //MyChildrenNode.id = lstNode.Count;
                //MyChildrenNode.name = p.Name;

                //MyChildrenNode.id = lstNode.Count;
                MyChildrenNode.label = p.Name;
                MyChildrenNode.data = new AEPFolderAttr
                {
                    physicalPath =  p.FullName
                };;
                MyChildrenNode.expandedIcon = "fa fa-folder-open";
                MyChildrenNode.collapsedIcon = "fa fa-folder";


                if (MyNode.children == null)
                {
                    MyNode.children = new List<AEPFolder>();
                }
                MyNode.children.Add(MyChildrenNode);

                Console.WriteLine(p.Name);
                getDir(lstDir, p.Name, MyChildrenNode, lstNode);
            }
        }
        //[HttpPost]
        //[Route("save")]
        //public async Task<IHttpActionResult> Save(PostData data)
        //{
        //    AppUser myAppUser = new AppUser();
        //    myAppUser.DomainName = ADUserFactory.GetUserName();
        //    return Ok(data);
        //}
        [HttpPost]
        [Route("createFolder")]
        public async Task<IHttpActionResult> createFolder(AEPCreateFolder data)
        {
            Int64 FolderID = FolderFactory.CreateFolder(data.name, data.parentID);
            AEPFolder myMyFolder = FolderFactory.GetMyFolder(FolderID);
             return Ok(myMyFolder);
        }
        [HttpGet]
        [Route("deleteFolder")]
        public async Task<IHttpActionResult> deleteFolder(Int64 FolderID)
        {
            FolderFactory.DeleteFolder(FolderID);
            return Ok();
        }
        [HttpGet]
        [Route("getFileInfo")]
        public async Task<IHttpActionResult> getFileInfo(string DocID)
        {
            AEPFile myFile = FileFactory.GetFileInfo(DocID);
            return Ok(myFile);
        }
        [HttpGet]
        [Route("deleteFile")]
        public async Task<IHttpActionResult> deleteFile(string DocID)
        {
            try
            {
                FileFactory.DeleteFile(DocID);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("getFileHistory")]
        public async Task<IHttpActionResult> getFileHistory(string DocID)
        {
            List<AEPFile> lstFileHistory = FileFactory.GetFileHistory(DocID);
            return Ok(lstFileHistory);
        }

        [HttpGet]
        [Route("setAlertMe")]
        public async Task<IHttpActionResult> setAlertMe(string DocID)
        {
            bool result = FileFactory.SetAlertMe(DocID);
            return Ok(result);
        }
        [HttpGet]
        [Route("setCancelAlertMe")]
        public async Task<IHttpActionResult> setCancelAlertMe(string DocID)
        {
            bool result = FileFactory.CancelAlertMe(DocID);
            return Ok(result);
        }
         [HttpGet]
         [Route("setDisCheckOutFile")]
        public async Task<IHttpActionResult> setDisCheckOutFile(string DocID)
        {
            try
            {
                FileFactory.DisCheckOutFile(DocID);
                return Ok();

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
         [HttpGet]
         [Route("getFolderAuth")]
         public async Task<IHttpActionResult> getFolderAuth(Int64 FolderID)
         {
             FolderAuthMainModel lstFolderAuth = FolderFactory.GetFolderAuthMain(FolderID, FolderID);//.GetFolderAuth(FolderID, FolderID);
             return Ok(lstFolderAuth);
         }

         [HttpPost]
         [Route("saveFolderAuth")]
         public async Task<IHttpActionResult> saveFolderAuth(FolderAuthMainModel lstFolderAuth)
         {
             FolderFactory.SaveFolderAuth(lstFolderAuth);
             return Ok(lstFolderAuth);
         }
         [HttpGet]
         [Route("getDownloadLog")]
         public async Task<IHttpActionResult> getDownloadLog()
         {
             try
             {
                 List<DownloadLogModel> lstLog = FileFactory.GetDownloadLog();
                 return Ok(lstLog);
             }
             catch (Exception ex)
             {
                 throw ex;
             }
         }
        
        [HttpGet]
        [Route("changeFolderOwner")]
        public async Task<IHttpActionResult> changeFolderOwner(Int64 FolderID, string OwnerAD)
        {
            FolderFactory.ChangeFolderOwner(FolderID, OwnerAD);
            return Ok(OwnerAD);
        }

        [HttpGet]
        [Route("modifyFolderName")]
        public async Task<IHttpActionResult> modifyFolderName(Int64 FolderID, string FolderName)
        {
            try
            {
                FolderFactory.ModifyFolderName(FolderID, FolderName);
                return Ok(FolderName);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        [Route("getMyCheckOutList")]
        public async Task<IHttpActionResult> getMyCheckOutList()
        {
            List<AEPFile> lstMyCheckOutList = FileFactory.GetMyCheckOutList();
            return Ok(lstMyCheckOutList);
        }

        [HttpGet]
        [Route("getMyAlertMeList")]
        public async Task<IHttpActionResult> getMyAlertMeList()
        {
            List<AEPFile> lstMyAlertMeList = FileFactory.GetMyAlertMeList();
            return Ok(lstMyAlertMeList);
        }

        [HttpGet]
        [Route("getKeywordSearch")]
        public async Task<IHttpActionResult> getKeywordSearch(string keyword)
        {
            try
            {
                List<AEPFile> lstKeywordFile = FileFactory.GetKeywordSearch(keyword);
                return Ok(lstKeywordFile);
            }
            catch (Exception e)
            {
                return BadRequest(e.InnerException.Message);
            }

        }

        [HttpGet]
        [Route("getDirByID")]
        public async Task<IHttpActionResult> getDirByID(Int64 FolderID)
        {
            AEPFolder myRetNode = FolderFactory.LoadHierarchyFolderByID(FolderID);
            return Ok(myRetNode);
        }

    }
}
