using ErrorOr;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Accounts.Services.UserServices;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IValidator<UpdateUserRequestDto> _updateUserValidator;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IValidator<UpdateUserRequestDto> updateUserValidator,
        ICurrentUserService currentUserService
        )
    {
        _userRepository = userRepository;
        _updateUserValidator = updateUserValidator;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<int>> DeleteUser(string? externalId = null, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;
        if (string.IsNullOrEmpty(externalId))
        {
            User? existingUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);
            if (currentUserExternalId != existingUser!.ExternalId)
                return UserErrors.InvalidArgs;
            
            return await _userRepository.DeleteUser(existingUser!, ctoken);
        }
        else
        {
            User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(externalId), ctoken);
            if (currentUserExternalId != existingUser!.ExternalId)
                return UserErrors.InvalidArgs;
            
            return await _userRepository.DeleteUser(existingUser!, ctoken);
        }
    }

    public async Task<IEnumerable<GetAllUsersResponseDto>> GetAllUsers(int page, int pageSize, CancellationToken ctoken = default)
    {
        IEnumerable<User> users = await _userRepository.GetAllUsers(page, pageSize, ctoken);

        return users.Select(u => new GetAllUsersResponseDto
        {
            UserExternalId = u.ExternalId,
            Firstname = u.Firstname,
            Lastname = u.Lastname,
            Email = u.Email
        });
    }

    public async Task<ErrorOr<GetUserResponseDto>> GetUserByExternalId(CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? existingUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);
        if (currentUserExternalId != existingUser!.ExternalId)
            return UserErrors.InvalidArgs;

        return new GetUserResponseDto
        {
            UserExternalId = existingUser!.ExternalId,
            Firstname = existingUser.Firstname,
            Lastname = existingUser.Lastname,
            Email = existingUser.Email,
            Version = existingUser.Version
        };
    }

    public async Task<ErrorOr<int>> UpdateUser(UpdateUserRequestDto request, CancellationToken ctoken = default)
    {
        await _updateUserValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? existingUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        if (currentUserExternalId != existingUser!.ExternalId)
            return UserErrors.InvalidArgs;

        existingUser.Firstname = request.Firstname ?? existingUser.Firstname;
        existingUser.Lastname = request.Lastname ?? existingUser.Lastname;
        existingUser.Email = request.Email ?? existingUser.Email;

        try
        {
            return await _userRepository.UpdateUser(existingUser, ctoken);

        }
        catch (DbUpdateConcurrencyException ex)
        {
            return UserErrors.ConcurrencyConflict;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return UserErrors.DuplicatedEntry; 
        }
    }

    private async Task<ErrorOr<User>> GetUserByEmail(string email, CancellationToken ctoken = default)
    {
        User? existingUser = await _userRepository.GetUserByEmail(email, ctoken);

        if (existingUser is null)
            return UserErrors.NotFound;

        return existingUser;
    }
}
