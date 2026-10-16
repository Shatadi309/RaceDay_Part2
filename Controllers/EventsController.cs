using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay_Part2.Data;
using RaceDay_Part2.Models;
using RaceDay_Part2.Models.DTO;
using System.Data.SqlClient;

namespace RaceDay_Part2.Controllers
{//start of namespace
    [Route("api/events")]
    [ApiController]
    public class EventsController : ControllerBase
    {//start of class
        private readonly DatabaseHelper _db;

        public EventsController(DatabaseHelper db) => _db = db;

        /// <summary>List all events.</summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var list = new List<Event>();
            using var reader = _db.ExecuteReader(
                "SELECT eventID, eventName, eventDescription, eventDate, location, entryFee, userID FROM event");

            while (reader.Read())
            {
                list.Add(new Event
                {
                    EventID = reader.GetInt32(0),
                    EventName = reader.GetString(1),
                    EventDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                    EventDate = reader.GetDateTime(3),
                    Location = reader.GetString(4),
                    EntryFee = reader.GetDecimal(5),
                    UserID = reader.GetInt32(6)
                });
            }
            return Ok(list);
        }

        /// <summary>Get one event by ID.</summary>
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {//start of getById method
            using var reader = _db.ExecuteReader(
                "SELECT eventID, eventName, eventDescription, eventDate, location, entryFee, userID FROM event WHERE eventID = @id",
                new SqlParameter("@id", id));

            if (!reader.Read()) return NotFound("Event does not exist.");

            return Ok(new Event
            {
                EventID = reader.GetInt32(0),
                EventName = reader.GetString(1),
                EventDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                EventDate = reader.GetDateTime(3),
                Location = reader.GetString(4),
                EntryFee = reader.GetDecimal(5),
                UserID = reader.GetInt32(6)
            });
        }//end of getById method

        /// <summary>Create a new event (Organizer only).</summary>
        [HttpPost]
        public IActionResult Create([FromBody] CreateEventDto dto)
        {//start of IAction
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return Unauthorized("Please log in.");

            var newId = _db.ExecuteScalar(
                @"INSERT INTO event (eventName, eventDescription, eventDate, location, entryFee, userID)
                  OUTPUT INSERTED.eventID
                  VALUES (@n, @d, @date, @loc, @fee, @uid)",
                new SqlParameter("@n", dto.EventName),
                new SqlParameter("@d", (object?)dto.EventDescription ?? DBNull.Value),
                new SqlParameter("@date", dto.EventDate),
                new SqlParameter("@loc", dto.Location),
                new SqlParameter("@fee", dto.EntryFee),
                new SqlParameter("@uid", userId.Value));

            _db.ExecuteNonQuery(
                "INSERT INTO eventCategory (eventID, categoryID) VALUES (@eid, @cid)",
                new SqlParameter("@eid", Convert.ToInt32(newId)),
                new SqlParameter("@cid", dto.CategoryID));

            return StatusCode(201, new { EventID = newId });
        }//end of IAction

        /// <summary>Update an event (Organizer only).</summary>
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] UpdateEventDto dto)
        {
            var rows = _db.ExecuteNonQuery(
                @"UPDATE event SET eventName=@n, eventDescription=@d, eventDate=@date,
                  location=@loc, entryFee=@fee WHERE eventID=@id",
                new SqlParameter("@n", dto.EventName),
                new SqlParameter("@d", (object?)dto.EventDescription ?? DBNull.Value),
                new SqlParameter("@date", dto.EventDate),
                new SqlParameter("@loc", dto.Location),
                new SqlParameter("@fee", dto.EntryFee),
                new SqlParameter("@id", id));

            return rows == 0 ? NotFound("Event does not exist.") : Ok("Event updated.");
        }

        /// <summary>Delete an event (Organizer only).</summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {//start of IAction Delete
            var rows = _db.ExecuteNonQuery(
                "DELETE FROM event WHERE eventID = @id",
                new SqlParameter("@id", id));

            return rows == 0 ? NotFound("Event does not exist.") : NoContent();
        }//end of IAction Delete

        /// <summary>List all events with their categories joined from the DB.</summary>
        [HttpGet("details")]
        public IActionResult GetDetailed()
        {//start of Getdetails method
            var list = new List<object>();
            using var reader = _db.ExecuteReader(
                @"SELECT e.eventID, e.eventName, e.eventDescription, e.eventDate,
                 e.location, e.entryFee, e.userID,
                 c.categoryID, c.categoryName
          FROM event e
          LEFT JOIN eventCategory ec ON e.eventID = ec.eventID
          LEFT JOIN category c ON ec.categoryID = c.categoryID
          ORDER BY e.eventID");

            var current = new Dictionary<int, dynamic>();

            while (reader.Read())
            {
                int eventId = reader.GetInt32(0);

                if (!current.ContainsKey(eventId))
                {
                    var ev = new
                    {
                        eventID = eventId,
                        eventName = reader.GetString(1),
                        eventDescription = reader.IsDBNull(2) ? null : reader.GetString(2),
                        eventDate = reader.GetDateTime(3),
                        location = reader.GetString(4),
                        entryFee = reader.GetDecimal(5),
                        userID = reader.GetInt32(6),
                        categories = new List<object>()
                    };
                    current[eventId] = ev;
                    list.Add(ev);
                }

                if (!reader.IsDBNull(7))
                {
                    ((List<object>)current[eventId].categories).Add(new
                    {
                        categoryID = reader.GetInt32(7),
                        categoryName = reader.GetString(8)
                    });
                }
            }
            return Ok(list);
        }//end of GetDetails method
    }
}