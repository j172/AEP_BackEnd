using AEP.BusinessServices;
using AEP.DataModel;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AEP.WebAPI.Controllers
{
    [Authorize]
     public class UploadController : ApiController
    {
        [Route("api/AEPFiles/uploadFile")]
        [HttpPost]
        public IHttpActionResult UploadFiles()
        {
            int i = 0;
            int cntSuccess = 0;
            var uploadedFileNames = new List<string>();
            string result = string.Empty;

            HttpResponseMessage response = new HttpResponseMessage();

            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {
                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[i];
                    var filePath = HttpContext.Current.Server.MapPath("~/UploadedFiles/" + postedFile.FileName);
                    try
                    {
                        postedFile.SaveAs(filePath);
                        uploadedFileNames.Add(httpRequest.Files[i].FileName);
                        cntSuccess++;
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }

                    i++;
                }
            }

            result = cntSuccess.ToString() + " files uploaded succesfully.<br/>";

            result += "<ul>";

            foreach (var f in uploadedFileNames)
            {
                result += "<li>" + f + "</li>";
            }

            result += "</ul>";

            return Json(result);
        }


        private bool ByteArrayToFile(string fileName, byte[] byteArray)
        {
            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    fs.Write(byteArray, 0, byteArray.Length);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception caught in process: {0}", ex);
                return false;
            }
        }

        [HttpPost]
        [Route("api/AEPFiles/UploadXls")]
        public async Task<IHttpActionResult> UploadJsonFile()
        {
            try
            {
                bool b = Request.Content.IsMimeMultipartContent();

                if (!b)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    var httpRequest = HttpContext.Current.Request;
                    if (httpRequest.Files.Count > 0)
                    {

                    }
                }

                var result = await Request.Content.ReadAsMultipartAsync();

                var requestJson0 = await result.Contents[0].ReadAsStringAsync();
                AEPFolderAttr MyFolderAttr = JsonConvert.DeserializeObject<AEPFolderAttr>(requestJson0);
                var requestJson1 = await result.Contents[1].ReadAsStringAsync();
                AEPFile Myfile = JsonConvert.DeserializeObject<AEPFile>(requestJson1);
                var requestJson = await result.Contents[2].ReadAsStringAsync();
                var comment = await result.Contents[3].ReadAsStringAsync();
                var strchecked = await result.Contents[4].ReadAsStringAsync();



                var fileName = requestJson;
                string folder = MyFolderAttr.physicalPath;
                string fileFullName = Path.Combine(folder, fileName);
                if (result.Contents.Count > 1)
                {
                    var fileByteArray = await result.Contents[5].ReadAsByteArrayAsync();
                    ByteArrayToFile(fileFullName, fileByteArray);
                    if (Myfile.name != fileName)
                    {
                       // FileFactory.CreateNewFile(fileName, fileFullName, MyFolderAttr);
                        return BadRequest("FileName not equal");
                    }
                    else
                    {
                        FileFactory.CheckInFile(Myfile.id, comment, strchecked);
                    }

                }
                 
                    return Ok();
                
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpPost]
        [Route("api/AEPFiles/UploadXls2")]
        public async Task<IHttpActionResult> UploadJsonFiles()
        {
            try
            {
                bool b = Request.Content.IsMimeMultipartContent();

                if (!b)
                {
                    HttpResponseMessage response = new HttpResponseMessage();
                    var httpRequest = HttpContext.Current.Request;
                    if (httpRequest.Files.Count > 0)
                    {

                    }
                }

                var result = await Request.Content.ReadAsMultipartAsync();
                var requestJson0 = await result.Contents[0].ReadAsStringAsync();
                AEPFolderAttr MyFolderAttr = JsonConvert.DeserializeObject<AEPFolderAttr>(requestJson0);
                var requestJson1 = await result.Contents[1].ReadAsStringAsync();
                List<string> lstFileNames = JsonConvert.DeserializeObject<List<string>>(requestJson1);
                var comment = await result.Contents[2].ReadAsStringAsync();
                var strchecked = await result.Contents[3].ReadAsStringAsync();


                string folder = MyFolderAttr.physicalPath;

                int StartIndex = 4;

                List<AEPReturnUploadFile> lstAEPReturnUploadFile = new List<AEPReturnUploadFile>();

                for (int i = StartIndex; i < result.Contents.Count; i++)
                {

                    string FileName =  lstFileNames[i - StartIndex];
                    string fileFullName = Path.Combine(folder, FileName);

                   AEPReturnUploadFile myAEPReturnUploadFile = new AEPReturnUploadFile();
                   myAEPReturnUploadFile.FileName = FileName;
                   myAEPReturnUploadFile.IsOK = false;
                   lstAEPReturnUploadFile.Add(myAEPReturnUploadFile);
                   try
                   {
                       var fileByteArray = await result.Contents[i].ReadAsByteArrayAsync();
                       ByteArrayToFile(fileFullName, fileByteArray);
                       string DocID = FileFactory.CreateNewFile(FileName, fileFullName, MyFolderAttr, comment, strchecked);
                       myAEPReturnUploadFile.DocID = DocID;
                       myAEPReturnUploadFile.IsOK = true;

                   }
                   catch (Exception ex3)
                   {
                       myAEPReturnUploadFile.IsOK = false;
                       myAEPReturnUploadFile.message = ex3.Message;

                   }

                }

                return Ok(lstAEPReturnUploadFile);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


         [HttpGet]
         [Route("api/AEPFiles/DownloadFile")]
        public IHttpActionResult Generate(string DocID)
        {

            string UserName = ADUserFactory.GetUserName();
            try
            {
                FileFactory.SetDownloadLog(DocID);
                byte[] b = FileFactory.ReadFile(DocID);
                //return Ok(lstPayment);
                var result = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new ByteArrayContent(b)
                };
                //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                result.Content.Headers.ContentDisposition.FileName = string.Format("MMS_S202_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                result.Content.Headers.ContentDisposition.Size = b.Length;
                var response = ResponseMessage(result);

                return response;
               
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
                //throw;
            }

        }

         [HttpGet]
         [Route("api/AEPFiles/DownloadHistoryFile")]
         public IHttpActionResult DownloadHistoryFile(Int64 HistoryID)
         {

             string UserName = ADUserFactory.GetUserName();


             try
             {
                 FileFactory.SetDocHDownloadLog(HistoryID);
                 byte[] b = FileFactory.ReadFileHistory(HistoryID);
                 //return Ok(lstPayment);
                 var result = new HttpResponseMessage(HttpStatusCode.OK)
                 {
                     Content = new ByteArrayContent(b)
                 };
                 //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                 result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                 result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                 result.Content.Headers.ContentDisposition.FileName = string.Format("MMS_S202_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                 result.Content.Headers.ContentDisposition.Size = b.Length;
                 var response = ResponseMessage(result);

                 return response;
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
                 //throw;
             }

         }

         [HttpGet]
         [Route("api/AEPFiles/CheckOut")]
         public IHttpActionResult CheckOut(string DocID)
         {

             string UserName = ADUserFactory.GetUserName();
             FileFactory.CheckOutFile(DocID);
             try
             {
                 byte[] b = FileFactory.ReadFile(DocID);
                 //return Ok(lstPayment);
                 var result = new HttpResponseMessage(HttpStatusCode.OK)
                 {
                     Content = new ByteArrayContent(b)
                 };
                 //result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/vnd.ms-excel");
                 result.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                 result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                 result.Content.Headers.ContentDisposition.FileName = string.Format("MMS_S202_{0}.xlsx", DateTime.Now.ToString("yyyyMMddHHmmss"));
                 result.Content.Headers.ContentDisposition.Size = b.Length;
                 var response = ResponseMessage(result);

                 return response;
             }
             catch (Exception ex)
             {
                 return BadRequest(ex.Message);
                 //throw;
             }

         }
        
        [HttpGet]
        [Route("api/AEPFiles/CheckOutStatus")]
        public IHttpActionResult CheckOutStatus(string DocID)
        {
            try
            {
                string docStatus = FileFactory.CheckOutStatus(DocID);
                return Ok(docStatus);
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


    }
}