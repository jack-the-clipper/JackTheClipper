using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JackTheClipperBusiness;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;

namespace JackTheClipperRequestHandler.Controllers
{
    [Route("clipper")]
    [ApiController]
    public class ClipperStatusController : ControllerBase
    {
        [Route("status")]
        [HttpGet]
        public ActionResult<MethodResult> GetStatus()
        {
            try
            {
                return Factory.GetControllerInstance<IClipperService>().GetStatus();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }
    }
}
