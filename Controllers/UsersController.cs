using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Part2.Data;
using RaceDay_Part2.Models;
using RaceDay_Part2.Models.DTO;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using static RaceDay_Part2.Models.user;

namespace RaceDay_Part2.Controllers
{//start of namespace
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {//start of class
        private readonly DatabaseHelper _db;

        public UsersController(DatabaseHelper db) => _db = db;

        /// <summary>Get the logged-in user's profile.</summary>
        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized();

            using var reader = _db.ExecuteReader(
                "SELECT userID, fullName, email, role, phoneNumber FROM users WHERE userID = @id",
                new SqlParameter("@id", userId.Value));

            if (!reader.Read()) return NotFound();

            return Ok(new User
            {
                UserID = reader.GetInt32(0),
                FullName = reader.GetString(1),
                Email = reader.GetString(2),
                Role = reader.GetString(3),
                PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }

        /// <summary>Update the logged-in user's profile.</summary>
        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized();

            _db.ExecuteNonQuery(
                @"UPDATE users SET fullName = @fn, phoneNumber = @ph, email = @em WHERE userID = @id",
                new SqlParameter("@fn", dto.FullName),
                new SqlParameter("@ph", (object?)dto.PhoneNumber ?? DBNull.Value),
                new SqlParameter("@em", dto.Email),
                new SqlParameter("@id", userId.Value));

            return Ok("Profile updated.");
        }


        /// <summary>Get all registered users (for testing / admin view).</summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<User>();
            using var reader = _db.ExecuteReader(
                "SELECT userID, fullName, email, role, phoneNumber FROM users");

            while (reader.Read())
            {
                list.Add(new User
                {
                    UserID = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Email = reader.GetString(2),
                    Role = reader.GetString(3),
                    PhoneNumber = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }
            return Ok(list);
        }
    }
}