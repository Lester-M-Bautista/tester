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

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Permissions")]
    [ApiExceptionFilter]
    public class PermissionsController : ApiController
    {
        private readonly IPermissionsBL _bl;
        public PermissionsController(IPermissionsBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Permissions>))]
        public async Task<IHttpActionResult> List([FromUri]Permissions filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Permissions();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Permissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissions))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Permissionid)
        {
            Permissions item = await _bl.GetByKeyAsync(Permissionid);
            if (item.Permissionid == Permissionid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Permissions))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Permissions model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Permissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissions))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Permissionid, [FromBody]Permissions model)
        {
            if (model.Permissionid != Permissionid) return BadRequest("Resource Permissionid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Permissionid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Permissions))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Permissionid, [FromBody]Permissions model)
        {
            if (model.Permissionid != Permissionid) return BadRequest("Resource Permissionid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("ByAppId/{appId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Permissions>))]
        public async Task<IHttpActionResult> ListByAppIdAsync(Guid appId)
        {
            List<Permissions> item = await _bl.ListByApplicationIdAsync(appId);
            DataList<Permissions> datalist = new DataList<Permissions>();
            datalist.Count = item.Count;
            datalist.Items = item;
            return Ok(datalist);
        }

        [HttpGet]
        [Route("PermissionByRole/{RoleId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Permissions>))]
        public async Task<IHttpActionResult> PermissionByRoleId(Guid RoleId)
        {
            List<Permissions> item = await _bl.ListPermissionByRoleId(RoleId);
            DataList<Permissions> datalist = new DataList<Permissions>();
            datalist.Count = item.Count;
            datalist.Items = item;
            return Ok(datalist);
        }

        [HttpGet]
        [Route("GetPermissionsbyUsername/Search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Permissions>))]
        public async Task<IHttpActionResult> GetPermissionsbyUsername([FromUri]PageConfig pageConfig, [FromUri]string keywords)
        {
            if (keywords == null)
                return base.Ok(new DataList<Permissions> { Count = 0, Items = new List<Permissions>() });
            return Ok(await _bl.GetPermissionsbyUsername(keywords));
        }

        [HttpGet]
        [Route("Name/{name}")]
        public async Task<Permissions> GetByNameAsync(string name)
        {
            return await _bl.GetByNameAsync(name);
        }

    

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Permissions model)
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