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
    public class EnrolmentsController : ControllerBase
    {
        private readonly DatabaseHelper _db;

        public EnrolmentsController(DatabaseHelper db)
        {
            _db = db;
        }

        /// <summary>Participant enrols in an event.</summary>
        [HttpPost("events/{id}/enrolment")]
        public IActionResult Enrol(int id, [FromBody] EnrolmentDto dto)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized("Please log in.");

            var evExists = _db.ExecuteScalar(
                "SELECT COUNT(*) FROM event WHERE eventID = @id",
                new SqlParameter("@id", id));
            if (Convert.ToInt32(evExists) == 0)
                return NotFound("Event does not exist.");

            var dup = _db.ExecuteScalar(
                @"SELECT COUNT(*) FROM enrolment e
                  INNER JOIN eventCategory ec ON e.eventCategoryID = ec.eventCategoryID
                  WHERE e.userID = @u AND ec.eventID = @e",
                new SqlParameter("@u", userId.Value),
                new SqlParameter("@e", id));
            if (Convert.ToInt32(dup) > 0)
                return Conflict("Already enrolled.");

            _db.ExecuteNonQuery(
                @"INSERT INTO enrolment (userID, eventCategoryID, enrolmentDate, paymentStatus, paymentAmount)
                  VALUES (@u, @ec, GETDATE(), @ps, @pa)",
                new SqlParameter("@u", userId.Value),
                new SqlParameter("@ec", dto.EventCategoryID),
                new SqlParameter("@ps", dto.PaymentStatus),
                new SqlParameter("@pa", dto.PaymentAmount));

            return StatusCode(201, "Enrolment created.");
        }

        /// <summary>Organizer views enrolments for an event.</summary>
        [HttpGet("events/{id}/enrolment")]
        public IActionResult GetForEvent(int id)
        {
            var list = new List<Enrolment>();
            using var reader = _db.ExecuteReader(
                @"SELECT e.enrolmentID, e.userID, e.eventCategoryID, e.enrolmentDate,
                         e.paymentStatus, e.paymentAmount
                  FROM enrolment e
                  INNER JOIN eventCategory ec ON e.eventCategoryID = ec.eventCategoryID
                  WHERE ec.eventID = @id",
                new SqlParameter("@id", id));

            while (reader.Read())
            {
                list.Add(new Enrolment
                {
                    EnrolmentID = reader.GetInt32(0),
                    UserID = reader.GetInt32(1),
                    EventCategoryID = reader.GetInt32(2),
                    EnrolmentDate = reader.GetDateTime(3),
                    PaymentStatus = reader.GetString(4),
                    PaymentAmount = reader.GetDecimal(5)
                });
            }
            return Ok(list);
        }

        /// <summary>Participant cancels their enrolment.</summary>
        [HttpDelete("enrolment/{id}")]
        public IActionResult Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized("Please log in.");

            var rows = _db.ExecuteNonQuery(
                "DELETE FROM enrolment WHERE enrolmentID = @id AND userID = @u",
                new SqlParameter("@id", id),
                new SqlParameter("@u", userId.Value));

            return rows == 0 ? NotFound("Enrolment does not exist.") : NoContent();
        }//end of IAction Cancel method


        /// <summary>Participant views their own enrolments (from DB).</summary>
        [HttpGet("enrolment/me")]
        public IActionResult MyEnrolments()
        {//start of MyEnrolments method
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized("Please log in.");

            var list = new List<object>();
            using var reader = _db.ExecuteReader(
                @"SELECT e.enrolmentID, e.enrolmentDate, e.paymentStatus, e.paymentAmount,
                 ev.eventID, ev.eventName, ev.eventDate, ev.location,
                 c.categoryID, c.categoryName
          FROM enrolment e
          INNER JOIN eventCategory ec ON e.eventCategoryID = ec.eventCategoryID
          INNER JOIN event ev ON ec.eventID = ev.eventID
          INNER JOIN category c ON ec.categoryID = c.categoryID
          WHERE e.userID = @u
          ORDER BY e.enrolmentDate DESC",
                new SqlParameter("@u", userId.Value));

            while (reader.Read())
            {
                list.Add(new
                {
                    enrolmentID = reader.GetInt32(0),
                    enrolmentDate = reader.GetDateTime(1),
                    paymentStatus = reader.GetString(2),
                    paymentAmount = reader.GetDecimal(3),
                    eventID = reader.GetInt32(4),
                    eventName = reader.GetString(5),
                    eventDate = reader.GetDateTime(6),
                    location = reader.GetString(7),
                    categoryID = reader.GetInt32(8),
                    categoryName = reader.GetString(9)
                });
            }
            return Ok(list);
        }//end of MyEnrolments method
    }//end of class
}//end of namespace