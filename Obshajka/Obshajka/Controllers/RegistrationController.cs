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
            s_postgresDbManager = new DbManager.PostgresDbManager();
        }

        public RegistrationController()
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<RegistrationController>();
        }

        [HttpPost("verification")]
        public IActionResult SendVerificationCode([FromBody] User user)
        {
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


        [HttpPost("confirmation")]
        public IActionResult ConfirmVerificationCode([FromBody] VerificationCodeWithEmail verificationCodeWithEmail)
        {
            try
            {
                var user = s_verificationCodesManager.VerifyUser(verificationCodeWithEmail.Email, verificationCodeWithEmail.VerificationCode);
                var newUser = new Models.User
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
