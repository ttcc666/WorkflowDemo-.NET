using AutoMapper;
using Dm;
using Microsoft.AspNetCore.Mvc;
using WorkFlowDemo.Api.Exceptions;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.Api.Controllers
{
    public class MaterialController : BaseController
    {
        private readonly MaterialBll _materialBll;
        private readonly IMapper _mapper;

        public MaterialController(MaterialBll materialBll, IMapper mapper)
        {
            _materialBll = materialBll;
            _mapper = mapper;
        }

        /// <summary>
        /// 扫描物料并保存临时记录
        /// </summary>
        /// <param name="createMaterialDto"></param>
        /// <returns></returns>
        [HttpPost("Scan")]
        public async Task<IActionResult> Scan([FromBody] CreateMaterialDto createMaterialDto)
        {
            var scan = _mapper.Map<MaterialTemporaryScan>(createMaterialDto);
            var result = await _materialBll.ScanAndSaveAsync(scan);
            if (!result.Item1)
            {
                return Fail(result.Item2, 400);
            }
            return Success(result.Item2);
        }

        /// <summary>
        /// 扫描完成
        /// </summary>
        /// <param name="complete"></param>
        /// <returns></returns>
        [HttpPost("CompleteScan")]
        public async Task<IActionResult> CompleteScan([FromBody] MaterialTemporaryScanComplete complete)
        {
            var result = await _materialBll.CompleteScanAsync(complete);
            if (!result.Item1)
            {
                return Fail(result.Item2, 400);
            }
            return Success(result.Item2);
        }
    }
}