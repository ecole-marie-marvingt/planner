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
        
        // En-têtes essentiels pour Hotmail/Outlook
        msg.Headers.Add("X-MSMail-Priority", "Normal");
        msg.Headers.Add("Precedence", "bulk");
        msg.Headers.Add("X-Originating-IP", "[37.187.38.22]");
        msg.Headers.Add("MIME-Version", "1.0");

        var date = slot.Date.ToString("dddd d MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
        var cancelUrl = booking.CancellationUrl;
        var startTime = slot.StartTime.ToString(@"HH\:mm");
        var endTime = slot.EndTime.ToString(@"HH\:mm");

        // HTML simplifié pour meilleure délivrabilité (Hotmail compatible)
        var html = "<!DOCTYPE html><html lang=\"fr\"><head><meta charset=\"UTF-8\"><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\"><title>Confirmation de réservation</title><style type=\"text/css\">body{font-family:'Segoe UI',Arial,sans-serif;line-height:1.6;color:#333;background-color:#f5f5f5;margin:0;padding:0;}table{border-collapse:collapse;width:100%;}.container{background-color:#ffffff;max-width:600px;margin:0 auto;}.header{background-color:#2563eb;padding:24px 32px;}.header h1{color:#ffffff;margin:0;font-size:20px;font-weight:600;}.content{padding:32px 24px;}.section-title{color:#1e3a5f;font-size:18px;font-weight:600;margin:0 0 16px 0;}.section-text{color:#555;font-size:14px;margin:0 0 16px 0;line-height:1.5;}.info-table{width:100%;margin:20px 0;border-collapse:collapse;}.info-table tr{border-bottom:1px solid #e0e0e0;}.info-table td{padding:12px;font-size:14px;}.info-table td:first-child{font-weight:600;color:#64748b;background-color:#f8fafc;width:35%;}.action-container{text-align:center;margin:28px 0;}.action-button{display:inline-block;padding:12px 32px;background-color:#dc2626;color:#ffffff;text-decoration:none;border-radius:4px;font-weight:600;font-size:14px;}.footer{background-color:#f8f8f8;padding:16px 24px;text-align:center;font-size:12px;color:#999;border-top:1px solid #e0e0e0;}</style></head><body><table role=\"presentation\" width=\"100%\"><tr><td align=\"center\"><table role=\"presentation\" class=\"container\"><tr><td class=\"header\"><h1>École Marie Marvingt</h1></td></tr><tr><td class=\"content\">"
            + $"<p class=\"section-title\">Votre réservation est confirmée !</p>"
            + $"<p class=\"section-text\">Bonjour <strong>{HtmlEncode(booking.UserName)}</strong>,</p>"
            + $"<p class=\"section-text\">Votre inscription au créneau suivant a bien été enregistrée :</p>"
            + "<table class=\"info-table\" role=\"presentation\">"
            + $"<tr><td>Créneau</td><td>{HtmlEncode(slot.Title)}</td></tr>"
            + $"<tr><td>Date</td><td>{HtmlEncode(date)}</td></tr>"
            + $"<tr><td>Horaire</td><td>{startTime} – {endTime}</td></tr>"
            + $"<tr><td>E-mail</td><td>{HtmlEncode(booking.Email)}</td></tr>"
            + "</table>"
            + "<p class=\"section-text\">Si vous ne pouvez plus venir, vous pouvez annuler votre réservation en cliquant sur le bouton ci-dessous :</p>"
            + $"<div class=\"action-container\"><a href=\"{HtmlEncode(cancelUrl)}\" class=\"action-button\">Annuler ma réservation</a></div>"
            + "</td></tr><tr><td class=\"footer\"><p style=\"margin:0;\">Ce message a été envoyé automatiquement. Merci de ne pas y répondre.</p><p style=\"margin:8px 0 0 0;\">© École Marie Marvingt</p></td></tr></table></td></tr></table></body></html>";

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
