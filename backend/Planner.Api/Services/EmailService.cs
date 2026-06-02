using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using Planner.Api.Models;

namespace Planner.Api.Services;

public sealed class EmailService(
    IOptionsMonitor<EmailSettings> options,
    ILogger<EmailService> logger) : IEmailService
{
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

            logger.LogInformation(
                "Email de confirmation envoyé à {Email} pour la réservation {BookingId}.",
                booking.Email, booking.BookingId);
        }
        catch (Exception ex)
        {
            // On logue mais on ne fait pas échouer la réservation
            logger.LogError(ex,
                "Échec de l'envoi de l'email de confirmation pour {BookingId}.",
                booking.BookingId);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private MimeMessage BuildConfirmationEmail(Booking booking, Slot slot, EmailSettings settings)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        msg.To.Add(new MailboxAddress(booking.UserName, booking.Email));
        msg.Subject = $"✅ Confirmation de réservation – {slot.Title}";

        var date = slot.Date.ToString("dddd d MMMM yyyy",
            System.Globalization.CultureInfo.GetCultureInfo("fr-FR"));
        var cancelUrl = booking.CancellationUrl;
        var startTime = slot.StartTime.ToString(@"hh\:mm");
        var endTime = slot.EndTime.ToString(@"hh\:mm");

        var html = "<html lang=\"fr\"><head><meta charset=\"UTF-8\"></head><body style=\"font-family:Arial,sans-serif;background:#f4f4f4;margin:0;padding:20px;\">"
            + "<div style=\"max-width:600px;margin:0 auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1)\">"
            + "<div style=\"background:#2563eb;padding:24px 32px;\"><h1 style=\"color:#fff;margin:0;font-size:22px;\">📅 École Marie Marvingt</h1></div>"
            + "<div style=\"padding:32px;\">"
            + "<h2 style=\"color:#1e3a5f;margin-top:0;\">Votre réservation est confirmée !</h2>"
            + $"<p>Bonjour <strong>{HtmlEncode(booking.UserName)}</strong>,</p>"
            + "<p>Votre inscription au créneau suivant a bien été enregistrée :</p>"
            + "<table style=\"width:100%;border-collapse:collapse;margin:20px 0;\">"
            + $"<tr><td style=\"padding:10px;background:#f8fafc;font-weight:bold;color:#64748b;width:40%\">Créneau</td><td style=\"padding:10px;background:#f8fafc;\">{HtmlEncode(slot.Title)}</td></tr>"
            + $"<tr><td style=\"padding:10px;font-weight:bold;color:#64748b;\">Date</td><td style=\"padding:10px;\">{HtmlEncode(date)}</td></tr>"
            + $"<tr><td style=\"padding:10px;background:#f8fafc;font-weight:bold;color:#64748b;\">Horaire</td><td style=\"padding:10px;background:#f8fafc;\">{startTime} – {endTime}</td></tr>"
            + $"<tr><td style=\"padding:10px;font-weight:bold;color:#64748b;\">E-mail</td><td style=\"padding:10px;\">{HtmlEncode(booking.Email)}</td></tr>"
            + "</table>"
            + "<hr style=\"border:none;border-top:1px solid #e2e8f0;margin:24px 0;\">"
            + "<p style=\"color:#64748b;font-size:14px;\">Si vous ne pouvez plus venir, vous pouvez annuler votre réservation ci-dessous.</p>"
            + "<div style=\"text-align:center;margin:24px 0;\">"
            + $"<a href=\"{cancelUrl}\" style=\"display:inline-block;padding:12px 28px;background:#dc2626;color:#fff;text-decoration:none;border-radius:6px;font-weight:bold;font-size:15px;\">Annuler ma réservation</a>"
            + "</div>"
            + "<p style=\"color:#94a3b8;font-size:12px;text-align:center;margin-top:32px;\">Ce message a été envoyé automatiquement, merci de ne pas y répondre.</p>"
            + "</div></div></body></html>";

        var text = $"Réservation confirmée – {slot.Title}\n\n"
            + $"Bonjour {booking.UserName},\n\n"
            + $"Créneau : {slot.Title}\n"
            + $"Date    : {date}\n"
            + $"Horaire : {startTime} – {endTime}\n"
            + $"E-mail  : {booking.Email}\n\n"
            + $"Pour annuler votre réservation :\n{cancelUrl}\n";

        var bodyBuilder = new BodyBuilder { HtmlBody = html, TextBody = text };
        msg.Body = bodyBuilder.ToMessageBody();
        return msg;
    }

    private static string HtmlEncode(string value) =>
        System.Net.WebUtility.HtmlEncode(value);
}
