using Microsoft.AspNetCore.Mvc;
using WorkFlowDemo.Models.Common;

namespace WorkFlowDemo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseController : ControllerBase
    {
        /// <summary>
        /// 成功返回
        /// </summary>
        /// <returns></returns>
        protected IActionResult Success()
        {
            return new JsonResult(ApiResponse.Success());
        }
        /// <summary>
        /// 成功返回带数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        protected IActionResult Success<T>(T data)
        {
            return new JsonResult(ApiResponse.Success(data));
        }
        /// <summary>
        /// 失败返回
        /// </summary>
        /// <param name="message"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        protected IActionResult Fail(string message, int code = 500)
        {
            return new JsonResult(ApiResponse.Fail(message, code));
        }
    }
}