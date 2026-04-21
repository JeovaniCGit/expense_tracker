using Bogus;
using ExpenseTracker.Application.Accounts.Contracts.Requests;
using ExpenseTracker.Domain.Accounts.Entity;
using ExpenseTracker.Domain.Authorization.UserRoles.Entity;
using ExpenseTracker.Domain.Collections.Entity;
using ExpenseTracker.Domain.Records.Entity;

namespace ExpenseTracker.IntegrationTests.Accounts.Builder;

public class UserBuilder
{
    private readonly Faker _faker = new Faker();
    private string _firstname;
    private string _lastname;
    private string _email;
    private string _password;
    private DateTimeOffset _passwordLastUpdated;
    private bool _isEmailVerified;
    private long _roleId = 3;
    private UserRole _role;
    private PasswordHistory _passwordHistory;
    private ICollection<TransactionRecord> _transactions;
    private ICollection<TransactionCollection> _collections;
    private Guid? _externalId;

    public UserBuilder()
    {
        _firstname = _faker.Name.FirstName();
        _lastname = _faker.Name.LastName();
        _email = _faker.Internet.Email();
        _password = GeneratePassword();
        _passwordLastUpdated = _faker.Date.PastOffset().ToUniversalTime();
        _isEmailVerified = false;
        _transactions = new List<TransactionRecord>();
        _collections = new List<TransactionCollection>();
    }

    public UserBuilder WithFirstname(string firstname)
    {
        _firstname = firstname;
        return this;
    }

    public UserBuilder WithLastname(string lastname)
    {
        _lastname = lastname;
        return this;
    }

    public UserBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public UserBuilder WithPassword(string password)
    {
        _password = password;
        return this;
    }

    public UserBuilder WithPasswordLastUpdated(DateTimeOffset passwordLastUpdated)
    {
        _passwordLastUpdated = passwordLastUpdated;
        return this;
    }

    public UserBuilder WithIsEmailVerified(bool isEmailVerified)
    {
        _isEmailVerified = isEmailVerified;
        return this;
    }

    public UserBuilder WithRoleId(long roleId)
    {
        _roleId = roleId;
        return this;
    }

    public UserBuilder WithRole(UserRole role)
    {
        _role = role;
        return this;
    }

    public UserBuilder WithPasswordHistory(PasswordHistory passwordHistory)
    {
        _passwordHistory = passwordHistory;
        return this;
    }

    public UserBuilder WithTransactions(ICollection<TransactionRecord> transactions)
    {
        _transactions = transactions;
        return this;
    }

    public UserBuilder WithCollections(ICollection<TransactionCollection> collections)
    {
        _collections = collections;
        return this;
    }

    public UserBuilder WithExternalId(Guid externalId)
    {
        _externalId = externalId;
        return this;
    }

    public AddUserRequestDto BuildCreateUserDto()
    {
        return new AddUserRequestDto
        {
            Firstname = _firstname,
            Lastname = _lastname,
            Email = _email,
            Password = GeneratePassword()
        };
    }

    public User Build()
    {
        return new User
        {
            ExternalId = _externalId ?? Guid.NewGuid(),
            Firstname = _firstname,
            Lastname = _lastname,
            Email = _email,
            Password = _password,
            PasswordLastUpdated = _passwordLastUpdated,
            IsEmailVerified = _isEmailVerified,
            RoleId = _roleId,
            Role = _role,
            PasswordHistory = _passwordHistory,
            Transactions = _transactions,
            Collections = _collections
        };
    }

    private string GeneratePassword()
    {
        var upper = _faker.Random.Char('A', 'Z');
        var digit = _faker.Random.Char('0', '9');
        var special = _faker.Random.Char('!', '/');
        var rest = _faker.Internet.Password(9, false);

        var password = $"{upper}{digit}{special}{rest}";

        // Shuffle so it's not predictable
        return new string(password.OrderBy(_ => _faker.Random.Int()).ToArray());
    }
}