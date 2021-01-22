using FluentValidation.Results;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Macrin.Common;
using Macrin.WebApi;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{

    [RoutePrefix("api/v1/IdentityClients")]
    [ApiExceptionFilter]
    public class IdentityClientController : ApiController
    {
        private readonly IIdentityApplicationBL _bl;
        public IdentityClientController(IIdentityApplicationBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<IdentityClient>))]
        public async Task<IHttpActionResult> List([FromUri]IdentityClient filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new IdentityClient();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityClient))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Applicationid)
        {
            IdentityClient item = await _bl.GetByKeyAsync(Applicationid);
            if (item.Applicationid == Applicationid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(IdentityClient))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]IdentityClient model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityClient))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Applicationid, [FromBody]IdentityClient model)
        {
            if (model.Applicationid != Applicationid) return BadRequest("Resource Applicationid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityClient))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Applicationid, [FromBody]IdentityClient model)
        {
            if (model.Applicationid != Applicationid) return BadRequest("Resource Applicationid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(IdentityClient model)
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