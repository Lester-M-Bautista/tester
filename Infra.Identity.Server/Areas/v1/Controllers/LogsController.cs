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
    [RoutePrefix("api/v1/Logs")]
    [ApiExceptionFilter]
    public class LogsController : ApiController
    {
        private readonly ILogsBL _bl;
        public LogsController(ILogsBL bl)
        {
            _bl = bl;
        }

        [HttpGet]
        [Route("")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Logs>))]
        public async Task<IHttpActionResult> List([FromUri]Logs filter, [FromUri]PageConfig pageConfig)
        {
            if (pageConfig == null)
                pageConfig = new PageConfig();
            if (filter == null)
                filter = new Logs();
            return Ok(await _bl.List(filter, pageConfig));
        }

        [HttpGet]
        [Route("GetLogs")]
        [SwaggerResponse(HttpStatusCode.OK, Type = typeof(DataList<Logs>))]
        [SwaggerResponse(HttpStatusCode.BadRequest, Type = typeof(BadRequestErrorMessage))]
        [SwaggerResponse(HttpStatusCode.NotFound)]
        public async Task<IHttpActionResult> GetLogs(DateTime datefrom, DateTime dateto)
        {
            List<Logs> item = await _bl.GetLogs(datefrom.Date,dateto.Date);
            DataList<Logs> datalist = new DataList<Logs>();
            datalist.Count = item.Count;
            datalist.Items = item;
            return Ok(datalist);
        }

 
        #region Helper Functions
        private async Task<IHttpActionResult> Save(Logs model)
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