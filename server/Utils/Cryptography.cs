using System;

namespace StdbModule.Utils.Cryptography;

public static class Sha256
{
    private static readonly uint[] _k = {
        0x428a2f98, 0x71374491, 0xb5c0fbcf, 0xe9b5dba5,
        0x3956c25b, 0x59f111f1, 0x923f82a4, 0xab1c5ed5,
        0xd807aa98, 0x12835b01, 0x243185be, 0x550c7dc3,
        0x72be5d74, 0x80deb1fe, 0x9bdc06a7, 0xc19bf174,
        0xe49b69c1, 0xefbe4786, 0x0fc19dc6, 0x240ca1cc,
        0x2de92c6f, 0x4a7484aa, 0x5cb0a9dc, 0x76f988da,
        0x983e5152, 0xa831c66d, 0xb00327c8, 0xbf597fc7,
        0xc6e00bf3, 0xd5a79147, 0x06ca6351, 0x14292967,
        0x27b70a85, 0x2e1b2138, 0x4d2c6dfc, 0x53380d13,
        0x650a7354, 0x766a0abb, 0x81c2c92e, 0x92722c85,
        0xa2bfe8a1, 0xa81a664b, 0xc24b8b70, 0xc76c51a3,
        0xd192e819, 0xd6990624, 0xf40e3585, 0x106aa070,
        0x19a4c116, 0x1e376c08, 0x2748774c, 0x34b0bcb5,
        0x391c0cb3, 0x4ed8aa4a, 0x5b9cca4f, 0x682e6ff3,
        0x748f82ee, 0x78a5636f, 0x84c87814, 0x8cc70208,
        0x90befffa, 0xa4506ceb, 0xbef9a3f7, 0xc67178f2
    };

    public static string ComputeHash(string input)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(input);
        byte[] hash = ComputeHash(data);
        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    public static byte[] ComputeHash(byte[] data)
    {
        uint[] h = {
            0x6a09e667,
            0xbb67ae85,
            0x3c6ef372,
            0xa54ff53a,
            0x510e527f,
            0x9b05688c,
            0x1f83d9ab,
            0x5be0cd19
        };

        int originalLength = data.Length;
        int padLength = (56 - (originalLength + 1) % 64 + 64) % 64;
        byte[] padded = new byte[originalLength + 1 + padLength + 8];
        Array.Copy(data, padded, originalLength);
        padded[originalLength] = 0x80;
        ulong bitLength = (ulong)originalLength * 8;
        for (int i = 0; i < 8; i++)
        {
            padded[padded.Length - 1 - i] = (byte)(bitLength >> (8 * i));
        }

        uint[] w = new uint[64];

        for (int chunk = 0; chunk < padded.Length; chunk += 64)
        {
            for (int i = 0; i < 16; i++)
            {
                w[i] = (uint)(padded[chunk + 4 * i] << 24 |
                              padded[chunk + 4 * i + 1] << 16 |
                              padded[chunk + 4 * i + 2] << 8 |
                              padded[chunk + 4 * i + 3]);
            }

            for (int i = 16; i < 64; i++)
            {
                uint s0 = RotR(w[i - 15], 7) ^ RotR(w[i - 15], 18) ^ (w[i - 15] >> 3);
                uint s1 = RotR(w[i - 2], 17) ^ RotR(w[i - 2], 19) ^ (w[i - 2] >> 10);
                w[i] = w[i - 16] + s0 + w[i - 7] + s1;
            }

            uint a = h[0], b = h[1], c = h[2], d = h[3];
            uint e = h[4], f = h[5], g = h[6], hVal = h[7];

            for (int i = 0; i < 64; i++)
            {
                uint S1 = RotR(e, 6) ^ RotR(e, 11) ^ RotR(e, 25);
                uint ch = (e & f) ^ (~e & g);
                uint temp1 = hVal + S1 + ch + _k[i] + w[i];
                uint S0 = RotR(a, 2) ^ RotR(a, 13) ^ RotR(a, 22);
                uint maj = (a & b) ^ (a & c) ^ (b ^ c);
                uint temp2 = S0 + maj;

                hVal = g;
                g = f;
                f = e;
                e = d + temp1;
                d = c;
                c = b;
                b = a;
                a = temp1 + temp2;
            }

            h[0] += a;
            h[1] += b;
            h[2] += c;
            h[3] += d;
            h[4] += e;
            h[5] += f;
            h[6] += g;
            h[7] += hVal;
        }

        byte[] result = new byte[32];
        for (int i = 0; i < 8; i++)
        {
            result[i * 4] = (byte)(h[i] >> 24);
            result[i * 4 + 1] = (byte)(h[i] >> 16);
            result[i * 4 + 2] = (byte)(h[i] >> 8);
            result[i * 4 + 3] = (byte)(h[i]);
        }

        return result;
    }

    private static uint RotR(uint x, int n) => (x >> n) | (x << (32 - n));
}
