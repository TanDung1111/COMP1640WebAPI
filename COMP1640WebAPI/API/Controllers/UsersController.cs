﻿using COMP1640WebAPI.BusinesLogic.DTO;
using COMP1640WebAPI.BusinesLogic.Repositories;
using COMP1640WebAPI.DataAccess.Models;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace COMP1640WebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UsersRepository _repository;
        private readonly IMapper _mapper;

        public UsersController(UsersRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            var users = await _repository.GetUsers();
            return Ok(users);
        }

        // POST: api/Users/login
        [HttpPost("login")]
        public async Task<ActionResult<Users>> Login(UsersDTOLogin user)
        {
            var existingUser = await _repository.GetUserByUsernameAsync(user.userName);

            if (existingUser == null || existingUser.password != user.password)
            {
                return NotFound("Invalid username or password.");
            }

            return Ok("RoleId: " + existingUser.roleId);
        }

        // POST: api/Users/register
        [HttpPost("register")]
        public async Task<ActionResult<Users>> Register(UsersDTOPost user)
        {
            if (await _repository.IsUsernameExistsAsync(user.userName))
            {
                return Conflict("Username existing.");
            }

            var newUser = new Users
            {
                userName = user.userName,
                password = user.password,
                roleId = 1 // Hardcoded roleId to 1
            };

            await _repository.AddUserAsync(newUser);

            return CreatedAtAction(nameof(GetUsers), new { id = newUser.userId }, newUser);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, UsersDTOPut usersDTO)
        {
            var userToUpdate = await _repository.GetUserByIdAsync(id);
            if (userToUpdate == null)
            {
                return NotFound();
            }

            if (userToUpdate.userName != usersDTO.userName && await _repository.IsUsernameExistsAsync(usersDTO.userName))
            {
                return Conflict("Username existing.");
            }

            if (!await _repository.IsRoleExistsAsync(usersDTO.roleId))
            {
                return BadRequest("Invalid roleId. Role does not exist.");
            }

            _mapper.Map(usersDTO, userToUpdate);

            await _repository.UpdateUserAsync(userToUpdate);

            return NoContent();
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsers(int id)
        {
            if (!_repository.IsUserExists(id))
            {
                return NotFound();
            }

            await _repository.DeleteUserAsync(id);

            return NoContent();
        }
    }
}
