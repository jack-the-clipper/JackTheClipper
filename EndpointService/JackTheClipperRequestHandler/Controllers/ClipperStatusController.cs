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

        #region GetPrincipalUnitBasicInformation        
        /// <summary>
        /// Gets the principal units.
        /// </summary>
        /// <returns>List of principal units.</returns>
        [Route("getprincipalunitsbasic")]
        [HttpGet]
        public ActionResult<IReadOnlyList<Tuple<string, Guid>>> GetPrincipalUnitBasicInformation()
        {
            try
            {
                var result = Factory.GetControllerInstance<IClipperService>().GetPrincipalUnitBasicInformation();
                return new ActionResult<IReadOnlyList<Tuple<string, Guid>>>(result);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return BadRequest(e.Message);
            }
        }
        #endregion

        #region GetPrincipalUnitChildren               
        /// <summary>
        /// Gets the children of a principal unit.
        /// </summary>
        /// <param name="principalUnitId">The principal unit identifier.</param>
        /// <returns>List of children of principal units.</returns>
        [Route("getprincipalunitchildren")]
        [HttpGet]
        public ActionResult<IReadOnlyList<Tuple<string, Guid>>> GetPrincipalUnitChildren([FromQuery]Guid principalUnitId)
        {
            try
            {
                var result = Factory.GetControllerInstance<IClipperService>().GetPrincipalUnitChildren(principalUnitId);
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
