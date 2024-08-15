using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace VaultService;

public class VaultDatabase : DbContext
{
    protected readonly IConfiguration Configuration;
    
    public VaultDatabase(IConfiguration configuration)
    {
        Configuration = configuration;
    }
    
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Configuration.GetConnectionString("VaultDatabase"));
    }
    
    public DbSet<VaultUser> Users { get; set; }
}
public class VaultUser
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string Salt { get; set; }
}

public interface IUserService
{
    Task<bool> ValidateUserAsync(string username, string password);
    Task<bool> CreateUserAsync(string username, string password);
    // Task TestConnectionAsync();
}

public class UserService : IUserService
{
    private readonly IDbContextFactory<VaultDatabase> _dbContextFactory;
    
    public UserService(IDbContextFactory<VaultDatabase> dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
    
    public async Task<VaultUser?> GetUserAsync(string username)
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();
        return await dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    
    public async Task<bool> ValidateUserAsync(string username, string password)
    {
        var user = await GetUserAsync(username);
        var salt = Convert.FromBase64String(user?.Salt ?? throw new InvalidOperationException());
        var derivedPassword = new Rfc2898DeriveBytes(password, salt, 100, HashAlgorithmName.SHA256);
        var passwordHash = Convert.ToBase64String(derivedPassword.GetBytes(32));
        return user.Password == passwordHash;
    }
    
    
    public async Task<bool> CreateUserAsync(string username, string password)
    {
        try
        {
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            
            //Derive a password from the user password using the salt and 100 iterations of SHA256.
            var derivedPassword = new Rfc2898DeriveBytes(password, salt, 100, HashAlgorithmName.SHA256);
            
            //Convert the salt and derived password to strings
            var passwordSalt = Convert.ToBase64String(salt);
            var passwordHash = Convert.ToBase64String(derivedPassword.GetBytes(32));
            
            var user = new VaultUser
            {
                Username = username,
                Password = passwordHash,
                Salt = passwordSalt
            };
            
            await using var dbContext = _dbContextFactory.CreateDbContext();
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
    
    // public async Task TestConnectionAsync()
    // {
    //     await using var dbContext = _dbContextFactory.CreateDbContext();
    //     await dbContext.Database.EnsureCreatedAsync();
    //     Console.WriteLine("Database connected successfully.");
    //     await dbContext.DisposeAsync();
    // }
}