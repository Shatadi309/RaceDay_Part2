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
    [Route("api")]
    [ApiController]
    public class ResultsController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public ResultsController(DatabaseHelper db) => _db = db;

        /// <summary>Organizer records a result.</summary>
        [HttpPost("enrolment/{id}/result")]
        public IActionResult Record(int id, [FromBody] ResultDto dto)
        {
            var enrExists = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM enrolment WHERE enrolmentID = @id",
                new SqlParameter("@id", id));
            if (Convert.ToInt32(enrExists) == 0)
                return NotFound("Enrolment does not exist.");

            var dup = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM result WHERE enrolmentID = @id",
                new SqlParameter("@id", id));
            if (Convert.ToInt32(dup) > 0)
                return Conflict("Result already exists for this enrolment.");

            _db.ExecuteNonQuery(
                "INSERT INTO result (enrolmentID, position, finishTime) VALUES (@e, @p, @f)",
                new SqlParameter("@e", id),
                new SqlParameter("@p", dto.Position),
                new SqlParameter("@f", dto.FinishTime));

            return StatusCode(201, "Result recorded.");
        }

        /// <summary>Participant views own results.</summary>
        [HttpGet("result/me")]
        public IActionResult MyResults()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized();

            var list = new List<Result>();
            using var reader = _db.ExecuteReader(
                @"SELECT r.resultID, r.enrolmentID, r.position, r.finishTime
                  FROM result r
                  INNER JOIN enrolment e ON r.enrolmentID = e.enrolmentID
                  WHERE e.userID = @u",
                new SqlParameter("@u", userId.Value));

            while (reader.Read())
            {
                list.Add(new Result
                {
                    ResultID = reader.GetInt32(0),
                    EnrolmentID = reader.GetInt32(1),
                    Position = reader.GetInt32(2),
                    FinishTime = reader.GetTimeSpan(3)
                });
            }
            return Ok(list);
        }

        /// <summary>Organizer views all results for an event.</summary>
        [HttpGet("event/{id}/result")]
        public IActionResult EventResults(int id)
        {
            var list = new List<Result>();
            using var reader = _db.ExecuteReader(
                @"SELECT r.resultID, r.enrolmentID, r.position, r.finishTime
                  FROM result r
                  INNER JOIN enrolment e ON r.enrolmentID = e.enrolmentID
                  INNER JOIN eventCategory ec ON e.eventCategoryID = ec.eventCategoryID
                  WHERE ec.eventID = @id",
                new SqlParameter("@id", id));

            while (reader.Read())
            {
                list.Add(new Result
                {
                    ResultID = reader.GetInt32(0),
                    EnrolmentID = reader.GetInt32(1),
                    Position = reader.GetInt32(2),
                    FinishTime = reader.GetTimeSpan(3)
                });
            }
            return Ok(list);
        }
    }
}