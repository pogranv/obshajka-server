using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using Obshajka.Models;
using Obshajka.Interfaces;
using Obshajka.DbManager;
using Obshajka.VerificationCodesManager.Exceptions;

namespace Obshajka.Controllers
{
    [Route("api/v1/reg")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private static readonly IVerificationCodesManager s_verificationCodesManager;

        private static readonly IDbManager s_postgresDbManager;

        private readonly ILogger<RegistrationController> _logger;

        private static class CodesManagerSettings
        {
            public static readonly int codesLifeTimeMinutes = 5;
            public static readonly string emailSenderHeader = "Общажка akkforfox5@gmail.com";
            public static readonly string emailHeader = "Код верификации";
            public static readonly string messageBody = $"Здравствуйте!{Environment.NewLine}Ваш код верификации для приложения «Общажка»: ";
        }

        static RegistrationController()
        {
            var emailParams = new VerificationCodesManager
                .EmailParams(CodesManagerSettings.emailSenderHeader, CodesManagerSettings.emailHeader, CodesManagerSettings.messageBody);
            s_verificationCodesManager = new VerificationCodesManager.VerificationCodesManager(CodesManagerSettings.codesLifeTimeMinutes, emailParams);
            s_postgresDbManager = new PostgresDbManager();
        }

        public RegistrationController()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<RegistrationController>();
        }

        /// <summary>
        /// Метод отправляет код подтверждения на почту пользователя.
        /// </summary>
        /// <param name="user">Пользователь, кому нужно отправить письмо с кодом подтверждения</param>
        /// <returns></returns>
        [HttpPost("verification")]
        public IActionResult SendVerificationCode([FromBody] User user)
        {
            if (!user.IsValidEmail())
            {
                _logger.LogWarning($"Регистрация не удалась: почта {user.Email} не имеет домена @edu.hse.ru или @hse.ru");
                return BadRequest($"Регистрация не удалась: почта {user.Email} не имеет домена @edu.hse.ru или @hse.ru");
            }
            if (s_postgresDbManager.CheckUserExist(user.Email))
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {user.Email} уже зарегистрирован");
                return Conflict("Пользователь с такой почтой уже зарегистрирован!");
            }

            try
            {
                s_verificationCodesManager.AddUser(user);
                s_verificationCodesManager.SendCodeToUser(user.Email);
                return Ok();
            }
            catch (UserAlreadyWaitConfirmationException ex)
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {user.Email} ожидает подтверждения почты");
                return Conflict(ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {user.Email} не запросил подтверждения почты " +
                    $"или с момента последнего запроса на подтверждение прошло больше 5 минут");
                return NotFound(ex.Message);
            }
            catch (FailSendCodeException ex)
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {user.Email} ввел некорректный формат почты");
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Метод сверяет код, который был отправлен пользователю с заданным.
        /// В случае совпадения возвращает идентификатор пользователя.
        /// </summary>
        /// <param name="verificationCodeWithEmail">Проверочный код и почта пользователя</param>
        /// <returns></returns>
        [HttpPost("confirmation")]
        public IActionResult ConfirmVerificationCode([FromBody] VerificationCodeWithEmail verificationCodeWithEmail)
        {
            if (s_postgresDbManager.CheckUserExist(verificationCodeWithEmail.Email))
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {verificationCodeWithEmail.Email} уже зарегистрирован");
                return Conflict("Пользователь с такой почтой уже зарегистрирован!");
            }
            try
            {
                var user = s_verificationCodesManager.VerifyUser(verificationCodeWithEmail.Email, verificationCodeWithEmail.VerificationCode);
                var newUser = new User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                };

                var userId = s_postgresDbManager.SaveNewUserToDbAndGetId(newUser);
                return Ok(userId);
            }
            catch (UserNotFoundException)
            {
                _logger.LogWarning($"Регистрация не удалась: пользователь с почтой {verificationCodeWithEmail.Email} не запросил подтверждения почты " +
                    $"или с момента последнего запроса на подтверждение прошло больше 5 минут");
                return NotFound();
            }
        }
    }
}
