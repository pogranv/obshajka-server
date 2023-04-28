using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Net;
using Obshajka.Postgres;
using Obshajka.Postgres.Models;
using Obshajka.VerificationCodeSender.Interfaces;
using Obshajka.VerificationCodeSender;
using Obshajka.VerificationCodeManager.Exceptions;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Metadata;
using Obshajka.Interfaces;

namespace Obshajka.VerificationCodeManager
{
    public class VerificationCodesManager : IVerificationCodesManager
    {
        private static class SmtpSettings 
        {
            public static readonly string smtpGmailString = "smtp.gmail.com";
            public static readonly int smtpGmailPort = 587;
            public static readonly string gmailKey = "lczcijnyiodlgire";
            public static readonly string emailForSmtp = "akkforfox5@gmail.com";
        }

        private ConcurrentDictionary<string, CodeWithDetails> _emailToDetails;
        private static readonly Random _getVerificationCode = new();
        private readonly Timer _periodic;
        private readonly string _emailSenderHeader;
        private readonly string _emailHeader;
        private readonly string _messageBody;
        private readonly int _codesLifeTimeMinutes;

        private void UpdateVerificationCodes(object obj)
        {
            var toRemoveEmails = _emailToDetails.Where(item => item.Value.IsDurationOfExistsOverdue(_codesLifeTimeMinutes));
            foreach (var item in toRemoveEmails)
            {
                if (_emailToDetails.ContainsKey(item.Key))
                {
                    Console.WriteLine($"Removed: {item.Key}");
                    _emailToDetails.TryRemove(item);
                }
            }
        }

        public VerificationCodesManager(int codesLifeDurationMinutes, EmailParams emailParams) 
        {
            _emailToDetails = new();
            _codesLifeTimeMinutes = codesLifeDurationMinutes;
            TimerCallback tm = new TimerCallback(UpdateVerificationCodes);
            _periodic = new Timer(tm, null, 0, _codesLifeTimeMinutes * 60 * 1000); // minutes * seconds * milisec
            _emailSenderHeader = emailParams.EmailSenderHeader;
            _emailHeader = emailParams.EmailHeader;
            _messageBody = emailParams.MessageBody;
        }

        public void AddUser(IUser user)
        {
            if (_emailToDetails.ContainsKey(user.Email))
            {
                throw new UserAlreadyWaitConfirmationException($"С момента последнего запроса кода подтверждения должно пройти { _codesLifeTimeMinutes } минут.");
            }
            string verificationCode = _getVerificationCode.Next(1000, 10000).ToString();
            _emailToDetails[user.Email] = new CodeWithDetails(user, verificationCode);
        }

        public IUser VerifyUser(string userEmail, string verificationCode)
        {
            if (string.IsNullOrEmpty(userEmail) || !_emailToDetails.ContainsKey(userEmail))
            {
                throw new UserNotFoundException($"С момента последней отправки кода подтверждения прошло больше {_codesLifeTimeMinutes} минут. Повторите запрос на отправку кода подтверждения.");
            }
            if (_emailToDetails[userEmail].IsEqualsVerificationCode(verificationCode))
            {
                CodeWithDetails codeWithDetails;
                if (_emailToDetails.TryGetValue(userEmail, out codeWithDetails))
                {
                    var toRemove = new KeyValuePair<string, CodeWithDetails>(userEmail, codeWithDetails);
                    _emailToDetails.TryRemove(toRemove);
                    return codeWithDetails.User;
                }
            }
            throw new UserNotFoundException($"С момента последней отправки кода подтверждения прошло больше {_codesLifeTimeMinutes} минут. Повторите запрос на отправку кода подтверждения.");
        }

        public void SendCodeToUser(string userEmail) 
        {
            if (string.IsNullOrEmpty(userEmail) || !_emailToDetails.ContainsKey(userEmail))
            {
                throw new UserNotFoundException($"С момента последней отправки кода подтверждения прошло больше {_codesLifeTimeMinutes} минут. Повторите запрос на отправку кода подтверждения.");
            }

            string userCode = _emailToDetails[userEmail].VerificationCode;

            try
            {
                using (MailMessage mailMessage = new MailMessage(_emailSenderHeader, userEmail))
                {
                    mailMessage.Subject = _emailHeader;
                    mailMessage.Body = _messageBody + userCode;
                    mailMessage.IsBodyHtml = false;
                    using (SmtpClient smtpClient = new SmtpClient(SmtpSettings.smtpGmailString, SmtpSettings.smtpGmailPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(SmtpSettings.emailForSmtp, SmtpSettings.gmailKey);
                        smtpClient.Send(mailMessage);
                    }
                }
            } 
            catch (FormatException)
            {
                throw new FailSendCodeException("Не удалось отправить код подтверждения: неверный формат почты");
            }
        }

    }
}
