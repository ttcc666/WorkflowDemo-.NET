using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WorkFlowDemo.BLL.Services;
using WorkFlowDemo.Models.Dtos;
using WorkFlowDemo.Models.Entities;

namespace WorkFlowDemo.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : BaseController
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _userService.GetListAsync();
            var dtos = _mapper.Map<List<UserDto>>(data);
            return Success(dtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _userService.GetByIdAsync(id);
            var dto = _mapper.Map<UserDto>(data);
            return Success(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateUserDto createUserDto)
        {
            var user = _mapper.Map<User>(createUserDto);
            var data = await _userService.AddAsync(user);
            return Success(data);
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] UpdateUserDto updateUserDto)
        {
            var user = _mapper.Map<User>(updateUserDto);
            var data = await _userService.UpdateAsync(user);
            return Success(data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var data = await _userService.DeleteAsync(id);
            return Success(data);
        }
    }
}