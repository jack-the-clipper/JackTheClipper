using System;
using System.Collections.Generic;
using JackTheClipperBusiness;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;

namespace JackTheClipperRequestHandler.Controllers
{
    /// <summary>
    /// Controller for basic services.
    /// </summary>
    [Route("clipper")]
    [ApiController]
    public class ClipperServiceController : ControllerBase
    {
        #region GetStatus
        /// <summary>
        /// Gets the status of the app.
        /// </summary>
        /// <returns>MethodResult with status.</returns>
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
        #endregion

        #region GetPrincipalUnits        
        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        [Route("principalunits")]
        [HttpGet]
        public ActionResult<IReadOnlyList<Tuple<string, Guid>>> GetPrincipalUnits()
        {
            try
            {
                var result = Factory.GetControllerInstance<IClipperService>().GetPrincipalUnits();
                return new ActionResult<IReadOnlyList<Tuple<string, Guid>>>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }
        #endregion
    }
}
