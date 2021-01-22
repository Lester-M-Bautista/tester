using FluentValidation.Results;
using Infra.Common.DTO;
using Infra.Core.Contract;
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
   
    [RoutePrefix("api/v1/Accounts")]
    [ApiExceptionFilter]
    public class AccountsController : ApiController
    {
        private readonly IUsersBL _bl;
        public AccountsController(IUsersBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Users>))]
        public async Task<IHttpActionResult> List([FromUri]Users filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Users();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Users))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Userid)
        {
            Users item = await _bl.GetByKeyAsync(Userid);
            if (item.Userid == Userid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Users))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Users model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Users))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Userid, [FromBody]Users model)
        {
            if (model.Userid != Userid) return BadRequest("Resource Userid's do not match.");
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Userid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Users))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Userid, [FromBody]Users model)
        {
            if (model.Userid != Userid) return BadRequest("Resource Userid's do not match.");
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
        }

        #region Helper Functions
        private async Task<IHttpActionResult> Save(Users model)
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