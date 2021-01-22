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
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace Infra.Identity.Server.Areas.v1.Controllers
{
    [RoutePrefix("api/v1/Usersinroles")]
    [ApiExceptionFilter]
    public class UsersinroleController : ApiController
    {
        private readonly IUsersinroleBL _bl;
        public UsersinroleController(IUsersinroleBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Usersinrole>))]
        public async Task<IHttpActionResult> List([FromUri]Usersinrole filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Usersinrole();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("{Userinroleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Usersinrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> Get(Guid Userinroleid)
        {
            Usersinrole item = await _bl.GetByKeyAsync(Userinroleid);
            if (item.Userinroleid == Userinroleid) return Ok(item);
            return NotFound();
        }

        [HttpPost]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.Created, Type = typeof(Usersinrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Post([FromBody]Usersinrole model)
        {
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("{Userinroleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Usersinrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Put(Guid Userinroleid, [FromBody]Usersinrole model)
        {
            //   if (model.Userinroleid != Userinroleid) return BadRequest("Resource Userinroleid's do not match.");
            //var transactionHeader = Request.Headers.GetValues("TransactionContext");
            //var transactionContext_str = transactionHeader.FirstOrDefault();
            //TransactionContext client_context = JsonConvert.DeserializeObject<TransactionContext>(transactionContext_str);

            //model.TransContext = client_context;
            model.AppendTransactionContext(Request);
            return await Save(model);
        }

        [HttpPut]
        [Route("Putwithcontext/{Userinroleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Usersinrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Putwithcontext(Guid Userinroleid, [FromUri]TransactionContext context, [FromBody]Usersinrole model)
        {
            var identity = HttpContext.Current.User as ClaimsIdentity;
            if (identity != null)
            {
                IEnumerable<Claim> claims = identity.Claims;
                var x = identity.FindFirst("ClaimName").Value;

            }
            if (model.Userinroleid != Userinroleid) return BadRequest("Resource Userinroleid's do not match.");
            model.TransContext = context;
            return await Save(model);
        }

        [HttpDelete]
        [Route("{Userinroleid}")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(Usersinrole))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        public async Task<IHttpActionResult> Delete(Guid Userinroleid, [FromBody]Usersinrole model)
        {
            // if (model.Userinroleid != Userinroleid) return BadRequest("Resource Userinroleid's do not match.");
            
            model = await _bl.GetByUserIdRoleId(model.Roleid, model.Userid);
            model.AppendTransactionContext(Request);
            model = await _bl.DeleteAsync(model);
            if (model.Validation.IsValid) return Ok(model);
            CreateModelState(model.Validation);
            return BadRequest(ModelState);
    
        }


        [HttpGet]
        [Route("List")]
        public async Task<List<Usersinrole>> Listbyuserroles()
        {
            return await _bl.ListAsync();
        }
        #region Helper Functions
        private async Task<IHttpActionResult> Save(Usersinrole model)
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