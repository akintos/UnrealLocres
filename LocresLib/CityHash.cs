// Copyright (c) 2011 Google, Inc.
// Copyright (c) 2014 Atvaark
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// CityHash, by Geoff Pike and Jyrki Alakuijala
// CityHash C# Port, by Atvaark
//
// This file provides CityHash64() and related functions.
//
// It's probably possible to create even faster hash functions by
// writing a program that systematically explores some of the space of
// possible hash functions, by using SIMD instructions, or by
// compromising on hash quality.

using System;
using System.Text;

using uint8 = System.Byte;
using uint32 = System.UInt32;
using uint64 = System.UInt64;
using uint128 = LocresLib.UInt128;

namespace LocresLib
{
    /// <summary>
    ///     UE4 CityHash managed impl.
    ///     Based on Atvaark's managed CityHash 1.0.3 implementation.
    /// </summary>
    public static class CityHash
    {
        // Some primes between 2^63 and 2^64 for various uses.
        private const uint64 K0 = 0xc3a5c85c97cb3127;
        private const uint64 K1 = 0xb492b66fbe98f273;
        private const uint64 K2 = 0x9ae16a3b2f90404f;
        private const uint64 K3 = 0xc949d7c7509e6557;

        // Magic numbers for 32-bit hashing. Copied from Murmur3.
        private const uint32 C1 = 0xcc9e2d51;
        private const uint32 C2 = 0x1b873593;

        public static bool BigEndian { get; set; } = false;

        // Hash 128 input bits down to 64 bits of output.
        // This is intended to be a reasonably good hash function.
        private static uint64 Hash128To64(uint128 x)
        {
            // Murmur-inspired hashing.
            const ulong kMul = 0x9ddfea08eb382d69;
            ulong a = (x.Low ^ x.High)*kMul;
            a ^= (a >> 47);
            ulong b = (x.High ^ a)*kMul;
            b ^= (b >> 47);
            b *= kMul;
            return b;
        }

        private static uint32 ByteSwapUInt32(uint32 x)
        {
            return
                (x >> 24) |
                ((x & 0x00ff0000) >> 8) |
                ((x & 0x0000ff00) << 8) |
                (x << 24);
        }

        private static uint64 ByteSwapUInt64(uint64 x)
        {
            return
                (x >> 56) |
                ((x & 0x00ff000000000000UL) >> 40) |
                ((x & 0x0000ff0000000000UL) >> 24) |
                ((x & 0x000000ff00000000UL) >> 8) |
                ((x & 0x00000000ff000000UL) << 8) |
                ((x & 0x0000000000ff0000UL) << 24) |
                ((x & 0x000000000000ff00UL) << 40) |
                (x << 56);
        }

        private static uint64 Fetch64(byte[] p, int index)
        {
            uint64 x = BitConverter.ToUInt64(p, index);
            return BigEndian ? ByteSwapUInt64(x) : x;
        }

        private static uint64 Fetch64(byte[] p, uint index)
        {
            return Fetch64(p, (int) index);
        }

        private static uint32 Fetch32(byte[] p, int index)
        {
            uint32 x = BitConverter.ToUInt32(p, index);
            return BigEndian ? ByteSwapUInt32(x) : x;
        }

        private static uint32 Fetch32(byte[] p, uint index)
        {
            return Fetch32(p, (int) index);
        }

        // A 32-bit to 32-bit integer hash copied from Murmur3.
        private static uint32 Fmix(uint32 h)
        {
            h ^= h >> 16;
            h *= 0x85ebca6b;
            h ^= h >> 13;
            h *= 0xc2b2ae35;
            h ^= h >> 16;
            return h;
        }

        private static uint32 Rotate32(uint32 val, int shift)
        {
            // Avoid shifting by 32: doing so yields an undefined result.
            return shift == 0 ? val : ((val >> shift) | (val << (32 - shift)));
        }

        private static void Permute3<T>(ref T a, ref T b, ref T c)
        {
            Swap(ref a, ref b);
            Swap(ref a, ref c);
        }

        private static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;
            a = b;
            b = temp;
        }

        private static uint32 Mur(uint32 a, uint32 h)
        {
            // Helper from Murmur3 for combining two 32-bit values.
            a *= C1;
            a = Rotate32(a, 17);
            a *= C2;
            h ^= a;
            h = Rotate32(h, 19);
            return h*5 + 0xe6546b64;
        }

        private static uint32 Hash32Len13To24(byte[] s)
        {
            uint len = (uint) s.Length;
            uint32 a = Fetch32(s, (len >> 1) - 4);
            uint32 b = Fetch32(s, 4);
            uint32 c = Fetch32(s, len - 8);
            uint32 d = Fetch32(s, len >> 1);
            uint32 e = Fetch32(s, 0);
            uint32 f = Fetch32(s, len - 4);
            uint32 h = len;
            return Fmix(Mur(f, Mur(e, Mur(d, Mur(c, Mur(b, Mur(a, h)))))));
        }

        private static uint32 Hash32Len0To4(byte[] s)
        {
            uint len = (uint) s.Length;
            uint32 b = 0;
            uint32 c = 9;
            for (int i = 0; i < len; i++)
            {
                b = b*C1 + (uint) ((sbyte) s[i]);
                c ^= b;
            }
            return Fmix(Mur(b, Mur(len, c)));
        }

        private static uint32 Hash32Len5To12(byte[] s)
        {
            uint len = (uint) s.Length;
            uint32 a = len, b = len*5, c = 9, d = b;
            a += Fetch32(s, 0);
            b += Fetch32(s, len - 4);
            c += Fetch32(s, ((len >> 1) & 4));
            return Fmix(Mur(c, Mur(b, Mur(a, d))));
        }

        // Hash function for a string. Most useful in 32-bit binaries.
        public static uint32 CityHash32(string s)
        {
            return CityHash32(s, Encoding.Default);
        }

        // Hash function for a string. Most useful in 32-bit binaries.
        public static uint32 CityHash32(string s, Encoding encoding)
        {
            return CityHash32(encoding.GetBytes(s));
        }

        public static uint32 CityHash32(byte[] s)
        {
            uint len = (uint) s.Length;
            if (len <= 24)
            {
                return len <= 12
                    ? (len <= 4 ? Hash32Len0To4(s) : Hash32Len5To12(s))
                    : Hash32Len13To24(s);
            }
            // len > 24
            uint32 h = len, g = C1*len, f = g;
            {
                uint32 a0 = Rotate32(Fetch32(s, len - 4)*C1, 17)*C2;
                uint32 a1 = Rotate32(Fetch32(s, len - 8)*C1, 17)*C2;
                uint32 a2 = Rotate32(Fetch32(s, len - 16)*C1, 17)*C2;
                uint32 a3 = Rotate32(Fetch32(s, len - 12)*C1, 17)*C2;
                uint32 a4 = Rotate32(Fetch32(s, len - 20)*C1, 17)*C2;
                h ^= a0;
                h = Rotate32(h, 19);
                h = h*5 + 0xe6546b64;
                h ^= a2;
                h = Rotate32(h, 19);
                h = h*5 + 0xe6546b64;
                g ^= a1;
                g = Rotate32(g, 19);
                g = g*5 + 0xe6546b64;
                g ^= a3;
                g = Rotate32(g, 19);
                g = g*5 + 0xe6546b64;
                f += a4;
                f = Rotate32(f, 19);
                f = f*5 + 0xe6546b64;
            }
            uint iters = (len - 1)/20;
            uint offset = 0;
            do
            {
                uint32 a0 = Rotate32(Fetch32(s, offset)*C1, 17)*C2;
                uint32 a1 = Fetch32(s, offset + 4);
                uint32 a2 = Rotate32(Fetch32(s, offset + 8)*C1, 17)*C2;
                uint32 a3 = Rotate32(Fetch32(s, offset + 12)*C1, 17)*C2;
                uint32 a4 = Fetch32(s, offset + 16);
                h ^= a0;
                h = Rotate32(h, 18);
                h = h*5 + 0xe6546b64;
                f += a1;
                f = Rotate32(f, 19);
                f = f*C1;
                g += a2;
                g = Rotate32(g, 18);
                g = g*5 + 0xe6546b64;
                h ^= a3 + a1;
                h = Rotate32(h, 19);
                h = h*5 + 0xe6546b64;
                g ^= a4;
                g = ByteSwapUInt32(g)*5;
                h += a4*5;
                h = ByteSwapUInt32(h);
                f += a0;
                Permute3(ref f, ref h, ref g);
                offset += 20;
            } while (--iters != 0);
            g = Rotate32(g, 11)*C1;
            g = Rotate32(g, 17)*C1;
            f = Rotate32(f, 11)*C1;
            f = Rotate32(f, 17)*C1;
            h = Rotate32(h + g, 19);
            h = h*5 + 0xe6546b64;
            h = Rotate32(h, 17)*C1;
            h = Rotate32(h + f, 19);
            h = h*5 + 0xe6546b64;
            h = Rotate32(h, 17)*C1;
            return h;
        }

        // Bitwise right rotate. Normally this will compile to a single
        // instruction, especially if the shift is a manifest constant.
        private static uint64 Rotate(uint64 val, int shift)
        {
            // Avoid shifting by 64: doing so yields an undefined result.
            return shift == 0 ? val : ((val >> shift) | (val << (64 - shift)));
        }

        private static uint64 ShiftMix(uint64 val)
        {
            return val ^ (val >> 47);
        }

        private static uint64 HashLen16(uint64 u, uint64 v)
        {
            return Hash128To64(new uint128(u, v));
        }

        private static uint64 HashLen16(uint64 u, uint64 v, uint64 mul)
        {
            // Murmur-inspired hashing.
            uint64 a = (u ^ v)*mul;
            a ^= (a >> 47);
            uint64 b = (v ^ a)*mul;
            b ^= (b >> 47);
            b *= mul;
            return b;
        }

        private static uint64 HashLen0To16(byte[] s, int offset)
        {
            int len = s.Length - offset;
            if (len >= 8)
            {
                uint64 mul = K2 + (uint)len * 2;
                uint64 a = Fetch64(s, offset) + K2;
                uint64 b = Fetch64(s, offset + len - 8);
                uint64 c = Rotate(b, 37) * mul + a;
                uint64 d = (Rotate(a, 25) + b) * mul;
                return HashLen16(c, d, mul);
            }
            if (len >= 4)
            {
                uint64 mul = K2 + (uint)len * 2;
                uint64 a = Fetch32(s, offset);
                return HashLen16((uint) len + (a << 3), Fetch32(s, offset + len - 4), mul); // added mul
            }
            if (len > 0)
            {
                uint8 a = s[offset];
                uint8 b = s[offset + (len >> 1)];
                uint8 c = s[offset + (len - 1)];
                uint32 y = a + ((uint32) b << 8);
                uint32 z = (uint) len + ((uint32) c << 2);
                return ShiftMix(y*K2 ^ z*K0)*K2;  // z*K3 -> z*K0 (why?)
            }
            return K2;
        }

        // This probably works well for 16-byte strings as well, but it may be overkill
        // in that case.
        private static uint64 HashLen17To32(byte[] s)
        {
            uint len = (uint) s.Length;
            uint64 mul = K2 + len * 2;
            uint64 a = Fetch64(s, 0)*K1;
            uint64 b = Fetch64(s, 8);
            uint64 c = Fetch64(s, len - 8) * mul;
            uint64 d = Fetch64(s, len - 16) * K2;
            return HashLen16(Rotate(a + b, 43) + Rotate(c, 30) + d,
                a + Rotate(b + K2, 18) + c, mul);
        }

        // Return a 16-byte hash for 48 bytes. Quick and dirty.
        // Callers do best to use "random-looking" values for a and b.
        private static uint128 WeakHashLen32WithSeeds(
            uint64 w, uint64 x, uint64 y, uint64 z, uint64 a, uint64 b)
        {
            a += w;
            b = Rotate(b + a + z, 21);
            uint64 c = a;
            a += x;
            a += y;
            b += Rotate(a, 44);
            return new uint128(a + z, b + c);
        }

        // Return a 16-byte hash for s[0] ... s[31], a, and b. Quick and dirty.
        private static uint128 WeakHashLen32WithSeeds(byte[] s, int offset, uint64 a, uint64 b)
        {
            return WeakHashLen32WithSeeds(Fetch64(s, offset),
                Fetch64(s, offset + 8),
                Fetch64(s, offset + 16),
                Fetch64(s, offset + 24),
                a,
                b);
        }

        // Return an 8-byte hash for 33 to 64 bytes.
        private static uint64 HashLen33To64(byte[] s)
        {
            uint len = (uint)s.Length;

            uint64 mul = K2 + len * 2;
            uint64 a = Fetch64(s, 0) * K2;
            uint64 b = Fetch64(s, 8);
            uint64 c = Fetch64(s, len - 24);
            uint64 d = Fetch64(s, len - 32);
            uint64 e = Fetch64(s, 16) * K2;
            uint64 f = Fetch64(s, 24) * 9;
            uint64 g = Fetch64(s, len - 8);
            uint64 h = Fetch64(s, len - 16) * mul;
            uint64 u = Rotate(a + g, 43) + (Rotate(b, 30) + c) * 9;
            uint64 v = ((a + g) ^ d) + f + 1;
            uint64 w = ByteSwapUInt64((u + v) * mul) + h;
            uint64 x = Rotate(e + f, 42) + c;
            uint64 y = (ByteSwapUInt64((v + w) * mul) + g) * mul;
            uint64 z = e + f + c;
            a = ByteSwapUInt64((x + z) * mul + y) + b;
            b = ShiftMix((z + a) * mul + d + h) * mul;
            return b + x;
        }

        public static uint64 CityHash64(string s)
        {
            return CityHash64(s, Encoding.Default);
        }

        public static uint64 CityHash64(string s, Encoding encoding)
        {
            return CityHash64(encoding.GetBytes(s));
        }

        public static uint64 CityHash64(byte[] s)
        {
            int len = s.Length;
            if (len <= 32)
            {
                if (len <= 16)
                {
                    return HashLen0To16(s, 0);
                }
                return HashLen17To32(s);
            }
            if (len <= 64)
            {
                return HashLen33To64(s);
            }


            // For strings over 64 bytes we hash the end first, and then as we
            // loop we keep 56 bytes of state: v, w, x, y, and z.
            uint64 x = Fetch64(s, len - 40);
            uint64 y = Fetch64(s, len - 16) + Fetch64(s, len - 56);
            uint64 z = HashLen16(Fetch64(s, len - 48) + (ulong) len, Fetch64(s, len - 24));
            uint128 v = WeakHashLen32WithSeeds(s, len - 64, (ulong) len, z);
            uint128 w = WeakHashLen32WithSeeds(s, len - 32, y + K1, x);
            x = x*K1 + Fetch64(s, 0);

            // Decrease len to the nearest multiple of 64, and operate on 64-byte chunks.
            len = (s.Length - 1) & ~63;
            int offset = 0;
            do
            {
                x = Rotate(x + y + v.Low + Fetch64(s, offset + 8), 37)*K1;
                y = Rotate(y + v.High + Fetch64(s, offset + 48), 42)*K1;
                x ^= w.High;
                y += v.Low + Fetch64(s, offset + 40);
                z = Rotate(z + w.Low, 33)*K1;
                v = WeakHashLen32WithSeeds(s, offset, v.High*K1, x + w.Low);
                w = WeakHashLen32WithSeeds(s, offset + 32, z + w.High, y + Fetch64(s, offset + 16));
                Swap(ref z, ref x);
                offset += 64;
                len -= 64;
            } while (len != 0);
            return HashLen16(HashLen16(v.Low, w.Low) + ShiftMix(y)*K1 + z,
                HashLen16(v.High, w.High) + x);
        }

        // Hash function for a string. For convenience, a 64-bit seed is also
        // hashed into the result.
        public static uint64 CityHash64WithSeed(string s, uint64 seed)
        {
            return CityHash64WithSeed(s, seed, Encoding.Default);
        }

        // Hash function for a string. For convenience, a 64-bit seed is also
        // hashed into the result.
        public static uint64 CityHash64WithSeed(string s, uint64 seed, Encoding encoding)
        {
            return CityHash64WithSeed(encoding.GetBytes(s), seed);
        }

        public static uint64 CityHash64WithSeed(byte[] s, uint64 seed)
        {
            return CityHash64WithSeeds(s, K2, seed);
        }

        // Hash function for a byte array. For convenience, two seeds are also
        // hashed into the result.
        public static uint64 CityHash64WithSeeds(string s, uint64 seed0, uint64 seed1)
        {
            return CityHash64WithSeeds(s, seed0, seed1, Encoding.Default);
        }

        // Hash function for a string. For convenience, two seeds are also
        // hashed into the result.
        public static uint64 CityHash64WithSeeds(string s, uint64 seed0, uint64 seed1, Encoding encoding)
        {
            return CityHash64WithSeeds(encoding.GetBytes(s), seed0, seed1);
        }

        public static uint64 CityHash64WithSeeds(byte[] s, uint64 seed0, uint64 seed1)
        {
            return HashLen16(CityHash64(s) - seed0, seed1);
        }
    }

    public class UInt128
    {
        public UInt128(UInt64 low, UInt64 high)
        {
            Low = low;
            High = high;
        }

        public UInt64 Low { get; set; }
        public UInt64 High { get; set; }

        protected bool Equals(UInt128 other)
        {
            return Low == other.Low && High == other.High;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((UInt128)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Low.GetHashCode() * 397) ^ High.GetHashCode();
            }
        }
    }
}