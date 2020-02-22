using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GameNepal.Models;

namespace GameNepal.Filters
{
    public class ExceptionFilter: FilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext filterContext)
        {
            if (filterContext.ExceptionHandled) return;

            var msg = filterContext.Exception.Message;
            var stackTrace = filterContext.Exception.StackTrace;
            var type = filterContext.Exception.GetType().ToString();
            var source = filterContext.Exception.Source;

            Helper.LogException(source,msg,type,stackTrace);

            filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(
                new { action = "Error", controller = "Home" }));
            filterContext.ExceptionHandled = true;
        }
    }
}