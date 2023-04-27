using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Net;
using Obshajka.Models;
using Obshajka.Postgres;
using Obshajka.Postgres.Models;
using Obshajka.VerificationCodeSender;
using Obshajka.VerificationCodeSender.Interfaces;
using Obshajka.DbManager;
using Obshajka.VerificationCodeManager.Exceptions;
using System.Reflection.Metadata;

namespace Obshajka.Controllers
{
    [Route("api/v1/reg")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        private static IVerificationCodesManager _verificationCodesManager;
        private static readonly int _codesLifeTimeMinutes = 5; // TODO: поменять
        private static readonly string _emailSenderHeader = "Общажка akkforfox5@gmail.com";
        private static readonly string _emailHeader = "Код верификации";
        private static readonly string _messageBody = $"Здравствуйте!{Environment.NewLine}Ваш код верификации для приложения «Общажка»: ";
        private static readonly IDbManager _dbManager;

        static RegistrationController()
        {
            // TODO: класс параметров
             _verificationCodesManager = new VerificationCodeManager.VerificationCodesManager(_codesLifeTimeMinutes, _emailSenderHeader, _emailHeader, _messageBody);
            _dbManager = new DbManager.DbManager();
        }



        [HttpPost("verification")]
        public IActionResult SendVerificationCode([FromBody] NewUser newUser)
        {
            if (_dbManager.CheckUserExist(newUser.Email))
            {
                return Conflict("Пользователь с такой почтой уже зарегистрирован!");
            }
            try
            {
                _verificationCodesManager.AddUser(newUser);
                _verificationCodesManager.SendCodeToUser(newUser.Email);
                
            }
            catch (UserAlreadyWaitConfirmationException ex)
            {
                return Conflict($"С момента последнего запроса кода подтверждения должно пройти {_codesLifeTimeMinutes} минут.");
            } 
            catch (UserNotFoundException ex)
            {
                return NotFound($"С момента последней отправки кода подтверждения прошло больше {_codesLifeTimeMinutes} минут. Повторите запрос на отправку кода подтверждения.");
            } 
            catch (FailSendCodeException ex)
            {
                return StatusCode(500);
            }
            return Ok();
            // return Ok();
        }

        public record VerificationCodeWithEmail(string Email, string VerificationCode);

        [HttpPost("confirmation")]
        public async Task<IActionResult> ConfirmVerificationCode([FromBody] VerificationCodeWithEmail verificationCodeWithEmail)
        {
            long userId;
            try
            {
                NewUser user = (NewUser)_verificationCodesManager.VerifyUser(verificationCodeWithEmail.Email, verificationCodeWithEmail.VerificationCode);
                userId = _dbManager.SaveNewUserToDbAndGetId(user);
                // return Ok(userId);
            } 
            catch (UserNotFoundException)
            {
                // TODO: на клиенте обработать пустой ответ
                return NotFound();
            }
            return Ok(userId);
        }
    }
}
