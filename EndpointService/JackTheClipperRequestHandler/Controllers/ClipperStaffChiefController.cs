using System;
using System.Collections.Generic;
using JackTheClipperBusiness;
using JackTheClipperCommon.Enums;
using JackTheClipperCommon.Interfaces;
using JackTheClipperCommon.SharedClasses;
using Microsoft.AspNetCore.Mvc;

namespace JackTheClipperRequestHandler.Controllers
{
    /// <summary>
    /// Controller for the StaffChief
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Route("clipper")]
    [ApiController]
    public class ClipperStaffChiefController : Controller
    {
        /// <summary>
        /// Adds a new unit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="name">The name of the new unit.</param>
        /// <param name="parentUnitId">The parent unit id.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("addunit")]
        public ActionResult<MethodResult> AddOrganizationalUnit([FromQuery]Guid userId, [FromQuery] string name, [FromQuery] Guid parentUnitId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().AddOrganizationalUnit(name, parentUnitId));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Deletes the principal unit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="unitId">The unit identifier.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpDelete]
        [Route("deleteorganizationalunit")]
        public ActionResult<MethodResult> DeleteOrganizationalUnit([FromQuery]Guid userId, [FromQuery] Guid unitId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().DeleteOrganizationalUnit(unitId));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        /// <summary>
        /// Updates a unit.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="updatedUnit">The updated unit.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("updateorganizationalunit")]
        public ActionResult<MethodResult> UpdateOrganizationalUnit([FromQuery] Guid userId, [FromBody] OrganizationalUnit updatedUnit)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().UpdateOrganizationalUnit(updatedUnit));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }

        #region SetUserOrganizationalUnits                       
        /// <summary>
        /// Sets the organizational units of a user.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="units">The organizational units.</param>
        /// <returns>MethodResult indicating success.</returns>
        [HttpPut]
        [Route("setuserorganizationalunits")]
        public ActionResult<MethodResult> SetUserOrganizationalUnits([FromQuery] Guid userId, [FromBody] IReadOnlyList<Guid> units)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperOrganizationalUnitAPI>().SetUserOrganizationalUnits(user, units));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }
        #endregion

        #region GetManageableUsers                       
        [HttpGet]
        [Route("getmanageableusers")]
        public ActionResult<IReadOnlyList<BasicUserInformation>> GetManageableUsers([FromQuery] Guid userId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<IReadOnlyList<BasicUserInformation>>(Factory.GetControllerInstance<IClipperStaffChiefAPI>().GetManageableUsers(userId));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }
        #endregion

        #region GetUserInfo                       
        [HttpGet]
        [Route("getuserinfo")]
        public ActionResult<ExtendedUser> GetUserInfo([FromQuery] Guid userId, [FromQuery] Guid requested)
        {
            try
            {
                var user = ClipperUserController.Get<User>(userId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<ExtendedUser>(Factory.GetControllerInstance<IClipperStaffChiefAPI>().GetUserInfo(requested));
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            return BadRequest();
        }
        #endregion


        [HttpPut]
        [Route("adminadduser")]
        public ActionResult<MethodResult> AdministrativelyAddUser([FromQuery]Guid staffChiefId, [FromBody]Tuple<User, IReadOnlyList<Guid>> toAdd)
        {
            try
            {
                if (toAdd.Item1.Id != Guid.Empty)
                {
                    throw new ArgumentNullException();
                }

                var user = ClipperUserController.Get<User>(staffChiefId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperStaffChiefAPI>().AdministrativelyAddUser(toAdd.Item1, toAdd.Item2));
                }

                return Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }

        [HttpDelete]
        [Route("deleteuser")]
        public ActionResult<MethodResult> DeleteUser([FromQuery]Guid staffChiefId, [FromQuery]Guid userId)
        {
            try
            {
                var user = ClipperUserController.Get<User>(staffChiefId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperStaffChiefAPI>().DeleteUser(userId));
                }

                return Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }


        [HttpPut]
        [Route("modifyuser")]
        public ActionResult<MethodResult> ModifyUser([FromQuery]Guid staffChiefId, [FromBody]Tuple<User, IReadOnlyList<Guid>> toModify)
        {
            try
            {
                if (toModify.Item1.Id == Guid.Empty)
                {
                    throw new ArgumentNullException();
                }

                var user = ClipperUserController.Get<User>(staffChiefId);
                if (user.Role > Role.User)
                {
                    return new ActionResult<MethodResult>(Factory.GetControllerInstance<IClipperStaffChiefAPI>().ModifyUser(toModify.Item1, toModify.Item2));
                }

                return Forbid();
            }
            catch (Exception error)
            {
                Console.WriteLine(error);
                return BadRequest(error.Message);
            }
        }
    }
}
