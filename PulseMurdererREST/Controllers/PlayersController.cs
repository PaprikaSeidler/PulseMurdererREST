using Microsoft.AspNetCore.Mvc;
using PulseMurdererV3;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace PulseMurdererREST.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase
    {
        private PlayerRepository _playerRepository;
        public PlayersController(PlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        // GET: api/<PlayersController>
        [HttpGet]
        public ActionResult <IEnumerable<Player>> Get()
        {
            IEnumerable<Player> players = _playerRepository.GetAllPlayers();
            if (players.Count() > 0)
            {
                return Ok(players);
            }
            return NoContent();

        }

        // GET api/<PlayersController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<PlayersController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<PlayersController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<PlayersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
