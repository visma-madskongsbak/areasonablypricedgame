#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using arpg.Data;
using arpg.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using IGDB;

namespace arpg.Pages
{
    [Authorize]
    public class RankingsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IGDBClient _igdbClient;

        public RankingsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IGDBClient iGDBClient)
        {
            _context = context;
            _userManager = userManager;
            _igdbClient = iGDBClient;
        }

        [BindProperty]
        public IEnumerable<Game> Ranking { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var efuser = await _context.Users.Include(u => u.Rankings).SingleAsync(u => u.Id == user.Id);
            if (efuser == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            if (efuser.Rankings == null)
            {
                var game = new Game(1, "testgame", "yo");
                await _context.Games.AddAsync(game);

                user.Rankings = new Game[] { game };
                await _context.SaveChangesAsync();
            }

            return Page();
        }


        public async Task<JsonResult> OnGetSearchGameAsync(string query)
        {
            if (string.IsNullOrEmpty(query)) return new JsonResult("");
            var games = await _igdbClient.QueryAsync<IGDB.Models.Game>(IGDBClient.Endpoints.Games, $"search \"{query.ToLower().Trim()}\"; fields id,name,cover; limit 10;");
            return new JsonResult(games);
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Ranking).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //KEK fix later
                //if (!RankingExists(Ranking.Id))
                //{
                //    return NotFound();
                //}
                //else
                //{
                //    throw;
                //}
            }

            return RedirectToPage("./Index");
        }

        //private bool RankingExists(int id)
        //{
        //    return _context.Ranking.Any(e => e.Id == id);
        //}
    }
}
