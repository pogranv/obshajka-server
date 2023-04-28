using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Net;
using Obshajka.Models;
using Obshajka.VerificationCodeSender;
using Obshajka.VerificationCodeSender.Interfaces;
using Obshajka.DbManager;
using Obshajka.DbManager.Models;
using Obshajka.VerificationCodeManager.Exceptions;
using System.Reflection.Metadata;

namespace Obshajka.Controllers
{
    [Route("api/v1/reg")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private static IVerificationCodesManager _verificationCodesManager;

        private static class CodeManagerSettings
        {
            public static readonly int codesLifeTimeMinutes = 5;
            public static readonly string emailSenderHeader = "Общажка akkforfox5@gmail.com";
            public static readonly string emailHeader = "Код верификации";
            public static readonly string messageBody = $"Здравствуйте!{Environment.NewLine}Ваш код верификации для приложения «Общажка»: ";
        }

        private static readonly IDbManager _postgresDbManager;

        static RegistrationController()
        {
            var emailParams = new VerificationCodeManager.EmailParams(CodeManagerSettings.emailSenderHeader, CodeManagerSettings.emailHeader, CodeManagerSettings.messageBody);
             _verificationCodesManager = new VerificationCodeManager.VerificationCodesManager(CodeManagerSettings.codesLifeTimeMinutes, emailParams);
            _postgresDbManager = new DbManager.PostgresDbManager();
        }

        [HttpPost("verification")]
        public IActionResult SendVerificationCode([FromBody] User user)
        {
            if (_postgresDbManager.CheckUserExist(user.Email))
            {
                return Conflict("Пользователь с такой почтой уже зарегистрирован!");
            }

            try
            {
                _verificationCodesManager.AddUser(user);
                _verificationCodesManager.SendCodeToUser(user.Email);
                return Ok();
            }
            catch (UserAlreadyWaitConfirmationException ex)
            {
                return Conflict(ex.Message);
            } 
            catch (UserNotFoundException ex)
            {
                return NotFound(ex.Message);
            } 
            catch (FailSendCodeException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        

        [HttpPost("confirmation")]
        public IActionResult ConfirmVerificationCode([FromBody] VerificationCodeWithEmail verificationCodeWithEmail)
        {
            long userId;
            try
            {
                var user = _verificationCodesManager.VerifyUser(verificationCodeWithEmail.Email, verificationCodeWithEmail.VerificationCode);
                var newUser = new DbManager.Models.User
                {
                    Name = user.Name,
                    Email = user.Email,
                    Password = user.Password,
                };
                userId = _postgresDbManager.SaveNewUserToDbAndGetId(newUser);
                return Ok(userId);
            } 
            catch (UserNotFoundException)
            {
                // TODO: на клиенте обработать пустой ответ
                return NotFound();
            }
        }
    }
}
