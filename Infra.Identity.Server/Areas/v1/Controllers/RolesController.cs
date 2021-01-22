

using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Macrin.Common;
using Macrin.WebApi;
using Newtonsoft.Json;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Roles")]
    [ApiExceptionFilter]
    public class RolesController : ApiController
    {
        private readonly IRolesBL _bl;
        public RolesController(IRolesBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Roles>))]
        public async Task<IHttpActionResult> List([FromUri]Roles filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Roles();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Roleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Roles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Roleid)
        {
            Roles item = await _bl.GetByKeyAsync(Roleid);
            if (item.Roleid == Roleid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Roles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Roles model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }
        [HttpGet]
        [Route("Name/{name}")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Roles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<Roles> GetByName(string name)
        {
            return await _bl.GetByNameAsync(name);
        }

        [HttpGet]
        [Route("List/{ApplicationId}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Roles>))]
       // [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> RoleByAppId(Guid ApplicationId)
        {
            List<Roles> item = await _bl.ListRoleByAppId(ApplicationId);
            DataList<Roles> datalist = new DataList<Roles>();
            datalist.Count = item.Count;
            datalist.Items = item;
            return Ok(datalist);
        }
        [HttpGet]
        [Route("RoleByUser/{id}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(List<Roles>))]
        //[SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> RoleByUserId(Guid id)
        {
            return Ok(await _bl.ListRoleByUserId(id));
        }

        [HttpPut]
        [Route("{Roleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Roles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Roleid, [FromBody]Roles model)
        {
            if (model.Roleid != Roleid) return BadRequest("Resource Roleid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Roleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Roles))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Roleid, [FromBody]Roles model)
        {
            if (model.Roleid != Roleid) return BadRequest("Resource Roleid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        [HttpGet]
        [Route("SelectbyUserName/Search")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Roles>))]
        public async Task<IHttpActionResult> SelectbyUserName([FromUri]PageConfig pageConfig, [FromUri]string keywords)
        {
            if (keywords == null)
                return base.Ok(new DataList<Roles> { Count = 0, Items = new List<Roles>() });
            return Ok(await _bl.SelectbyUserName(keywords));
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Roles model)
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