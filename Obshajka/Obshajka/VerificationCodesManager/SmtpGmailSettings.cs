namespace Obshajka.VerificationCodesManager
{
    public class SmtpGmailSettings
    {
        public string SmtpGmailString { get; private set; }
        public int SmtpGmailPort { get; private set; }
        public string GmailKey { get; private set; }
        public string EmailForSmtp { get; private set; }

        public static SmtpGmailSettings Build()
        {
            var config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SmtpConnecionSettings");
            int.TryParse(config["SmtpGmailPort"], out int port);
            return new SmtpGmailSettings
            {
                SmtpGmailString = config["SmtpGmailString"],
                SmtpGmailPort = port,
                GmailKey = config["GmailKey"],
                EmailForSmtp = config["EmailForSmtp"],
            };
        }
    }
}
