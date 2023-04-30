using System.Net;
using System.Net.Mail;
using System.Collections.Concurrent;

using Obshajka.VerificationCodesManager.Exceptions;
using Obshajka.Interfaces;

namespace Obshajka.VerificationCodesManager
{
    public class VerificationCodesManager : IVerificationCodesManager
    {
        private ConcurrentDictionary<string, CodeWithDetails> _emailToDetails;
        private static readonly Random s_getVerificationCode = new();

        private readonly Timer _periodic;
        private readonly int _codesLifeTimeMinutes;

        private readonly string _emailSenderHeader;
        private readonly string _emailHeader;
        private readonly string _messageBody;
        

        private readonly ILogger<VerificationCodesManager> _logger;

        private static readonly SmtpGmailSettings smtpGmailSettings;

        static VerificationCodesManager()
        {
            smtpGmailSettings = SmtpGmailSettings.Build();
        }

        public VerificationCodesManager(int codesLifeDurationMinutes, EmailParams emailParams)
        {
            _logger = LoggerFactory.Create(options => options.AddConsole()).CreateLogger<VerificationCodesManager>();

            _emailToDetails = new();

            _codesLifeTimeMinutes = codesLifeDurationMinutes;
            TimerCallback tm = new TimerCallback(UpdateVerificationCodes);
            _periodic = new Timer(tm, null, 0, _codesLifeTimeMinutes * 60 * 1000); // minutes * seconds * milisec

            _emailSenderHeader = emailParams.EmailSenderHeader;
            _emailHeader = emailParams.EmailHeader;
            _messageBody = emailParams.MessageBody;


        }

        private void UpdateVerificationCodes(object obj)
        {
            var toRemoveEmails = _emailToDetails.Where(item => item.Value.IsDurationOfExistsOverdue(_codesLifeTimeMinutes));
            foreach (var item in toRemoveEmails)
            {
                if (_emailToDetails.ContainsKey(item.Key))
                {
                    string userEmail = item.Key;
                    if (_emailToDetails.TryRemove(item))
                    {
                        _logger.LogInformation($"{TimeOnly.FromDateTime(DateTime.Now)}: Пользователь {userEmail} не подтвердил свою почту и был удален из очереди ожидания");
                    }
                }
            }
        }

        public void AddUser(IUser user)
        {
            if (_emailToDetails.ContainsKey(user.Email))
            {
                throw new UserAlreadyWaitConfirmationException($"С момента последнего запроса кода подтверждения должно пройти { _codesLifeTimeMinutes } минут.");
            }

            string verificationCode = s_getVerificationCode.Next(1000, 10000).ToString();
            _emailToDetails[user.Email] = new CodeWithDetails(user, verificationCode);

            _logger.LogInformation($"{TimeOnly.FromDateTime(DateTime.Now)} Пользователь {user.Email} добавлен в очередь ожидания подтверждения почты");
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
                    using (SmtpClient smtpClient = new SmtpClient(smtpGmailSettings.SmtpGmailString, smtpGmailSettings.SmtpGmailPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(smtpGmailSettings.EmailForSmtp, smtpGmailSettings.GmailKey);
                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (FormatException)
            {
                _logger.LogWarning($"Не удалось отправить код потверждения на почту {userEmail}: неверный формат почты");
                throw new FailSendCodeException("Не удалось отправить код подтверждения: неверный формат почты");
            }
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

                    _logger.LogInformation($"{TimeOnly.FromDateTime(DateTime.Now)}: Пользователь {userEmail} подтвердил свою почту и был удален из очереди ожидания");

                    return codeWithDetails.User;
                }
            }
            throw new UserNotFoundException($"С момента последней отправки кода подтверждения прошло больше {_codesLifeTimeMinutes} минут. Повторите запрос на отправку кода подтверждения.");
        }
    }
}
