using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Macrin.Common;
using Macrin.WebApi;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using static Infra.Common.DTO.Permissionsofrole;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Permissionsofroles")]
    [ApiExceptionFilter]
    public class PermissionsofroleController : ApiController
    {
        private readonly IPermissionsofroleBL _bl;
        public PermissionsofroleController(IPermissionsofroleBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Permissionsofrole>))]
        public async Task<IHttpActionResult> List([FromUri]Permissionsofrole filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Permissionsofrole();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("ListRolePermission/{userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<RolePermissionDisplay>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> ListRolePermissionByUser(Guid userid)
        {
            return Ok(await _bl.ListRolePermissionAsyncByUserId(userid));
        }

        [HttpGet]
        [Route("{Rolepermissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissionsofrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Rolepermissionid)
        {
            Permissionsofrole item = await _bl.GetByKeyAsync(Rolepermissionid);
            if (item.Rolepermissionid == Rolepermissionid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Permissionsofrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Permissionsofrole model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Rolepermissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissionsofrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Rolepermissionid, [FromBody]Permissionsofrole model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Rolepermissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissionsofrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Rolepermissionid, [FromBody]Permissionsofrole model)
        {
            // if (model.Rolepermissionid != Rolepermissionid) return BadRequest("Resource Rolepermissionid's do not match.");
            model = await _bl.GetByRoleIdPermissionId(model.Roleid, model.Permissionid);
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("List/{roleid}")]
        public async Task<List<Permissionsofrole>> ListRolePermission(Guid roleid)
        {
            return await _bl.ListRolePermissionAsync(roleid);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Permissionsofrole model)
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