﻿using Cinema.DataAccess.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema_System.Areas.Guest.Controllers
{
    [Route("api/cinemas")]
    [ApiController]
    public class CinemasController : Controller
    {

        private readonly ApplicationDbContext _context;

        public CinemasController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet("{cityName}")]
        public async Task<IActionResult> GetCinemasByCity(string cityName)
        {
            var cinemas = await _context.Theaters
                .Where(t => t.CinemaCity.Equals(cityName) && t.Status == CinemaStatus.Open)
                .ToListAsync();

            if (!cinemas.Any())
            {
                return NotFound(new { message = "Không tìm thấy rạp nào trong thành phố này." });
            }

            return Ok(cinemas);
        }
        
        [HttpGet("id/{cinemaId}")]
        public async Task<IActionResult> GetCinemasById(int cinemaId)
        {
            var cinemas = await _context.Theaters
                .Where(t => t.CinemaID == cinemaId && t.Status == CinemaStatus.Open)
                .FirstOrDefaultAsync();

            if (cinemas == null)
            {
                return NotFound(new { message = "Không tìm thấy rạp nào trong thành phố này." });
            }

            return Ok(cinemas);
        }
    }
}
