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

    [RoutePrefix("api/v1/IdentityLogins")]
    [ApiExceptionFilter]
    public class IdentityLoginController : ApiController
    {
        private readonly IIdentityLoginBL _bl;
        public IdentityLoginController(IIdentityLoginBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<IdentityLogin>))]
        public async Task<IHttpActionResult> List([FromUri]IdentityLogin filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new IdentityLogin();
            return Ok(await _bl.List(filter, pageConfig));
        }
      

        [HttpGet]
        [Route("{Loginid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityLogin))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Loginid)
        {
            IdentityLogin item = await _bl.GetByKeyAsync(Loginid);
            if (item.Loginid == Loginid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(IdentityLogin))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]IdentityLogin model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Loginid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityLogin))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Loginid, [FromBody]IdentityLogin model)
        {
            if (model.Loginid != Loginid) return BadRequest("Resource Loginid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Loginid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityLogin))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Loginid, [FromBody]IdentityLogin model)
        {
            if (model.Loginid != Loginid) return BadRequest("Resource Loginid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(IdentityLogin model)
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