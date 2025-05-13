using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using PulseMurdererREST.Records;
using PulseMurdererREST.Helpers;
using PulseMurdererV3;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
namespace PulseMurdererREST.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PlayersController : ControllerBase {
        private PlayerRepository _playerRepository;

        public PlayersController(PlayerRepository playerRepository) {
            _playerRepository = playerRepository;
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        // GET: api/<PlayersController>
        [HttpGet]
        public ActionResult<IEnumerable<Player>> Get() {
            IEnumerable<Player> players = _playerRepository.GetAllPlayers();
            if (players.Count() > 0)
            {
                return Ok(players);
            }
            return NoContent();
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // GET api/<PlayersController>/5
        [HttpGet("{id}")]
        public ActionResult<Player> Get(int id) {
            Player? player = _playerRepository.GetPlayerById(id);
            if (player != null)
            {
                return Ok(player);
            }
            return NotFound(player);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // POST api/<PlayersController>
            [HttpPost]
            public ActionResult<Player> Post([FromBody] PlayerRecord newPlayerRecord) {
                try {
                    Player converted = RecordHelper.ConvertPlayerRecord(newPlayerRecord);
                    Player createdPlayer = _playerRepository.AddPlayer(converted);
                    return Created("/" + createdPlayer.Id, createdPlayer);
                }
                catch (ArgumentNullException ex) {
                    return BadRequest(ex.Message);
                }
                catch (Exception ex) {
                    return BadRequest(ex.Message);
                }
            }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        // PUT api/<PlayersController>/5
        [HttpPut("{id}")]
        public ActionResult<Player> Put(int id, [FromBody] PlayerRecord newValue) {
            try {
                Player? convertedPlayer = RecordHelper.ConvertPlayerRecord(newValue);
                Player? updatedPlayer = _playerRepository.UpdatePlayer(id, convertedPlayer);

                if (updatedPlayer != null) {
                    return Ok(updatedPlayer);
                }
                else {
                    return NotFound("Player not found");
                }
            }
            catch (ArgumentNullException ex) {
                return BadRequest(ex.Message);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        // DELETE api/<PlayersController>/5
        [HttpDelete("{id}")]
        public void Delete(int id) {
        }

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost("{voterId}/vote")]
        public ActionResult Vote(int voterId, [FromBody] VoteRecord vote) {
            var voter = _playerRepository.GetPlayerById(voterId);
            var target = _playerRepository.GetPlayerById(vote.TargetId);

            if (voter == null || target == null)
                return BadRequest("Invalid player or target");

            if (!voter.IsAlive)
                return BadRequest("Dead players cannot vote");

            if (voter.HasVoted)
                return BadRequest("Player has already voted");

            target.VotesRecieved++;
            voter.HasVoted = true;

            // If all alive players have voted, resolve the vote
            List<Player> alivePlayers = _playerRepository.
                GetAllPlayers().
                Where(p => p.IsAlive).
                ToList();
            if (alivePlayers.All(p => p.HasVoted)) {
                _playerRepository.TallyVotes();

                // OPTIONAL: WebSocket broadcast here
                string? toJson = JsonSerializer.Serialize(_playerRepository.GetAllPlayers());
                UDPSender.Send(toJson);
            }

            return Ok("Vote recorded");
        }

        private void ResolveVotes(List<Player> alivePlayers) {
            var groupedVotes = alivePlayers
                .GroupBy(p => p.VotesRecieved)
                .Select(g => new { PlayerId = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .ToList();

            if (groupedVotes.Count == 0 || groupedVotes[0]?.PlayerId == null)
                return;

            int eliminatedId = groupedVotes[0].PlayerId;

            var eliminatedPlayer = _playerRepository.GetPlayerById(eliminatedId);

            if (eliminatedPlayer != null) {
                eliminatedPlayer.IsAlive = false;
                _playerRepository.UpdatePlayer(eliminatedId, eliminatedPlayer);
            }

            // Clear all votes
            foreach (var player in alivePlayers) {
                player.VotesRecieved = 0;
                _playerRepository.UpdatePlayer(player.Id, player);
            }

            // Notify clients via WebSocket
            string? toJson = JsonSerializer.Serialize(_playerRepository.GetAllPlayers());
            UDPSender.Send(toJson);
        }
    }
}
