using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using AEP.WebAPI.Models;
using System.Web.Http.Cors;

namespace AEPWebAPI.Controllers
{
     [Authorize]
    [RoutePrefix("data")]
    public class DataController : ApiController
    {
        [HttpPost]
        [Route("save")]
        public IHttpActionResult Save(PostData data)
        {
            return Ok(data.ToString());
        }
    }
}
