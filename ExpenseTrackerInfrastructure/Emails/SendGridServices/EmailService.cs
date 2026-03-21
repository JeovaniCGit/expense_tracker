using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Emails.Enums;
using ExpenseTracker.Application.Emails.Exceptions;
using ExpenseTracker.Application.Emails.Model;
using ExpenseTracker.Application.Emails.Models;
using ExpenseTracker.Application.Emails.Services;
using ExpenseTracker.Domain.Email.Entity;
using ExpenseTracker.Domain.Email.Repository;
using ExpenseTracker.Infrastructure.Emails.Exceptions;
using ExpenseTracker.Infrastructure.Emails.SendGridConfiguration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using System.Text.Json;

namespace ExpenseTracker.Infrastructure.Emails.Services;

public sealed class EmailService : IEmailService
{
    private readonly SendGridOptions _options;
    private readonly ISendGridClient _client;
    private readonly ILogger<EmailService> _logger;
    private readonly IDateProvider _dateTimeProvider;
    private readonly IEmailDeliveryRepository _emailDeliveryRepository;
    public EmailService(
        IOptions<SendGridOptions> options,
        ISendGridClient client,
        ILogger<EmailService> logger,
        IDateProvider dateTimeProvider,
        IEmailDeliveryRepository emailDeliveryRepository
        )
    {
        _options = options.Value;
        _client = client;
        _logger = logger;
        _dateTimeProvider = dateTimeProvider;
        _emailDeliveryRepository = emailDeliveryRepository;
    }
    public async Task<EmailSenderResult> SendPasswordResetEmail(string to, string receiverName, string verificationToken, long userId, CancellationToken ctoken = default)
    {
        return await SendEmail(to, receiverName, _options.ResetTemplateId, verificationToken, userId, ctoken);
    }

    public async Task<EmailSenderResult> SendVerificationEmail(string to, string receiverName, string verificationToken, long userId, CancellationToken ctoken = default)
    {
        return await SendEmail(to, receiverName, _options.VerificationTemplateId, verificationToken, userId, ctoken);
    }

    private async Task<EmailSenderResult> SendEmail(string to, string receiverName, string templateId, string verificationToken, long userId, CancellationToken ctoken = default)
    {
        var message = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            TemplateId = templateId,
        };

        message.AddTo(to);
        message.SetTemplateData(new
        {
            receiverName,
            verificationToken
        });

        Response response = await _client.SendEmailAsync(message, ctoken);
        string body = await response.Body.ReadAsStringAsync(ctoken);
        EmailProviderErrorResponse? parsedResponse = null;

        if (response.IsSuccessStatusCode)
        {
            EmailDelivery emailStatus = new EmailDelivery
            {
                Id = userId,
                ExternalId = Guid.NewGuid(),
                Status = EmailDeliveryStatus.Sent.ToString(),
                SentAt = _dateTimeProvider.Now
            };

            await _emailDeliveryRepository.SaveChanges(emailStatus);
            return EmailSenderResult.Ok();
        }

        string? correlationId = response.Headers.Contains("X-Correlation-ID")
            ? response.Headers.GetValues("X-Correlation-ID").FirstOrDefault()
            : null;

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId ?? string.Empty,
            ["StatusCode"] = response.StatusCode
        }))
        {
            Exception exception = response.StatusCode switch
            {
                HttpStatusCode.BadRequest => new InvalidRecipientException(),

                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationFailedException(),

                HttpStatusCode.InternalServerError or HttpStatusCode.ServiceUnavailable => new EmailServiceUnavailableException(),

                HttpStatusCode.TooManyRequests => new RateLimitedException(),

                HttpStatusCode.RequestTimeout => new TransientEmailException(),

                _ => new UnexpectedOperationException()
            };

            try
            {
                parsedResponse = JsonSerializer.Deserialize<EmailProviderErrorResponse>(body);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex,
                    "Failed to parse provider error response.");
            }

            _logger.LogWarning(exception,
                "Email service returned {StatusCode}. Errors: {Errors}",
                response.StatusCode,
                parsedResponse?.Errors.Select(e => $"{e.Field}: {e.Message}")
                .ToArray());

            EmailDelivery emailStatus = new EmailDelivery
            {
                Id = userId,
                ExternalId = Guid.NewGuid(),
                Status = EmailDeliveryStatus.Failed.ToString(),
                SentAt = _dateTimeProvider.Now
            };

            await _emailDeliveryRepository.SaveChanges(emailStatus);
            throw exception;
        }
    }
}
