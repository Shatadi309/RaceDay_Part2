using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Part2.Data;
using RaceDay_Part2.Helpers;
using RaceDay_Part2.Models;
using RaceDay_Part2.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using static RaceDay_Part2.Models.user;

namespace RaceDay_Part2.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly DatabaseHelper _db;
        private readonly PasswordHasher _hasher;

        public AuthController(DatabaseHelper db, PasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        /// <summary>Register a new user (Organizer or Participant).</summary>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest("Email and password are required.");

            if (dto.Role != "Organizer" && dto.Role != "Participant")
                return BadRequest("Role must be Organizer or Participant.");

            var existing = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM users WHERE email = @Email",
                new SqlParameter("@Email", dto.Email));

            if (Convert.ToInt32(existing) > 0)
                return Conflict("Email already exists.");

            var hash = _hasher.Hash(dto.Password);

            _db.ExecuteNonQuery(
                @"INSERT INTO users (fullName, email, role, phoneNumber, password)
                  VALUES (@FullName, @Email, @Role, @Phone, @Password)",
                new SqlParameter("@FullName", dto.FullName),
                new SqlParameter("@Email", dto.Email),
                new SqlParameter("@Role", dto.Role),
                new SqlParameter("@Phone", (object?)dto.PhoneNumber ?? DBNull.Value),
                new SqlParameter("@Password", hash));

            return StatusCode(201, "Account created.");
        }

        /// <summary>Log in and start a session.</summary>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            using var reader = _db.ExecuteReader(
                "SELECT userID, fullName, email, role, phoneNumber, password FROM users WHERE email = @Email",
                new SqlParameter("@Email", dto.Email));

            if (!reader.Read())
                return Unauthorized("Incorrect email or password.");

            var user = new User
            {
                UserID = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3),
                PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4),
                Password = reader.GetString(5)
            };

            if (!_hasher.Verify(dto.Password, user.Password))
                return Unauthorized("Incorrect email or password.");

            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("FullName", user.FullName);

            return Ok(new { user.UserID, user.FullName, user.Email, user.Role });
        }

        /// <summary>Log out.</summary>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok("Logged out.");
        }
    }
}