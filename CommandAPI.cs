using Microsoft.AspNetCore.Mvc;
using System.Linq;
using CommunityServerAPI;

namespace CommunityServerAPI
{
    [Route("api/{steamId}/{action}/{amount}/{executorName}/{data}")]
    [ApiController]
    public class CommandController : ControllerBase
    {
        private readonly CommandQueue _commandQueue;

        public CommandController(CommandQueue commandQueue)
        {
            _commandQueue = commandQueue;
        }

        [HttpGet]
        public IActionResult ExecuteCommand(ulong steamId, string action, int amount, string executorName, string data)
        {
            ActionType validAction;
            try
            {
                validAction = (ActionType)Enum.Parse(typeof(ActionType), action, true);
            }
            catch
            {
                return BadRequest("Invalid action.");
            }

            var command = new Command
            {
                Action = validAction,
                StreamerId = steamId,
                Amount = amount,
                ExecutorName = executorName,
                Data = data.Split('§'),
            };

            _commandQueue.Enqueue(command);
            return Ok($"Command for {executorName} has been queued for execution.");
        }
    }
}