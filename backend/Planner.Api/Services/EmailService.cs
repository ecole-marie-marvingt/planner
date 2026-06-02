using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using Planner.Api.Models;

namespace Planner.Api.Services;

public sealed class EmailService(
    IOptionsMonitor<EmailSettings> options,
    ILogger<EmailService> logger) : IEmailService
{
    // Throttling pour éviter le rate limiting de Gmail
    private static readonly SemaphoreSlim _emailSemaphore = new(1, 1);
    private static DateTime _lastEmailSent = DateTime.MinValue;
    public async Task SendBookingConfirmationAsync(
        Booking booking,
        Slot slot,
        CancellationToken ct = default)
    {
        var settings = options.CurrentValue;
        if (!settings.Enabled)
        {
            logger.LogInformation(
                "Email désactivé – confirmation pour {Email} non envoyée.", booking.Email);
            return;
        }

        var message = BuildConfirmationEmail(booking, slot, settings);

        // Throttling : attendre au moins 2 secondes entre les emails
        await _emailSemaphore.WaitAsync(ct);
        try
        {
            var timeSinceLastEmail = DateTime.UtcNow - _lastEmailSent;
            var delayNeeded = TimeSpan.FromSeconds(2) - timeSinceLastEmail;
            if (delayNeeded > TimeSpan.Zero)
            {
                await Task.Delay(delayNeeded, ct);
            }

            // Retry 3 fois en cas d'erreur
            int maxRetries = 3;
            for (int attempt = 1; attempt <= maxRetries; attempt++)
            {
                try
                {
                    using var client = new SmtpClient();
                    var secureOption = settings.UseSsl
                        ? SecureSocketOptions.StartTls
                        : SecureSocketOptions.None;

                    await client.ConnectAsync(settings.Host, settings.Port, secureOption, ct);

                    if (!string.IsNullOrEmpty(settings.UserName))
                        await client.AuthenticateAsync(settings.UserName, settings.Password, ct);

                    await client.SendAsync(message, ct);
                    await client.DisconnectAsync(true, ct);

                    _lastEmailSent = DateTime.UtcNow;

                    logger.LogInformation(
                        "Email de confirmation envoyé à {Email} pour la réservation {BookingId}.",
                        booking.Email, booking.BookingId);
                    break;
                }
                catch (Exception ex) when (attempt < maxRetries)
                {
                    logger.LogWarning(ex,
                        "Tentative {Attempt}/{MaxRetries} d'envoi échouée pour {BookingId}. Nouvelle tentative dans 5 secondes.",
                        attempt, maxRetries, booking.BookingId);
                    await Task.Delay(TimeSpan.FromSeconds(5), ct);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex,
                        "Échec définitif de l'envoi de l'email de confirmation pour {BookingId} après {MaxRetries} tentatives.",
                        booking.BookingId, maxRetries);
                }
            }
        }
        finally
        {
            _emailSemaphore.Release();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private MimeMessage BuildConfirmationEmail(Booking booking, Slot slot, EmailSettings settings)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        msg.To.Add(new MailboxAddress(booking.UserName, booking.Email));
        msg.Subject = $"Confirmation de réservation – {slot.Title}";
        
        // En-têtes pour améliorer la délivrabilité et éviter les spam
        msg.MessageId = MimeUtils.GenerateMessageId("gmail.com");
        msg.Date = DateTimeOffset.UtcNow;
        
        // Reply-To : peut être différent de From
        var replyToAddress = string.IsNullOrEmpty(settings.ReplyToAddress) 
            ? settings.FromAddress 
            : settings.ReplyToAddress;
        msg.ReplyTo.Add(new MailboxAddress(settings.FromName, replyToAddress));
        
        msg.Headers.Add("X-Mailer", "Planner API v1.0");
        msg.Headers.Add("X-Priority", "3");
        msg.Headers.Add("Importance", "normal");
        msg.Headers.Add("List-Unsubscribe-Post", "List-Unsubscribe=One-Click");
        msg.Headers.Add("List-Unsubscribe", $"<mailto:{settings.FromAddress}?subject=unsubscribe>");

        var date = slot.Date.ToString("dddd d MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
        var cancelUrl = booking.CancellationUrl;
        var startTime = slot.StartTime.ToString(@"HH\:mm");
        var endTime = slot.EndTime.ToString(@"HH\:mm");

        // HTML simplifié pour meilleure délivrabilité
        var html = "<html lang=\"fr\"><head><meta charset=\"UTF-8\"><style type=\"text/css\">body{font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;}.container{max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,0.1);}.header{background:#2563eb;padding:24px 32px;}.header h1{color:#fff;margin:0;font-size:22px;}.content{padding:32px;}.content h2{color:#1e3a5f;margin-top:0;}.info-table{width:100%;border-collapse:collapse;margin:20px 0;}.info-table td{padding:10px;}.info-table tr:nth-child(odd) td:first-child{background:#f8fafc;}.info-row-label{font-weight:bold;color:#64748b;width:40%;}.action-button{display:inline-block;padding:12px 28px;background:#dc2626;color:#fff;text-decoration:none;border-radius:6px;font-weight:bold;font-size:15px;}.footer{color:#94a3b8;font-size:12px;text-align:center;margin-top:32px;}</style></head><body>"
            + "<div class=\"container\">"
            + "<div class=\"header\"><h1>École Marie Marvingt</h1></div>"
            + "<div class=\"content\">"
            + "<h2>Votre réservation est confirmée !</h2>"
            + $"<p>Bonjour <strong>{HtmlEncode(booking.UserName)}</strong>,</p>"
            + "<p>Votre inscription au créneau suivant a bien été enregistrée :</p>"
            + "<table class=\"info-table\">"
            + $"<tr><td class=\"info-row-label\">Créneau</td><td>{HtmlEncode(slot.Title)}</td></tr>"
            + $"<tr><td class=\"info-row-label\">Date</td><td>{HtmlEncode(date)}</td></tr>"
            + $"<tr><td class=\"info-row-label\">Horaire</td><td>{startTime} – {endTime}</td></tr>"
            + $"<tr><td class=\"info-row-label\">E-mail</td><td>{HtmlEncode(booking.Email)}</td></tr>"
            + "</table>"
            + "<hr style=\"border:none;border-top:1px solid #e2e8f0;margin:24px 0;\">"
            + "<p style=\"color:#64748b;font-size:14px;\">Si vous ne pouvez plus venir, vous pouvez annuler votre réservation :</p>"
            + "<div style=\"text-align:center;margin:24px 0;\">"
            + $"<a href=\"{HtmlEncode(cancelUrl)}\" class=\"action-button\">Annuler ma réservation</a>"
            + "</div>"
            + "<p class=\"footer\">Ce message a été envoyé automatiquement. Merci de ne pas y répondre.</p>"
            + "</div></div></body></html>";

        var text = $"Réservation confirmée – {slot.Title}\n"
            + $"═════════════════════════════════════════════════════\n\n"
            + $"Bonjour {booking.UserName},\n\n"
            + $"Votre inscription a bien été enregistrée :\n\n"
            + $"Créneau : {slot.Title}\n"
            + $"Date    : {date}\n"
            + $"Horaire : {startTime} – {endTime}\n"
            + $"E-mail  : {booking.Email}\n\n"
            + $"Pour annuler votre réservation, consultez l'email HTML ou\n"
            + $"accédez au lien suivant :\n"
            + $"{cancelUrl}\n\n"
            + $"───────────────────────────────────────────────────────\n"
            + $"Message automatique. Merci de ne pas y répondre.\n"
            + $"École Marie Marvingt";

        var bodyBuilder = new BodyBuilder { HtmlBody = html, TextBody = text };
        msg.Body = bodyBuilder.ToMessageBody();
        return msg;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
