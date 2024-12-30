using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using PracticeAPI.Models;
using PracticeAPI.Models.DTOs;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static PracticeAPI.Models.User;

namespace PracticeAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly PracticeAPIContext _context;
        private readonly IConfiguration _configuration;
        public UserController(PracticeAPIContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegistartionUserDTOs dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return BadRequest("Email already in use.");

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                ContactNumber = dto.ContactNumber,
                Postcode = dto.Postcode,
                Gender = dto.Gender,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Hobbies = new List<Hobby>(),
                Files = new List<FileUpload>()
            };

            // Populate hobbies and set their User property
            user.Hobbies = dto.Hobbies.Select(h => new Hobby
            {
                Name = h,
                User = user
            }).ToList();

            // Handle file uploads
            if (dto.Files != null && dto.Files.Count > 0)
            {
                foreach (var file in dto.Files)
                {
                    var filePath = Path.Combine("Uploads", file.FileName);

                    // Save file to server
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    // Add file metadata
                    user.Files.Add(new FileUpload
                    {
                        FileName = file.FileName,
                        FilePath = filePath,
                        User = user // Set the required User property
                    });
                }
            }
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully!");
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDTOscs dto)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized("Invalid credentials.");

            var token = GenerateJwtToken(user);

            return Ok(new { Token = token });
        }

        [Authorize]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDTO updatedUserDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _context.Users
                .Include(u => u.Hobbies)
                .Include(u => u.Files)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found.");

            // Update user details
            user.FirstName = updatedUserDto.FirstName;
            user.LastName = updatedUserDto.LastName;
            user.Email = updatedUserDto.Email;
            user.ContactNumber = updatedUserDto.ContactNumber;
            user.Postcode = updatedUserDto.Postcode;
            user.Gender = updatedUserDto.Gender;
            user.Address = updatedUserDto.Address;
            user.City = updatedUserDto.City;
            user.State = updatedUserDto.State;

            // Update hobbies
            user.Hobbies.Clear();
            user.Hobbies = updatedUserDto.Hobbies.Select(h => new Hobby
            {
                Name = h,
                User = user
            }).ToList();

            await _context.SaveChangesAsync();
            return Ok("User updated successfully.");
        }


        // 6. Delete a user
        [Authorize]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> delete(int id)
        {
            var user = await _context.Users
                .Include(u => u.Hobbies)
                .Include(u => u.Files)
                .SingleOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return NotFound("User not found.");

            // Remove the user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok("User deleted successfully.");
        }



        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // --- Protected Route ---
        [Authorize]
        [HttpGet("protected-route")]
        public IActionResult ProtectedRoute()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var claims = identity.Claims.Select(c => new { c.Type, c.Value });
                return Ok(new { Message = "This route is protected and requires a valid token.", Claims = claims });
            }
            return Unauthorized("No valid claims found.");
        }


        // --- List Users with Pagination ---
        [HttpGet("list")]
        public async Task<IActionResult> List(int page = 1, int pageSize = 10)
        {
            var users = await _context.Users
                .Include(u => u.Hobbies)
                .Include(u => u.Files)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.Email,
                    u.ContactNumber,
                    u.Postcode,
                    u.Gender,
                    u.Address,
                    u.City,
                    u.State,
                    Hobbies = u.Hobbies.Select(h => h.Name),
                    Files = u.Files.Select(f => f.FileName)
                })
                .ToListAsync();

            return Ok(new
            {
                TotalCount = await _context.Users.CountAsync(),
                CurrentPage = page,
                PageSize = pageSize,
                Users = users
            });
        }
    }
}
