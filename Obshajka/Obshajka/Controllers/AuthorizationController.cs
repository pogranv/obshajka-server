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

        private static readonly IDbManager _postgresDbManager;

        static AuthorizationController()
        {
            _postgresDbManager = new DbManager.PostgresDbManager();
        }

        [HttpPost("authorize")]
        public async Task<IActionResult> Autorize([FromBody] EmailWithPassword emailWithPassword)
        {
            try
            {
                var userId = _postgresDbManager.GetUserIdByEmailAndPassword(emailWithPassword.Email, emailWithPassword.Password);
                return Ok(userId);
            }
            catch (UserNotFoundException)
            {
                return Unauthorized();
            }
        }
    }
}
