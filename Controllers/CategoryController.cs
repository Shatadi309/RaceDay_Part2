using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Part2.Data;
using RaceDay_Part2.Models;
using RaceDay_Part2.Models.DTO;
using System.Data.SqlClient;

namespace RaceDay_Part2.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public CategoriesController(DatabaseHelper db) => _db = db;

        /// <summary>Get all categories.</summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<Category>();
            using var reader = _db.ExecuteReader(
                "SELECT categoryID, categoryName, categoryDescription FROM category");

            while (reader.Read())
            {
                list.Add(new Category
                {
                    CategoryID = reader.GetInt32(0),
                    CategoryName = reader.GetString(1),
                    CategoryDescription = reader.IsDBNull(2) ? null : reader.GetString(2)
                });
            }
            return Ok(list);
        }

        /// <summary>Create a category (Organizer only).</summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateCategoryDto dto)
        {
            var exists = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM category WHERE categoryName = @n",
                new SqlParameter("@n", dto.CategoryName));

            if (Convert.ToInt32(exists) > 0)
                return Conflict("Category already exists.");

            _db.ExecuteNonQuery(
                "INSERT INTO category (categoryName, categoryDescription) VALUES (@n, @d)",
                new SqlParameter("@n", dto.CategoryName),
                new SqlParameter("@d", (object?)dto.CategoryDescription ?? DBNull.Value));

            return StatusCode(201, "Category created.");
        }

        /// <summary>Update a category (Organizer only).</summary>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] CreateCategoryDto dto)
        {
            var rows = _db.ExecuteNonQuery(
                "UPDATE category SET categoryName=@n, categoryDescription=@d WHERE categoryID=@id",
                new SqlParameter("@n", dto.CategoryName),
                new SqlParameter("@d", (object?)dto.CategoryDescription ?? DBNull.Value),
                new SqlParameter("@id", id));

            return rows == 0 ? NotFound() : Ok("Category updated.");
        }//end of method


        /// <summary>Get all categories with the number of events using each.</summary>
        [HttpGet("with-counts")]
        public IActionResult GetWithCounts()
        {//start of GetWithCounts method
            var list = new List<object>();
            using var reader = _db.ExecuteReader(
                @"SELECT c.categoryID, c.categoryName, c.categoryDescription,
                 COUNT(ec.eventID) AS eventCount
          FROM category c
          LEFT JOIN eventCategory ec ON c.categoryID = ec.categoryID
          GROUP BY c.categoryID, c.categoryName, c.categoryDescription
          ORDER BY c.categoryID");

            while (reader.Read())
            {
                list.Add(new
                {
                    categoryID = reader.GetInt32(0),
                    categoryName = reader.GetString(1),
                    categoryDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                    eventCount = reader.GetInt32(3)
                });
            }
            return Ok(list);
        }//end of GetWithCounts method
    }//end of class
}//end of namespace