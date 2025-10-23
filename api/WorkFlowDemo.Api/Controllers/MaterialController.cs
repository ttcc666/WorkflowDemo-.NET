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
        private readonly IMaterialService _materialService;
        private readonly IMapper _mapper;

        public MaterialController(IMaterialService materialService, IMapper mapper)
        {
            _materialService = materialService;
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
            var result = await _materialService.ScanAndSaveAsync(scan);
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
            var result = await _materialService.CompleteScanAsync(complete);
            if (!result.Item1)
            {
                return Fail(result.Item2, 400);
            }
            return Success(result.Item2);
        }

        /// <summary>
        /// 查询物料列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetMaterials")]
        public async Task<IActionResult> GetMaterials()
        {
            var materials = await _materialService.GetMaterialsAsync();
            return Success(materials);
        }

        /// <summary>
        /// 添加物料
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddMaterial")]
        public async Task<IActionResult> AddMaterial([FromBody] AddMaterialDto dto)
        {
            var material = _mapper.Map<Material>(dto);
            var result = await _materialService.AddMaterialAsync(material);
            return Success(result);
        }

        /// <summary>
        /// 查询库存列表
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetInventories")]
        public async Task<IActionResult> GetInventories()
        {
            var inventories = await _materialService.GetInventoriesAsync();
            return Success(inventories);
        }

        /// <summary>
        /// 添加库存
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("AddInventory")]
        public async Task<IActionResult> AddInventory([FromBody] AddInventoryDto dto)
        {
            var inventory = _mapper.Map<MaterialInventory>(dto);
            var result = await _materialService.AddInventoryAsync(inventory);
            return Success(result);
        }

        /// <summary>
        /// 生成批号
        /// </summary>
        /// <returns></returns>
        [HttpGet("GenerateBatchNumber")]
        public async Task<IActionResult> GenerateBatchNumber()
        {
            var batchNumber = await _materialService.GenerateBatchNumberAsync();
            return Success(batchNumber);
        }

        /// <summary>
        /// 写入临时扫描记录
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost("ScanItem")]
        public async Task<IActionResult> ScanItem([FromBody] ScanDto dto)
        {
            var scan = _mapper.Map<MaterialTemporaryScan>(dto);
            var result = await _materialService.ScanItemAsync(scan);
            if (!result.Item1)
            {
                return Fail(result.Item2, 400);
            }
            return Success(result.Item2);
        }
    }
}