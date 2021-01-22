using FluentValidation.Results;
using Infra.Common;
using Infra.Common.DTO;
using Infra.Core.Business;
using Infra.Core.Contract;
using Macrin.Common;
using Macrin.WebApi;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Infra.Identity.Server.Controllers
{
    /// <summary>
    /// Available API's for Account
    /// </summary>
	[RoutePrefix("api/Account")]
    public class AccountController : ApiController
    {
        #region Constructor Injection Code
        private readonly IUsersBL _user;
        public AccountController(IUsersBL user, ILogsBL logbl)
        {
            //_user = user;
            _user = new UsersBL(System.Web.HttpContext.Current.GetOwinContext().Get<Core.Domain.InfraEntities>(),logbl);
        }
        #endregion

        /// <summary>
        /// List UserPermission
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("UserPermissions")]
        //[Authorize]
        public async Task<List<string>> UserPermissions()
        {
            try
            {
                ClaimsIdentity identity = (ClaimsIdentity)User.Identity;
                return JsonConvert.DeserializeObject<List<string>>(identity.Claims.FirstOrDefault(x => x.Type == "permissions").Value);

                //string application = identity.Claims.FirstOrDefault(x => x.Type == "system").Value;
                //return await _user.ListAdministrativePermissionAsync(User.Identity.Name, application);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }

        }

        /// <summary>
        /// Change Password
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
		[HttpPost]
        [Route("ChangePassword")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Users))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        //[Authorize]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordModel model)
        {
            ClaimsIdentity identity = (ClaimsIdentity)User.Identity;
            Users user = await _user.ChangePasswordAsync(User.Identity.Name, model.CurrentPassword, model.NewPassword);

            if (user.Validation.IsValid)
                return Ok(user);
            CreateModelState(user.Validation);
            return BadRequest(ModelState);
        }

        private void CreateModelState(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }

        [HttpGet]
        [Route("List")]
        public async Task<DataList<Users>> List([FromUri] string query = null,
                                               [FromUri] Guid department = new Guid(),
                                               [FromUri] int pageIndex = 0,
                                               [FromUri] int pageSize = 20)
        {
            return await _user.GetList(query, department, pageIndex, pageSize);
        }

        [HttpGet]
        [Route("List/Application{application_name}")]
        public async Task<DataList<Users>> ListByAppliation(string application_name,
                                               [FromUri] string query = null,
                                               [FromUri] int pageIndex = 0,
                                               [FromUri] int pageSize = 20)
        {
            return await _user.GetListByApplication(query, application_name, pageIndex, pageSize);
        }

        [HttpGet]
        [Route("List/Role/{role}")]
        public async Task<DataList<Users>> ListByRole(string role,
                                               [FromUri] string query = null,
                                               [FromUri] int pageIndex = 0,
                                               [FromUri] int pageSize = 20)
        {
            return await _user.GetListByRole(query, role, pageIndex, pageSize);
        }
    }
}
