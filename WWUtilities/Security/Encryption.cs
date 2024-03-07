using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using ww.Utilities.Extensions;

namespace ww.Utilities.Security
{
    public static class Encryption
    {
        private static Lazy<ReadOnlyDictionary<string, KeePass.Credential>> _keePassCreds = new Lazy<ReadOnlyDictionary<string, KeePass.Credential>>(() => KeePass.GetCredentials("Encryption").ToReadOnlyDictionary(x => x.Title.ReadString(), x => x));
        private static ReadOnlyDictionary<string, KeePass.Credential> KeePassCreds => _keePassCreds.Value;


        public static byte[] GenerateKeyForAESGCM()
        {
            return AESGCM.NewKey();
        }

        public static string EncryptUsingAESGCM(string unencrypted, byte[] key = null)
        {
            return AESGCM.SimpleEncrypt(unencrypted, key ?? Convert.FromBase64String(KeePassCreds["AES Key"].Password.ReadString()));
        }
        public static byte[] EncryptUsingAESGCM(byte[] unencrypted, byte[] key = null)
        {
            return AESGCM.SimpleEncrypt(unencrypted, key ?? Convert.FromBase64String(KeePassCreds["AES Key"].Password.ReadString()));
        }

        public static string DecryptUsingAESGCM(string encrypted, byte[] key = null)
        {
            return AESGCM.SimpleDecrypt(encrypted, key ?? Convert.FromBase64String(KeePassCreds["AES Key"].Password.ReadString()));
        }
        public static byte[] DecryptUsingAESGCM(byte[] encrypted, byte[] key = null)
        {
            return AESGCM.SimpleDecrypt(encrypted, key ?? Convert.FromBase64String(KeePassCreds["AES Key"].Password.ReadString()));
        }

        // Adapted from https://stackoverflow.com/a/10366194 (also on https://codereview.stackexchange.com/questions/14892/)
        private static class AESGCM
        {
            private static readonly SecureRandom Random = new SecureRandom();

            //Preconfigured Encryption Parameters
            private const int NonceBitSize = 128;
            private const int MacBitSize = 128;
            private const int KeyBitSize = 256;

            //Preconfigured Password Key Derivation Parameters
            private const int SaltBitSize = 128;
            private const int Iterations = 10000;
            private const int MinPasswordLength = 12;


            /// <summary>
            /// Helper that generates a random new key on each call.
            /// </summary>
            /// <returns></returns>
            public static byte[] NewKey()
            {
                var key = new byte[KeyBitSize / 8];
                Random.NextBytes(key);
                return key;
            }

            /// <summary>
            /// Simple Encryption And Authentication (AES-GCM) of a UTF8 string.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="key">The key.</param>
            /// <param name="nonSecretPayload">Optional non-secret payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Secret Message Required!;secretMessage</exception>
            /// <remarks>
            /// Adds overhead of (Optional-Payload + BlockSize(16) + Message +  HMac-Tag(16)) * 1.33 Base64
            /// </remarks>
            public static string SimpleEncrypt(string secretMessage, byte[] key, byte[] nonSecretPayload = null)
            {
                if (secretMessage.IsNullOrBlank())
                    return "";

                byte[] plainText = Encoding.UTF8.GetBytes(secretMessage);
                byte[] cipherText = SimpleEncrypt(plainText, key, nonSecretPayload);
                return Convert.ToBase64String(cipherText);
            }

            /// <summary>
            /// Simple Decryption & Authentication (AES-GCM) of a UTF8 Message
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="key">The key.</param>
            /// <param name="nonSecretPayloadLength">Length of the optional non-secret payload.</param>
            /// <returns>Decrypted Message</returns>
            public static string SimpleDecrypt(string encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
            {
                if (encryptedMessage.IsNullOrBlank())
                    return "";

                byte[] cipherText = Convert.FromBase64String(encryptedMessage);
                byte[] plainText = SimpleDecrypt(cipherText, key, nonSecretPayloadLength);
                return plainText == null ? null : Encoding.UTF8.GetString(plainText);
            }

            /// <summary>
            /// Simple Encryption And Authentication (AES-GCM) of a UTF8 String
            /// using key derived from a password (PBKDF2).
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayload">The non secret payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// Adds additional non secret payload for key generation parameters.
            /// </remarks>
            public static string SimpleEncryptWithPassword(string secretMessage, string password, byte[] nonSecretPayload = null)
            {
                if (secretMessage.IsNullOrBlank())
                    return "";

                byte[] plainText = Encoding.UTF8.GetBytes(secretMessage);
                byte[] cipherText = SimpleEncryptWithPassword(plainText, password, nonSecretPayload);
                return Convert.ToBase64String(cipherText);
            }

            /// <summary>
            /// Simple Decryption and Authentication (AES-GCM) of a UTF8 message
            /// using a key derived from a password (PBKDF2)
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Encrypted Message Required!;encryptedMessage</exception>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// </remarks>
            public static string SimpleDecryptWithPassword(string encryptedMessage, string password, int nonSecretPayloadLength = 0)
            {
                if (encryptedMessage.IsNullOrBlank())
                    return "";

                byte[] cipherText = Convert.FromBase64String(encryptedMessage);
                byte[] plainText = SimpleDecryptWithPassword(cipherText, password, nonSecretPayloadLength);
                return plainText == null ? null : Encoding.UTF8.GetString(plainText);
            }

            /// <summary>
            /// Simple Encryption And Authentication (AES-GCM) of a UTF8 string.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="key">The key.</param>
            /// <param name="nonSecretPayload">Optional non-secret payload.</param>
            /// <returns>Encrypted Message</returns>
            /// <remarks>
            /// Adds overhead of (Optional-Payload + BlockSize(16) + Message +  HMac-Tag(16)) * 1.33 Base64
            /// </remarks>
            public static byte[] SimpleEncrypt(byte[] secretMessage, byte[] key, byte[] nonSecretPayload = null)
            {
                //User Error Checks
                if (key == null || key.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "key");

                if (secretMessage == null || secretMessage.Length == 0)
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                //Non-secret Payload Optional
                nonSecretPayload = nonSecretPayload ?? new byte[] { };

                //Using random nonce large enough not to repeat
                byte[] nonce = new byte[NonceBitSize / 8];
                Random.NextBytes(nonce, 0, nonce.Length);

                var cipher = new GcmBlockCipher(new AesEngine());
                var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
                cipher.Init(true, parameters);

                //Generate Cipher Text With Auth Tag
                var cipherText = new byte[cipher.GetOutputSize(secretMessage.Length)];
                int len = cipher.ProcessBytes(secretMessage, 0, secretMessage.Length, cipherText, 0);
                cipher.DoFinal(cipherText, len);

                //Assemble Message
                using (var combinedStream = new MemoryStream())
                {
                    using (var binaryWriter = new BinaryWriter(combinedStream))
                    {
                        //Prepend Authenticated Payload
                        binaryWriter.Write(nonSecretPayload);
                        //Prepend Nonce
                        binaryWriter.Write(nonce);
                        //Write Cipher Text
                        binaryWriter.Write(cipherText);
                    }
                    return combinedStream.ToArray();
                }
            }

            /// <summary>
            /// Simple Decryption & Authentication (AES-GCM) of a UTF8 Message
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="key">The key.</param>
            /// <param name="nonSecretPayloadLength">Length of the optional non-secret payload.</param>
            /// <returns>Decrypted Message</returns>
            public static byte[] SimpleDecrypt(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
            {
                //User Error Checks
                if (key == null || key.Length != KeyBitSize / 8)
                    throw new ArgumentException(String.Format("Key needs to be {0} bit!", KeyBitSize), "key");

                if (encryptedMessage == null || encryptedMessage.Length == 0)
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                using (var cipherStream = new MemoryStream(encryptedMessage))
                {
                    using (var cipherReader = new BinaryReader(cipherStream))
                    {
                        //Grab Payload
                        byte[] nonSecretPayload = cipherReader.ReadBytes(nonSecretPayloadLength);

                        //Grab Nonce
                        byte[] nonce = cipherReader.ReadBytes(NonceBitSize / 8);

                        var cipher = new GcmBlockCipher(new AesEngine());
                        var parameters = new AeadParameters(new KeyParameter(key), MacBitSize, nonce, nonSecretPayload);
                        cipher.Init(false, parameters);

                        //Decrypt Cipher Text
                        byte[] cipherText = cipherReader.ReadBytes(encryptedMessage.Length - nonSecretPayloadLength - nonce.Length);
                        byte[] plainText = new byte[cipher.GetOutputSize(cipherText.Length)];

                        try
                        {
                            int len = cipher.ProcessBytes(cipherText, 0, cipherText.Length, plainText, 0);
                            cipher.DoFinal(plainText, len);
                        }
                        catch (InvalidCipherTextException)
                        {
                            //Return null if it doesn't authenticate
                            return null;
                        }

                        return plainText;
                    }
                }
            }

            /// <summary>
            /// Simple Encryption And Authentication (AES-GCM) of a UTF8 String
            /// using key derived from a password.
            /// </summary>
            /// <param name="secretMessage">The secret message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayload">The non secret payload.</param>
            /// <returns>
            /// Encrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Must have a password of minimum length;password</exception>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// Adds additional non secret payload for key generation parameters.
            /// </remarks>
            public static byte[] SimpleEncryptWithPassword(byte[] secretMessage, string password, byte[] nonSecretPayload = null)
            {
                nonSecretPayload = nonSecretPayload ?? new byte[] { };

                //User Error Checks
                if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                    throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

                if (secretMessage == null || secretMessage.Length == 0)
                    throw new ArgumentException("Secret Message Required!", "secretMessage");

                var generator = new Pkcs5S2ParametersGenerator();

                //Use Random Salt to minimize pre-generated weak password attacks.
                var salt = new byte[SaltBitSize / 8];
                Random.NextBytes(salt);

                generator.Init(
                    PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
                    salt,
                    Iterations);

                //Generate Key
                var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

                //Create Full Non Secret Payload
                var payload = new byte[salt.Length + nonSecretPayload.Length];
                Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
                Array.Copy(salt, 0, payload, nonSecretPayload.Length, salt.Length);

                return SimpleEncrypt(secretMessage, key.GetKey(), payload);
            }

            /// <summary>
            /// Simple Decryption and Authentication of a UTF8 message
            /// using a key derived from a password
            /// </summary>
            /// <param name="encryptedMessage">The encrypted message.</param>
            /// <param name="password">The password.</param>
            /// <param name="nonSecretPayloadLength">Length of the non secret payload.</param>
            /// <returns>
            /// Decrypted Message
            /// </returns>
            /// <exception cref="System.ArgumentException">Must have a password of minimum length;password</exception>
            /// <remarks>
            /// Significantly less secure than using random binary keys.
            /// </remarks>
            public static byte[] SimpleDecryptWithPassword(byte[] encryptedMessage, string password, int nonSecretPayloadLength = 0)
            {
                //User Error Checks
                if (string.IsNullOrWhiteSpace(password) || password.Length < MinPasswordLength)
                    throw new ArgumentException(String.Format("Must have a password of at least {0} characters!", MinPasswordLength), "password");

                if (encryptedMessage == null || encryptedMessage.Length == 0)
                    throw new ArgumentException("Encrypted Message Required!", "encryptedMessage");

                var generator = new Pkcs5S2ParametersGenerator();

                //Grab Salt from Payload
                var salt = new byte[SaltBitSize / 8];
                Array.Copy(encryptedMessage, nonSecretPayloadLength, salt, 0, salt.Length);

                generator.Init(
                    PbeParametersGenerator.Pkcs5PasswordToBytes(password.ToCharArray()),
                    salt,
                    Iterations);

                //Generate Key
                var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

                return SimpleDecrypt(encryptedMessage, key.GetKey(), salt.Length + nonSecretPayloadLength);
            }
        }
    }
}
