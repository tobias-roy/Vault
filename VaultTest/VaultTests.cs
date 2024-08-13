using Xunit;
using VaultService;

namespace PasswordVaultTests
{
    public class VaultTests
    {
        private readonly IVaultManager VaultManager = new VaultManager();
        
        [Fact]
        public void TestEncryptDecrypt()
        {
            string originalText = "This is a test";
            string password = "password123";
            string encrypted = VaultManager.EncryptString(originalText, password);
            string decrypted = VaultManager.DecryptString(encrypted, password);

            Assert.Equal(originalText, decrypted);
        }

        [Fact]
        public void TestAddEntry()
        {
            var entries = new List<VaultEntry>();
            var entry = new VaultEntry { Website = "example.com", Username = "user", Password = "pass" };
            entries.Add(entry);

            Assert.Single(entries);
            Assert.Equal("example.com", entries[0].Website);
        }

        [Fact]
        public void TestSaveLoadEntries()
        {
            var entries = new List<VaultEntry>
            {
                new VaultEntry { Website = "example.com", Username = "user", Password = "pass" }
            };

            VaultManager.SaveEntries(entries);
            var loadedEntries = VaultManager.LoadEntries();

            Assert.Single(loadedEntries);
            Assert.Equal("example.com", loadedEntries[0].Website);
        }
    }
}