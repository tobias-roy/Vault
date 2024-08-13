using System;
using Microsoft.Extensions.DependencyInjection;
using VaultService;

namespace PasswordVault
{
    class Program
    {
        static void Main(string[] args)
        {
           
        }

        static void AddEntry()
        {
            Console.Write("Website: ");
            string website = Console.ReadLine();
            Console.Write("Username: ");
            string username = Console.ReadLine();
            Console.Write("Password: ");
            string password = Console.ReadLine();

            Console.WriteLine("Entry added.");
        }

        static void ViewEntries()
        {
            var entries = VaultManager.LoadEntries();

            foreach (var entry in entries)
            {
                Console.WriteLine($"Website: {entry.Website}, Username: {entry.Username}, Password: {entry.Password}");
            }
        }

        static void EncryptVault()
        {
            Console.Write("Enter password to encrypt: ");
            string password = Console.ReadLine();
            VaultManager.EncryptVault(password);
            Console.WriteLine("Vault encrypted.");
        }

        static void DecryptVault()
        {
            Console.Write("Enter password to decrypt: ");
            string password = Console.ReadLine();
            VaultManager.DecryptVault(password);
            Console.WriteLine("Vault decrypted.");
        }
    }
}
