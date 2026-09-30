namespace FacturacionelectronicaCore.Web.Filters
{
    using FacturacionelectronicaCore.Negocio.Modelo;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Filters;
    public class ApiExceptionFilter : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            context.HttpContext.Response.StatusCode = 500;

            if(context.Exception is BusinessException)
            {
                var businessException = context.Exception as BusinessException;

                context.Result = new JsonResult(new
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Message = $"Un error no controlado ha ocurrido. {businessException.ExceptionMessage}. {businessException.GetBaseException().StackTrace}"
                }); ;
            } else 
            {
                context.Result = new JsonResult(new
                {
                    StatusCode = context.HttpContext.Response.StatusCode,
                    Message = $"Un error no controlado ha ocurrido. {context.Exception.Message}. {context.Exception.StackTrace}"
                });
            }

            base.OnException(context);
        }
    }
}
