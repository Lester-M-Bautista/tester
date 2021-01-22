using FluentValidation.Results;
using Infra.Common;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Macrin.Common;
using Macrin.WebApi;
using Microsoft.AspNet.Identity.Owin;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{

    [RoutePrefix("api/v1/IdentityUsers")]
    [ApiExceptionFilter]
    public class IdentityUserController : ApiController
    {
        private readonly IIdentityUserBL _bl;
        public IdentityUserController(IIdentityUserBL bl)
        {
            //  _bl = new IdentityUserBL(System.Web.HttpContext.Current.GetOwinContext().Get<Core.Domain.InfraEntities>());
            _bl = new IdentityUserBL(new Core.Domain.InfraEntities());
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<IdentityUser>))]
        public async Task<IHttpActionResult> List([FromUri]IdentityUser filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new IdentityUser();
            var items = await _bl.List(filter, pageConfig);
            return Ok(items.Items);
        }
        [HttpGet]
        [Route("{id:int}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        public async Task<IdentityUser> GetById(Guid id)
        {
            return await _bl.GetById(id);
        }

        [HttpGet]
        [Route("Email/{email}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        public async Task<IdentityUser> GetByEmail(string email)
        {
            return await _bl.GetByEmail(email);
        }

        [HttpGet]
        [Route("Name/{username}/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        public async Task<IdentityUser> GetByName(string username)
        {
            return await _bl.GetByName(username);
        }

        [HttpGet]
        [Route("{name}/{key}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        public async Task<Guid> GetByUserIdByLogin(string name, string key)
        {
            return await _bl.GetByUserIdByLogin(name, key);
        }

        [HttpGet]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Userid)
        {
            IdentityUser item = await _bl.GetByKeyAsync(Userid);
            if (item.Id == Userid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(IdentityUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]IdentityUser model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Userid, [FromBody]IdentityUser model)
        {
            if (model.Id != Userid) return BadRequest("Resource Userid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Userid, [FromBody]IdentityUser model)
        {
            if (model.Id != Userid) return BadRequest("Resource Userid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }


        [HttpGet]
        [Route("PermissionsByApplication/{username}/{application}")]
        public async Task<List<string>> ListUserPermission(string username, string application)
        {
            return await _bl.ListAdministrativePermissionAsync(username, application);

        }

        [HttpPost]
        [Route("ChangePassword/{username}/")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityUser))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> ChangePassword(string username, ChangePasswordModel model)
        {
            IdentityUser user = await _bl.ChangePasswordAsync(username, model);
            //return user;
            if (user.Validation.IsValid)
            {
                model.AppendTransactionContext(Request);
                return Ok(user);
            }
            CreateModelState(user.Validation);
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("Role/{id}")]
        public async Task<List<IdentityUser>> ListUserRole(Guid id)
        {
            return await _bl.ListUserRole(id);
        }

        [HttpGet]
        [Route("Claim/{id:int}")]
        public async Task<List<Claim>> ListUserClaim(Guid id)
        {
            return await _bl.ListUserClaim(id);
        }

        [HttpGet]
        [Route("Permissions/{id}")]
        public async Task<List<string>> ListUserPermissions(Guid id)
        {
            return await _bl.ListUserPermissions(id);

        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(IdentityUser model)
        {
            model = await _bl.SaveAsync(model);
            if (model.Validation.IsValid)
                return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        private void CreateModelState(ValidationResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
            }
        }
        #endregion
    }
}