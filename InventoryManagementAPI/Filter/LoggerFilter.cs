using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceLayer.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace InventoryManagementAPI.Filter
{
    public class LoggerFilter : ActionFilterAttribute
    {
        #region Fields
        private readonly ILoggerService _loggerService;

        #endregion


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            _loggerService.Info((string.Format("Action Method Executing = " + MethodBase.GetCurrentMethod() + "and Path is:" + context.HttpContext.Request.Path)));
            base.OnActionExecuting(context);
        }



    }
}
