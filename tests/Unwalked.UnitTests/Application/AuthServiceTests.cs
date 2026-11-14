using Unwalked.Application.Auth;
using Unwalked.Application.Errors;
using Unwalked.Domain.Common;
using Unwalked.UnitTests.Fakes;

namespace Unwalked.UnitTests.Application;

public class AuthServiceTests
{
    private const string Password = "long enough secret";

    private InMemoryUserRepository users = null!;
    private RecordingTokenIssuer tokens = null!;
    private AuthService service = null!;

    [SetUp]
    public void CreateService()
    {
        users = new InMemoryUserRepository();
        tokens = new RecordingTokenIssuer();
        service = new AuthService(users, new PrefixPasswordHasher(), tokens, new FixedClock());
    }

    [Test]
    public async Task Register_StoresNormalizedEmailHashedPasswordAndCreationTime()
    {
        var id = await service.RegisterAsync(" Walker@Example.LT ", Password, CancellationToken.None);

        var stored = users.Users["walker@example.lt"];
        Assert.Multiple(() =>
        {
            Assert.That(stored.Id, Is.EqualTo(id));
            Assert.That(stored.PasswordHash, Is.EqualTo("hashed:" + Password));
            Assert.That(stored.CreatedAt, Is.EqualTo(FixedClock.Default));
        });
    }

    [Test]
    public async Task Register_RejectsSecondAccountWithSameEmail()
    {
        await service.RegisterAsync("walker@example.lt", Password, CancellationToken.None);

        Assert.ThrowsAsync<ConflictException>(() => service.RegisterAsync("WALKER@example.lt", Password, CancellationToken.None));
    }

    [Test]
    public void Register_ValidatesPasswordBeforeStoring()
    {
        Assert.ThrowsAsync<DomainValidationException>(() => service.RegisterAsync("walker@example.lt", "short", CancellationToken.None));
        Assert.That(users.Users, Is.Empty);
    }

    [Test]
    public async Task Login_IssuesTokenForMatchingCredentials()
    {
        var id = await service.RegisterAsync("walker@example.lt", Password, CancellationToken.None);

        var token = await service.LoginAsync(" Walker@Example.lt", Password, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(token.Token, Is.EqualTo($"token-for-{id}"));
            Assert.That(tokens.IssuedFor.Single().Id, Is.EqualTo(id));
        });
    }

    [Test]
    public async Task Login_FailsWithWrongPassword()
    {
        await service.RegisterAsync("walker@example.lt", Password, CancellationToken.None);

        Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync("walker@example.lt", "wrong password!", CancellationToken.None));
        Assert.That(tokens.IssuedFor, Is.Empty);
    }

    [Test]
    public void Login_FailsWithUnknownEmailUsingSameError()
    {
        var error = Assert.ThrowsAsync<AuthenticationFailedException>(() => service.LoginAsync("nobody@example.lt", Password, CancellationToken.None));

        Assert.That(error!.Message, Is.EqualTo("Email or password is incorrect."));
    }
}
