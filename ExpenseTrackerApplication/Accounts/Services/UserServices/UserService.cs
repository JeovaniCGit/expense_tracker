using ErrorOr;
using ExpenseTracker.Application.Abstractions.DateTimeProvider;
using ExpenseTracker.Application.Abstractions.DbExceptionHandler;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Application.Accounts.Contracts.Responses;
using ExpenseTracker.Application.Accounts.Errors;
using ExpenseTracker.Application.Authorization.BCryptLib;
using ExpenseTracker.Application.Authorization.UserRoles.Enums;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Accounts.Repository;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Accounts.Services.UserServices;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHistoryRepository _passwordHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IValidator<AddUserRequestDto> _addUserValidator;
    private readonly IValidator<UpdateUserRequestDto> _updateUserValidator;
    private readonly IDateProvider _dateProvider;
    private readonly ICurrentUserService _currentUserService;

    public UserService(
        IUserRepository userRepository,
        IPasswordHistoryRepository passwordHistoryRepository,
        IPasswordHasher passwordHasher,
        IValidator<AddUserRequestDto> addUserValidator,
        IValidator<UpdateUserRequestDto> updateUserValidator,
        IDateProvider dateProvider,
        ICurrentUserService currentUserService
        )
    {
        _userRepository = userRepository;
        _passwordHistoryRepository = passwordHistoryRepository;
        _passwordHasher = passwordHasher;
        _addUserValidator = addUserValidator;
        _updateUserValidator = updateUserValidator;
        _dateProvider = dateProvider;
        _currentUserService = currentUserService;
    }

    public async Task<ErrorOr<AddUserResponseDto>> CreateUser(AddUserRequestDto request, CancellationToken ctoken = default)
    {
        await _addUserValidator.ValidateAndThrowAsync(request, ctoken);

        try
        {
            User newUser = new User
            {
                Firstname = request.Firstname,
                Lastname = request.Lastname,
                Email = request.Email,
                Password = _passwordHasher.Hash(request.Password),
                RoleId = (long)UserRoleEnum.RegularUser
            };

            User createdUser = await _userRepository.CreateUser(newUser, ctoken);

            return new AddUserResponseDto
            {
                ExternalId = createdUser.ExternalId,
                Firstname = createdUser.Firstname,
                Lastname = createdUser.Lastname,
                CreatedAt = createdUser.CreatedAt
            };

        } catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
        {
            return UserErrors.DuplicatedEntry;
        }
    }

    public async Task<ErrorOr<int>> DeleteUser(string externalId, CancellationToken ctoken = default)
    {
        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(externalId), ctoken);
        if (existingUser is null)
            return UserErrors.InvalidArgs;

        if (currentUser.RoleId != (int)UserRoleEnum.Admin 
            && currentUser!.ExternalId != Guid.Parse(externalId))
            return UserErrors.Forbidden;

        return await _userRepository.DeleteUser(existingUser, ctoken);
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

        return new GetUserResponseDto
        {
            UserExternalId = existingUser.ExternalId,
            Firstname = existingUser.Firstname,
            Lastname = existingUser.Lastname,
            Email = existingUser.Email
        };
    }

    public async Task<ErrorOr<int>> UpdateUser(UpdateUserRequestDto request, CancellationToken ctoken = default)
    {
        await _updateUserValidator.ValidateAndThrowAsync(request, ctoken);

        Guid currentUserExternalId = _currentUserService.UserExternalId;
        User? currentUser = await _userRepository.GetUserByExternalId(currentUserExternalId, ctoken);

        User? existingUser = await _userRepository.GetUserByExternalId(Guid.Parse(request.UserExternalId), ctoken);

        if (existingUser is null)
            return UserErrors.NotFound;

        if (currentUserExternalId != existingUser.ExternalId)
            return UserErrors.Forbidden;

        if (!string.IsNullOrEmpty(request.Password))
        {
            string newHashedPass = _passwordHasher.Hash(request.Password!);
            PasswordHistory? passwordHistory = await _passwordHistoryRepository.GetByPasswordHash(newHashedPass, ctoken);

            if (passwordHistory is not null)
                return UserErrors.InvalidPassword;
        }

        existingUser.Firstname = request.Firstname ?? existingUser.Firstname;
        existingUser.Lastname = request.Lastname ?? existingUser.Lastname;
        existingUser.Email = request.Email ?? existingUser.Email;
        existingUser.Password = request.Password is null ? string.Empty : _passwordHasher.Hash(request.Password);

        if (!string.IsNullOrEmpty(request.Password))
        {
            PasswordHistory newPasswordHistory = new PasswordHistory
            {
                UserId = existingUser.Id,
                PasswordHash = existingUser.Password,
                CreatedAt = _dateProvider.Now
            };

            try
            {
                await _passwordHistoryRepository.Add(newPasswordHistory, ctoken);
                existingUser.PasswordLastUpdated = _dateProvider.Now;

                return await _userRepository.UpdateUser(existingUser, ctoken);

            } catch (DbUpdateException ex) when (ex.IsUniqueConstraintViolation())
            {
                return UserErrors.DuplicatedEntry;
            }
        }

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
