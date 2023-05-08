using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Obshajka.Interfaces;
using Obshajka.Models;
using Obshajka.DbManager;
using Obshajka.DbManager.Exceptions;

namespace Obshajka.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private static readonly IDbManager s_postgresDbManager;

        private readonly ILogger<AuthorizationController> _logger;

        static AuthorizationController()
        {
            s_postgresDbManager = new DbManager.PostgresDbManager();
            Console.WriteLine("New version2");
        }

        public AuthorizationController()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<AuthorizationController>();
        }

        /// <summary>
        /// Метод возвращает идентификатор пользователя по почте и паролю.
        /// </summary>
        /// <param name="emailWithPassword">Почта и пароль</param>
        /// <returns></returns>
        [HttpPost("authorize")]
        public IActionResult Autorize([FromBody] EmailWithPassword emailWithPassword)
        {
            try
            {
                var userId = s_postgresDbManager.GetUserIdByEmailAndPassword(emailWithPassword.Email, emailWithPassword.Password);
                return Ok(userId);
            }
            catch (UserNotFoundException)
            {
                _logger.LogWarning($"Пользователь с почтой {emailWithPassword.Email} не был авторизован: неправильный логин или пароль");
                return Unauthorized();
            }
        }
    }
}
