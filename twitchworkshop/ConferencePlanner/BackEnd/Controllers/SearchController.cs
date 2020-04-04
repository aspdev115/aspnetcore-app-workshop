using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BackEnd.Data;
using ConferenceDTO;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public SearchController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpPost]
        public async Task<ActionResult<List<SearchResult>>> Search(SearchTerm term)
        {
            var query = term.Query.ToLowerInvariant();
            var sessionResults = (await _db.Sessions.Include(s => s.Track)
                                                   .Include(s => s.SessionSpeakers)
                                                     .ThenInclude(ss => ss.Speaker)
                                                   .ToListAsync())
                                                   .Where(s =>
                                                       s.Title.ToLowerInvariant().Contains(query) ||
                                                       s.Track.Name.ToLowerInvariant().Contains(query)
                                                   )
                                                   .ToList();

            var speakerResults = (await _db.Speakers.Include(s => s.SessionSpeakers)
                                                     .ThenInclude(ss => ss.Session)
                                                   .ToListAsync())
                                                   .Where(s =>
                                                       (s.Name?.ToLowerInvariant().Contains(query) ?? false) ||
                                                       (s.Bio?.ToLowerInvariant().Contains(query) ?? false) ||
                                                       (s.WebSite?.ToLowerInvariant().Contains(query) ?? false)
                                                   )
                                                   .ToList();

            var results = sessionResults.Select(session => new SearchResult
            {
                Type = SearchResultType.Session,
                Session = session.MapSessionResponse()
            })
            .Concat(speakerResults.Select(speaker => new SearchResult
            {
                Type = SearchResultType.Speaker,
                Speaker = speaker.MapSpeakerResponse()
            }));

            return results.ToList();
        }
    }
}