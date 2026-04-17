using System;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Negocio.Business;
using Negocio.Business.Email;
using Negocio.Data;

namespace PortalNegocioWS.Services
{
    public class EmailQueueService : BackgroundService, IEmailQueue
    {
        private readonly ILogger<EmailQueueService> _logger;
        private readonly IDataContextFactory _factory;
        private readonly IMemoryCache _cache;
        private readonly Channel<EmailMessage> _channel;

        public EmailQueueService(
            ILogger<EmailQueueService> logger,
            IDataContextFactory factory,
            IMemoryCache cache)
        {
            _logger = logger;
            _factory = factory;
            _cache = cache;
            _channel = Channel.CreateUnbounded<EmailMessage>(
                new UnboundedChannelOptions { SingleReader = true });
        }

        // IEmailQueue implementation — called by business services and controllers
        public void Queue(EmailMessage message)
        {
            _channel.Writer.TryWrite(message);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
            {
                await ProcessWithRetryAsync(message, stoppingToken);
            }
        }

        private async Task ProcessWithRetryAsync(EmailMessage message, CancellationToken ct)
        {
            const int maxAttempts = 3;
            const int delayMs = 2000;

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    SendMailInternal(message);
                    return; // success — exit retry loop
                }
                catch (Exception ex) when (attempt < maxAttempts)
                {
                    _logger.LogWarning(ex,
                        "SendMail attempt {Attempt}/{Max} failed. Recipients: {Recipients}, Subject: {Subject}",
                        attempt, maxAttempts, message.Recipients, message.Subject);
                    await Task.Delay(delayMs, ct);
                }
                catch (Exception ex)
                {
                    // Final attempt — log ERROR and drop message (per D-06)
                    _logger.LogError(ex,
                        "SendMail permanently failed after {Max} attempts. Recipients: {Recipients}, Subject: {Subject}",
                        maxAttempts, message.Recipients, message.Subject);
                }
            }
        }

        private record SmtpConfig(string Server, int Port, string Username,
            string Password, string Sender, bool Ssl);

        private SmtpConfig GetSmtpConfig()
        {
            return _cache.GetOrCreate("smtp_config", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
                using var cx = _factory.Create();
                return new SmtpConfig(
                    Server:   cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "serv_mail").Select(x => x.CONSVALOR).First(),
                    Port:     Convert.ToInt32(cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "port_mail").Select(x => x.CONSVALOR).First()),
                    Username: cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "usr_mail").Select(x => x.CONSVALOR).First(),
                    Password: cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "pwd_mail").Select(x => x.CONSVALOR).First(),
                    Sender:   cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "send_mail").Select(x => x.CONSVALOR).First(),
                    Ssl:      Convert.ToBoolean(cx.POGECONSTANTEs.Where(x => x.CONSREFERENCIA == "ssl_mail").Select(x => x.CONSVALOR).First())
                );
            });
        }

        private void SendMailInternal(EmailMessage message)
        {
            var cfg = GetSmtpConfig();
            using var mail = new MailMessage();
            mail.From = new MailAddress(cfg.Sender);

            if (message.Bcc)
                message.Recipients.ForEach(r => mail.Bcc.Add(r));
            else
                message.Recipients.ForEach(r => mail.To.Add(r));

            mail.Subject = message.Subject;
            mail.Body = message.Body;
            mail.IsBodyHtml = true;

            using var smtp = new SmtpClient(cfg.Server);
            smtp.Port = cfg.Port;
            smtp.Credentials = new System.Net.NetworkCredential(cfg.Username, cfg.Password);
            smtp.EnableSsl = cfg.Ssl;
            smtp.Send(mail);
        }
    }
}