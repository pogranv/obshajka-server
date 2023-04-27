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

namespace Obshajka.VerificationCodeManager
{
    public class VerificationCodesManager : IVerificationCodesManager
    {

        private static class SmtpSettings 
        {

        }
        private ConcurrentDictionary<string, CodeWithDetails> _emailToDetails;
        private static readonly Random _getVerificationCode = new();
        private readonly Timer _periodic;
        private readonly string _smtpGmailString = "smtp.gmail.com";
        private readonly int _smtpGmailPort = 587;
        private readonly string _emailForSmtp = "akkforfox5@gmail.com";
        private readonly string _emailSenderHeader;
        private readonly string _emailHeader;
        private readonly string _gmailKey = "lczcijnyiodlgire";
        private readonly string _messageBody;

        private readonly int _codesLifeDurationMinutes;
        private void UpdateVerificationCodes(object obj)
        {
            var toRemoveEmails = _emailToDetails.Where(item => item.Value.IsDurationOfExistsOverdue(_codesLifeDurationMinutes));
            foreach (var item in toRemoveEmails)
            {
                if (_emailToDetails.ContainsKey(item.Key))
                {
                    Console.WriteLine($"Removed: {item.Key}");
                    _emailToDetails.TryRemove(item);
                }
            }
        }
        public VerificationCodesManager(int codesLifeDurationMinutes, string emailSenderHeader, string emailHeader, string messageBody) 
        {
            _emailToDetails = new();
            _codesLifeDurationMinutes = codesLifeDurationMinutes;
            TimerCallback tm = new TimerCallback(UpdateVerificationCodes);
            _periodic = new Timer(tm, null, 0, _codesLifeDurationMinutes * 60 * 1000); // minutes * seconds * milisec
            _emailSenderHeader = emailSenderHeader;
            _emailHeader = emailHeader;
            _messageBody = messageBody;
        }
        public void AddUser(IUser user)
        {
            if (_emailToDetails.ContainsKey(user.Email))
            {
                throw new UserAlreadyWaitConfirmationException("Пользователь уже ждет подтверждения почты");
            }
            string verificationCode = _getVerificationCode.Next(1000, 10000).ToString();
            _emailToDetails[user.Email] = new CodeWithDetails(user, verificationCode);
        }

        public IUser VerifyUser(string userEmail, string verificationCode)
        {
            if (string.IsNullOrEmpty(userEmail) || !_emailToDetails.ContainsKey(userEmail))
            {
                throw new UserNotFoundException("Пользователь с такой почтой не найден");
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
            throw new UserNotFoundException("Пользователь с такой почтой не найден");
        }
        public void SendCodeToUser(string userEmail) 
        {
            if (string.IsNullOrEmpty(userEmail) || !_emailToDetails.ContainsKey(userEmail))
            {
                throw new UserNotFoundException("Пользователь с такой почтой не найден");
            }

            string userCode = _emailToDetails[userEmail].VerificationCode;

            try
            {
                using (MailMessage mailMessage = new MailMessage(_emailSenderHeader, userEmail))
                {
                    mailMessage.Subject = _emailHeader;
                    mailMessage.Body = _messageBody + userCode;
                    mailMessage.IsBodyHtml = false;
                    using (SmtpClient smtpClient = new SmtpClient(_smtpGmailString, _smtpGmailPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(_emailForSmtp, _gmailKey);
                        smtpClient.Send(mailMessage);
                    }
                }
            } 
            catch (FormatException)
            {
                throw new FailSendCodeException("Не удалось отправить код подтверждения");
            }
        }

    }
}
