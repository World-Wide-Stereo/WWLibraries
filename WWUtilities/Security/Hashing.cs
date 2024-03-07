using ww.Utilities.Extensions;

namespace ww.Utilities.Security
{
    public static class Hashing
    {
        /// <summary>
        /// Hash a password using the OpenBSD bcrypt scheme.
        /// </summary>
        /// <param name="password">The password to hash.</param>
        /// <param name="workFactor">The log2 of the number of rounds of hashing to apply - the work factor therefore increases as 2**workFactor.</param>
        /// <returns>The hashed password.</returns>
        public static string HashPassword(string password, int workFactor = 12)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(workFactor));
        }

        /// <summary>
        /// Verifies that the hash of the given password matches the provided hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="correctHash">The stored hash of the user's password.</param>
        /// <returns>true if the passwords match, false otherwise.</returns>
        public static bool ValidatePassword(string password, string correctHash)
        {
            return !correctHash.IsNullOrBlank() && BCrypt.Net.BCrypt.Verify(password, correctHash);
        }
    }
}
