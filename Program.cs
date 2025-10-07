using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ECDHDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            // Step 1: Alice generates her key pair
            using var alice = new ECDiffieHellmanCng(256); // Explicitly use 256-bit curve (P-256)

            // Alice's public key as byte array (for transmission)
            byte[] alicePublicKey = alice.PublicKey.ToByteArray();
            Console.WriteLine("Alice's Public Key (hex): " + BitConverter.ToString(alicePublicKey).Replace("-", ""));

            // Step 2: Bob generates his key pair
            using var bob = new ECDiffieHellmanCng(256);

            // Bob's public key as byte array (for transmission)
            byte[] bobPublicKey = bob.PublicKey.ToByteArray();
            Console.WriteLine("Bob's Public Key (hex): " + BitConverter.ToString(bobPublicKey).Replace("-", ""));

            // Step 3: Exchange public keys and derive shared secrets
            // Alice derives using Bob's public key (import byte[] to public key object)
            using var bobPublicKeyObj = ECDiffieHellmanCngPublicKey.FromByteArray(bobPublicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] aliceSharedSecret = alice.DeriveKeyMaterial(bobPublicKeyObj);
            Console.WriteLine("Alice's Shared Secret (hex): " + BitConverter.ToString(aliceSharedSecret).Replace("-", ""));

            // Bob derives using Alice's public key (import byte[] to public key object)
            using var alicePublicKeyObj = ECDiffieHellmanCngPublicKey.FromByteArray(alicePublicKey, CngKeyBlobFormat.EccPublicBlob);
            byte[] bobSharedSecret = bob.DeriveKeyMaterial(alicePublicKeyObj);
            Console.WriteLine("Bob's Shared Secret (hex): " + BitConverter.ToString(bobSharedSecret).Replace("-", ""));

            // Step 4: Verify the shared secrets match
            bool secretsMatch = aliceSharedSecret.SequenceEqual(bobSharedSecret);
            Console.WriteLine($"Shared secrets match: {secretsMatch}");

            // Step 5: Derive symmetric key from shared secret (same for both)
            byte[] symmetricKey = SHA256.HashData(aliceSharedSecret); // 32-byte AES-256 key
            Console.WriteLine("Derived Symmetric Key (hex): " + BitConverter.ToString(symmetricKey).Replace("-", ""));

            // Step 6: Alice encrypts a message
            string plaintext = "Hi, I am Alice.";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            Console.WriteLine($"Alice's Plaintext: {plaintext}");

            // Generate random IV (16 bytes for AES-CBC)
            byte[] iv = new byte[16];
            RandomNumberGenerator.Fill(iv);

            // Encrypt
            byte[] ciphertext;
            /* From System.Security.Cryptography we are using Aes:
                 Represents the abstract base class from which all implementations of the Advanced
                 Encryption Standard (AES) must inherit.
                 Initializes a new instance of the System.Security.Cryptography.Aes class.
                 Creates and Returns a cryptographic object that is used to perform the symmetric algorithm.*/
            using (var aes = Aes.Create())
            {
                aes.Key = symmetricKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7; // Handles any padding needs

                using var encryptor = aes.CreateEncryptor();
                ciphertext = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
            }

            // Simulate transmission: IV + Ciphertext (prepend IV to ciphertext)
            byte[] encryptedMessage = iv.Concat(ciphertext).ToArray();
            Console.WriteLine("Alice's Encrypted Message (IV + Ciphertext, hex): " + BitConverter.ToString(encryptedMessage).Replace("-", ""));

            // Step 7: Bob decrypts the message (using the same key and extracting IV)
            byte[] receivedIv = encryptedMessage.Take(16).ToArray(); // First 16 bytes are the IV
            byte[] receivedCiphertext = encryptedMessage.Skip(16).ToArray(); // Remaining bytes are the ciphertext

            byte[] decryptedBytes;
            using (var aes = Aes.Create())
            {
                aes.Key = symmetricKey; // Becouse, we know from Step 4 that Alice and Bob get the same symmetric key
                aes.IV = receivedIv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                decryptedBytes = decryptor.TransformFinalBlock(receivedCiphertext, 0, receivedCiphertext.Length);
            }

            string decryptedMessage = Encoding.UTF8.GetString(decryptedBytes);
            Console.WriteLine($"Bob's Decrypted Message: {decryptedMessage}");

            Console.WriteLine("Encryption demo complete. Press any key to exit.");
            Console.ReadKey();


        }
    }
}