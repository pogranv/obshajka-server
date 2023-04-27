using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Obshajka.Postgres;
using Obshajka.Models;
using Obshajka.DbManager;
using Obshajka.DbManager.Exceptions;

namespace Obshajka.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {

        private static readonly IDbManager _dbManager;

        static AuthorizationController()
        {
            _dbManager = new DbManager.DbManager();
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Autorize([FromBody] EmailWithPassword emailWithPassword)
        {
            try
            {
                var userId = _dbManager.GetUserIdByEmailAndPassword(emailWithPassword.Email, emailWithPassword.Password);
                return Ok(userId);
            }
            catch (UserNotFoundException)
            {
                return Unauthorized();
            }
        }
    }
}
