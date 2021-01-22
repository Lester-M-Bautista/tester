using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
using Macrin.Common;
using Macrin.WebApi;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Applications")]
    [ApiExceptionFilter]
    public class ApplicationsController : ApiController
    {
        private readonly IApplicationsBL _bl;
        public ApplicationsController(IApplicationsBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Applications>))]
        public async Task<IHttpActionResult> List([FromUri]Applications filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Applications();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Applications))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Applicationid)
        {
            Applications item = await _bl.GetByKeyAsync(Applicationid);
            if (item.Applicationid == Applicationid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Applications))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Applications model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Applications))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Applicationid, [FromBody]Applications model)
        {
            if (model.Applicationid != Applicationid) return BadRequest("Resource Applicationid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Applicationid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Applications))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Applicationid, [FromBody]Applications model)
        {
            if (model.Applicationid != Applicationid) return BadRequest("Resource Applicationid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Applications model)
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