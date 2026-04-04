using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Emails.Enums;
using ExpenseTracker.Application.Emails.Exceptions;
using ExpenseTracker.Application.Emails.Services;
using ExpenseTracker.Domain.Email.Repository;
using ExpenseTracker.Infrastructure.Emails.Exceptions;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Emails.Jobs;

public sealed class SendEmailJob
{
    private readonly IEmailService _emailService;
    private readonly IEmailDeliveryRepository _emailDeliveryRepository;
    private readonly IDateProvider _dateProvider;
    private readonly ILogger<SendEmailJob> _logger;
    public SendEmailJob(
        IEmailService emailService, 
        IEmailDeliveryRepository emailDeliveryRepository, 
        IDateProvider dateProvider,
        ILogger<SendEmailJob> logger
        )
    {
        _emailService = emailService;
        _emailDeliveryRepository = emailDeliveryRepository;
        _dateProvider = dateProvider;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 10, 30, 60 })]
    public async Task RetryPasswordResetEmail(string to, string receiver, string verificationToken, long userId, IJobCancellationToken jobToken)
    {
        try
        {
            jobToken.ThrowIfCancellationRequested();
            await _emailService.SendPasswordResetEmail(to, receiver, verificationToken, userId, CancellationToken.None);

            string emailStatus = await _emailDeliveryRepository.GetEmailStatusByUserId(userId);
            if (emailStatus != EmailDeliveryStatus.Sent.ToString())
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["Status"] = emailStatus
                }))
                {
                    _logger.LogWarning("Password Reset email delivery failed for userId: {UserId} with status: {Status}", userId, emailStatus);
                    throw new EmailDeliveryFailedException($"Email delivery failed for userId: {userId}");
                }
            }
        }
        catch (InvalidRecipientException ex)
        {
            // Intentionally not rethrowing – non-retryable
        }
        catch (AuthenticationFailedException ex)
        {
            // Intentionally not rethrowing – non-retryable
        }
    }

    [AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 10, 30, 60 })]
    public async Task RetryVerificationEmail(string to, string receiver, string verificationToken, long userId, IJobCancellationToken jobToken)
    {
        try
        {
            jobToken.ThrowIfCancellationRequested();
            await _emailService.SendVerificationEmail(to, receiver, verificationToken, userId, CancellationToken.None);

            string emailStatus = await _emailDeliveryRepository.GetEmailStatusByUserId(userId);
            if (emailStatus != EmailDeliveryStatus.Sent.ToString())
            {
                using (_logger.BeginScope(new Dictionary<string, object>
                {
                    ["UserId"] = userId,
                    ["Status"] = emailStatus
                }))
                {
                    _logger.LogWarning("Password Reset email delivery failed for userId: {UserId} with status: {Status}", userId, emailStatus);
                    throw new EmailDeliveryFailedException($"Email delivery failed for userId: {userId}");
                }
            }
        }
        catch (InvalidRecipientException ex)
        {
            // Intentionally not rethrowing – non-retryable
        }
        catch (AuthenticationFailedException ex)
        {
            // Intentionally not rethrowing – non-retryable
        }
    }
}
