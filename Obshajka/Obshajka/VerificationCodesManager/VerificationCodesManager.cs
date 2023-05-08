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

        private static readonly SmtpGmailSettings s_smtpGmailSettings;

        static VerificationCodesManager()
        {
            s_smtpGmailSettings = SmtpGmailSettings.Build();
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

        /// <summary>
        /// Метод добавляет пользователя в лист ожидания подтверждения почты.
        /// </summary>
        /// <param name="user">Пользователь</param>
        /// <exception cref="UserAlreadyWaitConfirmationException"></exception>
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

        /// <summary>
        /// Метод отправляет на укзаанную почту код подтверждения.
        /// </summary>
        /// <param name="userEmail">Почта</param>
        /// <exception cref="UserNotFoundException"></exception>
        /// <exception cref="FailSendCodeException"></exception>
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
                    using (SmtpClient smtpClient = new SmtpClient(s_smtpGmailSettings.SmtpGmailString, s_smtpGmailSettings.SmtpGmailPort))
                    {
                        smtpClient.EnableSsl = true;
                        smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtpClient.UseDefaultCredentials = false;
                        smtpClient.Credentials = new NetworkCredential(s_smtpGmailSettings.EmailForSmtp, s_smtpGmailSettings.GmailKey);
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

        /// <summary>
        /// Метод сверяет код подтверждения, который был отправлен пользователю с введенным.
        /// В случае соотвествия возвращает информацию о пользователе.
        /// </summary>
        /// <param name="userEmail"></param>
        /// <param name="verificationCode"></param>
        /// <returns></returns>
        /// <exception cref="UserNotFoundException"></exception>
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
    }
}
