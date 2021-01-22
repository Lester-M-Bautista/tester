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
    
    [RoutePrefix("api/v1/IdentityRefreshTokens")]
    [ApiExceptionFilter]
    public class IdentityRefreshTokenController : ApiController
    {
        private readonly IIdentityRefreshTokenBL _bl;
        public IdentityRefreshTokenController(IIdentityRefreshTokenBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<IdentityRefreshToken>))]
        public async Task<IHttpActionResult> List([FromUri]IdentityRefreshToken filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new IdentityRefreshToken();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Tokenid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityRefreshToken))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Tokenid)
        {
            IdentityRefreshToken item = await _bl.GetByKeyAsync(Tokenid);
            if (item.Tokenid == Tokenid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(IdentityRefreshToken))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]IdentityRefreshToken model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Tokenid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityRefreshToken))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Tokenid, [FromBody]IdentityRefreshToken model)
        {
            if (model.Tokenid != Tokenid) return BadRequest("Resource Tokenid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Tokenid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(IdentityRefreshToken))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Tokenid, [FromBody]IdentityRefreshToken model)
        {
            if (model.Tokenid != Tokenid) return BadRequest("Resource Tokenid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        [HttpPost]
        [Route("Add")]
        public async Task AddRefreshToken(IdentityRefreshToken token)
        {
            await _bl.AddRefreshTokenAsync(token);
        }
      
        [HttpPost]
        [Route("Remove")]
        public async Task RemoveRefreshToken(Guid tokenId)
        {
            await _bl.RemoveRefreshTokenAsync(tokenId);
        }

      
        [HttpGet]
        [Route("{tokenid}")]
        public IdentityRefreshToken GetToken(string tokenId)
        {
            return _bl.GetToken(tokenId);
        }



        #region Helper Functions
        private async Task<IHttpActionResult> Save(IdentityRefreshToken model)
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