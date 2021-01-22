using FluentValidation.Results;
using Infra.Common.DTO.Identity;
using Infra.Core.Contract.Identity;
using Macrin.Common;
using Macrin.WebApi;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
   
    [RoutePrefix("api/v1/IdentityProfiles")]
    [ApiExceptionFilter]
    public class IdentityProfileController : ApiController
    {
        private readonly IIdentityProfileBL _bl;
        public IdentityProfileController(IIdentityProfileBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<IdentityProfile>))]
        public async Task<IHttpActionResult> List([FromUri]IdentityProfile filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new IdentityProfile();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Useraccountid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityProfile))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Useraccountid)
        {
            IdentityProfile item = await _bl.GetByKeyAsync(Useraccountid);
            if (item.Useraccountid == Useraccountid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(IdentityProfile))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]IdentityProfile model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Useraccountid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityProfile))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Useraccountid, [FromBody]IdentityProfile model)
        {
            if (model.Useraccountid != Useraccountid) return BadRequest("Resource Useraccountid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Useraccountid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityProfile))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Useraccountid, [FromBody]IdentityProfile model)
        {
            if (model.Useraccountid != Useraccountid) return BadRequest("Resource Useraccountid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(IdentityProfile model)
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