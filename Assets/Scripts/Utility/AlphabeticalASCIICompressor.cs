using System;
using System.Text;
using System.Runtime.CompilerServices;

/// <summary>
/// Deprecated. Do not use. Or do, it still works, but currently has no purpose.
/// </summary>
public static class AlphabeticalASCIICompressor {

    /// <summary>
    /// Compresses a string by reducing the char size to 6 bits and placing ten of them in each long.
    /// String input must only contain alphabetical ASCII characters.
    /// </summary>
    /// <param name="str">string to be compressed</param>
    /// <returns>A long array, each long containing ten compressed chars within it</returns>
    /// <exception cref="Exception"></exception>
    public static long[] Compress(string str) {
        long[] output = new long[Ceiling(str.Length / 10f)];

        int longCounter = 0;
        int innerLongCounter = 0;
        foreach (char c in str) {
            if (!IsAsciiLetter(c))
                throw new Exception($"Invoke string ID ({str}) must only contain ASCII alphabetical characters");

            if (innerLongCounter >= 10) {
                ++longCounter;
                innerLongCounter = 0;
            }

            long compressedChar = c & 0b111111;         // captures the bits that matter (first/rightmost six bits) using a bitmask
            output[longCounter] |= compressedChar << (6 * (9 - innerLongCounter));
            ++innerLongCounter;
        }

        return output;
    }

    /// <summary>
    /// Decompresses a compressed long array back into its string form.
    /// Should only be used on long arrays returned from AlphabeticalASCIICompressor.CompressString(string).
    /// </summary>
    /// <param name="longs">long array to decompress</param>
    /// <returns>The original string that was compressed using Compress(string)</returns>
    public static string Decompress(long[] longs) {
        StringBuilder sb = new();

        foreach (long l in longs) {
            for (int i = 9; i >= 0; --i) {
                long current = (l >> (6 * i)) & 0b111111;        // captures the 6-bit compressed character at that position in the long

                // means it's not an alphabetic character (empty/end of string)
                if (current == 0) {
                    break;
                }

                current += 0b01000000;                          // adds back the ASCII character 0b01 prefix

                sb.Append((char)current);
            }
        }

        return sb.ToString();
    }

    // more bespoke, efficient ceiling function for non-negatives
    [MethodImpl(MethodImplOptions.AggressiveInlining)]          // because Unity doesn't like to inline functions
    private static int Ceiling(float x) {
        int xi = (int)x;
        return (x > xi) ? xi + 1 : xi;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]          // because Unity doesn't like to inline functions
    private static bool IsAsciiLetter(char c) {
        return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }
}
