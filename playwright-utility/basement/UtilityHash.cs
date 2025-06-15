using LogUtility;
using Blake2Fast;
using Org.BouncyCastle.Crypto.Digests;

namespace Utility;

public class UtilityHash {

    //========================================================================================================
// Solution 2 - Fastest
    public static string GenerateBlake2b256Fast(byte[] imageData)
    {
        try
        {
            byte[] hash = Blake2b.ComputeHash(32, imageData);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return string.Empty;
        }
    }
    
    public static string GenerateBlake2b256Hash(byte[] imageData)
    {
        try
        {
            // Use BouncyCastle to compute the BLAKE2b-256 hash
            Blake2bDigest blake2b = new(256); // 256-bit BLAKE2b

            // Pass the data to the digest
            blake2b.BlockUpdate(imageData, 0, imageData.Length);

            // Create the hash output buffer
            byte[] hash = new byte[blake2b.GetDigestSize()];

            // Compute the hash
            blake2b.DoFinal(hash, 0);

            // Convert the hash to a hex string
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
        catch (Exception ex)
        {
            LibLog.LogError(ex.Message);
            return string.Empty;
        }
    }
    //========================================================================================================
}