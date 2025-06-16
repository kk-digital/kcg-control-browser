using LogUtility;
using Blake2Fast;
using Org.BouncyCastle.Crypto.Digests;

namespace Utility;

public class UtilityHash {
    
    // use for deduplication entries into parquet files
    // Implementing the interface IEquatable<DigestKey> serves an important purpose related to efficient and correct equality comparison
    public readonly struct DigestKey : IEquatable<DigestKey>
    {
        // Internal byte array holding the 32-byte digest
        private readonly byte[] _digestBytes;

        // Constructor validates input length and initializes the internal byte array
        public DigestKey(byte[] digestBytes)
        {
            if (digestBytes == null || digestBytes.Length != 32)
                throw new ArgumentException("Digest must be exactly 32 bytes");
            _digestBytes = digestBytes;
        }
    
        // Exposes the internal bytes as a ReadOnlySpan<byte> for safe, efficient access
        public ReadOnlySpan<byte> DigestBytes
        {
            get
            {
                return _digestBytes;
            }
        }

        // Implements type-specific equality check by comparing each byte of the digest
        public bool Equals(DigestKey other)
        {
            for (int i = 0; i < 32; i++)
                if (_digestBytes[i] != other._digestBytes[i]) return false;
            return true;
        }

        // Overrides object.Equals to use the strongly typed Equals method
        public override bool Equals(object obj) =>
            obj is DigestKey other && Equals(other);

        // Provides a hash code based on the digest bytes, using a common hash combining pattern
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < 32; i++)
                    hash = hash * 31 + _digestBytes[i];
                return hash;
            }
        }
    
        // Converts the digest bytes to a lowercase hex string for display or storage
        public string ToHexString()
        {
            // Convert the ReadOnlySpan<byte> to an array, then to hex string
            return Convert.ToHexString(DigestBytes.ToArray()).ToLowerInvariant();
        }
    }
    
    public static DigestKey HexStringToDigestKey(string hex)
    {
        // Check if the input hex string is null and throw an exception if so
        if (hex == null)
            throw new ArgumentNullException(nameof(hex));
    
        // Validate that the hex string length is exactly 64 characters (32 bytes * 2 hex chars per byte)
        if (hex.Length != 64)
            throw new ArgumentException("Hex string must be exactly 64 characters for a 32-byte digest.");

        // Allocate a byte array to hold the 32 bytes of the digest
        byte[] bytes = new byte[32];
    
        // Loop through each pair of hex characters to convert them into a byte
        for (int i = 0; i < 32; i++)
        {
            // Convert two hex characters at a time into a byte and store it in the array
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }
    
        // Create and return a new DigestKey instance initialized with the byte array
        return new DigestKey(bytes);
    }
    
    public static string GenerateBlake2b256Fast(byte[] fileBytes)
    {
        try
        {
            // Compute a 32-byte Blake2b hash from the input byte array (fileBytes)
            byte[] hash = Blake2b.ComputeHash(32, fileBytes);
        
            // Convert the raw hash bytes to a lowercase hex string for easy storage or display
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch (Exception ex)
        {
            // Log any exception that occurs during hashing
            LibLog.LogError(ex.Message);
        
            // Return an empty string on error to indicate failure without throwing
            return string.Empty;
        }
    }
    
    public static DigestKey GenerateBlake2b256FastDigestKey(byte[] fileBytes)
    {
        try
        {
            // Compute a 32-byte Blake2b hash from the input byte array (fileBytes)
            byte[] hash = Blake2b.ComputeHash(32, fileBytes);
        
            // Wrap the raw hash bytes in a DigestKey struct for efficient use as a dictionary key
            return new DigestKey(hash);
        }
        catch (Exception ex)
        {
            // Log any exception that occurs during hashing
            LibLog.LogError(ex.Message);
        
            // Return the default DigestKey (empty) on error to avoid breaking the flow
            return default;
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