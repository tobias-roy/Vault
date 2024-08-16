using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace VaultService
{
    public interface IVaultManager
    {
        void AddEntry(VaultEntry newEntries);
        List<VaultEntry> ViewEntries();
        void EncryptVault(string? password);
        void DecryptVault(string? password);
        List<VaultEntry> LoadEntries();
        void SaveEntries(List<VaultEntry> entries);
        string EncryptString(string plainText, string? password);
        string DecryptString(string cipherText, string? password);
    }

    public class VaultManager : IVaultManager
    {
        private readonly string _vaultFilePath = "vault.dat";

        /// <summary>
        /// Adds an object of the type VaultEntry to the vault.
        /// </summary>
        /// <param name="newEntry"></param>
        public void AddEntry(VaultEntry newEntry)
        {
            var entries = LoadEntries();
            entries.Add(newEntry);
            SaveEntries(entries);
        }

        /// <summary>
        /// Loads all entries from the vault.
        /// </summary>
        /// <returns></returns>
        public  List<VaultEntry> ViewEntries()
        {
            return LoadEntries();
        }

        /// <summary>
        /// Encrypts the vault using the provided password.
        /// Deletes the plain text file after encryption.
        /// </summary>
        /// <param name="password"></param>
        public void EncryptVault(string? password)
        {
            var entries = LoadEntries();
            var json = JsonConvert.SerializeObject(entries);
            var encrypted = EncryptString(json, password);
            File.WriteAllText(_vaultFilePath, encrypted);
            File.Delete("vault.json"); // Delete the plain text file after encryption.
        }

        /// <summary>
        /// Decrypts the vault using the provided password.
        /// Writes the decrypted content to a new file.
        /// </summary>
        /// <param name="password"></param>
        public  void DecryptVault(string? password)
        {
            if (File.Exists(_vaultFilePath))
            {
                var encrypted = File.ReadAllText(_vaultFilePath);
                var decrypted = DecryptString(encrypted, password);
                File.WriteAllText("vault.json", decrypted);
            }
            else
            {
                File.Create("vault.json").Close();
            }
        }

        /// <summary>
        /// Loads the entries from the vault file.
        /// </summary>
        /// <returns>List of vault entries</returns>
        public  List<VaultEntry> LoadEntries()
        {
            if (!File.Exists("vault.json"))
                return new List<VaultEntry>();

            var json = File.ReadAllText("vault.json");
            return JsonConvert.DeserializeObject<List<VaultEntry>>(json) ?? new List<VaultEntry>();
        }

        /// <summary>
        /// Saves the vault entries to the vault file as plain text.
        /// Used before encryption.
        /// </summary>
        /// <param name="entries"></param>
        public  void SaveEntries(List<VaultEntry> entries)
        {
            var json = JsonConvert.SerializeObject(entries, Formatting.Indented);
            File.WriteAllText("vault.json", json);
        }

        /// <summary>
        /// Encrypts a string using AES encryption.
        /// </summary>
        /// <param name="plainText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string EncryptString(string plainText, string? password)
        {
            using var aes = Aes.Create();
            aes.GenerateIV();
            var iv = aes.IV;

            // Derive the key from the password
            var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SaltIsGoodForYou!")).GetBytes(32);
            // var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("HardSalt"), 100, HashAlgorithmName.SHA256).GetBytes(32);
            aes.Key = key;

            // Encrypt the data
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            ms.Write(iv, 0, iv.Length); // Prepend IV to the ciphertext
            using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
            using var sw = new StreamWriter(cs);
            sw.Write(plainText);
            sw.Close();

            return Convert.ToBase64String(ms.ToArray());
        }

        /// <summary>
        /// Decrypts a string using AES encryption.
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public string DecryptString(string cipherText, string password)
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            // Extract the IV from the beginning of the ciphertext
            var iv = new byte[16];
            Array.Copy(fullCipher, 0, iv, 0, iv.Length);

            // Extract the actual ciphertext
            var cipher = new byte[fullCipher.Length - iv.Length];
            Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            // Derive the key from the password
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("SaltIsGoodForYou!")).GetBytes(32);
            // var key = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes("HardSalt"), 100, HashAlgorithmName.SHA256).GetBytes(32);
            aes.Key = key;
            aes.IV = iv;

            // Decrypt the data
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(cipher);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);

            return sr.ReadToEnd();
        }
    }

    public class VaultEntry
    {
        public string Website { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
