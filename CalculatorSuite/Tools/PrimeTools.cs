using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace CalculatorSuite.Tools;

public static class PrimeTools
{
    /// <summary>
    /// Uses the Sieve of Eratosthenes to list all prime numbers up to the target.
    /// </summary>
    /// <remarks>
    /// Uses a limit of 1,000,000 to avoid excessive memory usage.
    /// </remarks>
    public static List<int> ListPrimes(int target)
    {
        if (target > 1000000)
            throw new ArgumentOutOfRangeException(nameof(target), "Target exceeds maximum limit of 1,000,000.");

        if (target < 2)
            return [];

        // Set up sieve
        bool[] isPrime = new bool[target + 1];
        for (int i = 2; i <= target; i++)
            isPrime[i] = true;

        // Mark non-primes
        int limit = (int)Math.Sqrt(target);
        for (int i = 2; i <= limit; i++)
        {
            if (!isPrime[i])
                continue;
            for (int j = i * i; j <= target; j += i)
                isPrime[j] = false;
        }

        // Collate primes list
        List<int> primes = [];
        for (int i = 2; i <= target; i++)
            if (isPrime[i])
                primes.Add(i);

        return primes;
    }

    // Check primality methods

    /// <summary>
    /// Uses 6k +/- 1 optimization to check for primality.
    /// </summary>
    public static bool Check(ulong num)
    {
        if (num <= 1)
            return false;

        if (num == 2 || num == 3 || num == 5)
            return true;

        if (num % 2 == 0 || num % 3 == 0 || num % 5 == 0)
            return false;

        ulong limit = (ulong)Math.Sqrt(num);

        for (ulong k = 6; k <= limit; k += 6)
        {
            if (num % (k - 1) == 0 || num % (k + 1) == 0)
                return false;
        }

        return true;
    }

    // Miller-Rabin primality test

    /// <summary>
    /// Uses the Miller-Rabin primality test to check for primality.
    /// </summary>
    public static bool IsPrime(ulong num)
    {
        if (num < 2)
            throw new ArgumentOutOfRangeException(nameof(num), "The number must be at least two.");

        if (num == 2 || num == 3)
            return true;
        if (num % 2 == 0)
            return false;

        ulong d = num - 1;
        int s = 0;

        while ((d & 1) == 0)
        {
            d >>= 1;
            s++;
        }

        // Deterministic bases for 64-bit numbers
        ulong[] bases = [2, 325, 9375, 28178, 450775, 9780504, 1795265022];

        foreach (ulong a in bases)
        {
            if (a % num == 0)
                continue;

            if (!MillerTest(a, d, s, num))
                return false;
        }

        return true;
    }

    private static bool MillerTest(ulong a, ulong d, int s, ulong n)
    {
        ulong x = ModPow(a, d, n);
        if (x == 1 || x == n - 1)
            return true;

        for (int r = 1; r < s; r++)
        {
            x = ModMul(x, x, n);
            if (x == n - 1)
                return true;
        }

        return false;
    }

    private static ulong ModPow(ulong a, ulong d, ulong mod)
    {
        ulong result = 1;

        while (d > 0)
        {
            if ((d & 1) == 1)
                result = ModMul(result, a, mod);

            a = ModMul(a, a, mod);
            d >>= 1;
        }

        return result;
    }

    private static ulong ModMul(ulong a, ulong b, ulong mod)
    {
        return (ulong)((BigInteger)a * b % mod);
    }

    /// <summary>
    /// Lists all prime factors of the target number using division by known primes.
    /// </summary>
    public static List<int> PrimeFactors(int target)
    {
        if (target < 2)
            throw new ArgumentOutOfRangeException(nameof(target), "Target must be at least 2.");
        if (target > 1000000)
            throw new ArgumentOutOfRangeException(nameof(target), "Target cannot be larger than 1,000,000.");

        List<int> factors = [];
        int n = target;
        List<int> primes = ListPrimes((int)Math.Sqrt(target) + 1);

        foreach (int p in primes)
        {
            if (p * p > n)
                break;

            while (n % p == 0)
            {
                factors.Add(p);
                n /= p;
            }

            if (n == 1)
                break;
        }

        if (n > 1)
            factors.Add(n);

        return factors;
    }

    /// <summary>
    /// Counts the number of primes up to the specified limit using a segmented sieve.
    /// </summary>
    public static ulong CountPrimes(ulong limit)
    {
        if (limit < 2)
            return 0;

        ulong sqrtLimit = (ulong)Math.Sqrt(limit) + 1;
        bool[] isPrimeSmall = new bool[sqrtLimit + 1];
        for (ulong i = 0; i <= sqrtLimit; i++)
            isPrimeSmall[i] = true;

        for (ulong i = 2; i * i <= sqrtLimit; i++)
            if (isPrimeSmall[i])
                for (ulong j = i * i; j <= sqrtLimit; j += i)
                    isPrimeSmall[j] = false;

        List<ulong> smallPrimes = new List<ulong>();
        for (ulong i = 2; i <= sqrtLimit; i++)
            if (isPrimeSmall[i])
                smallPrimes.Add(i);

        ulong block_size = 10000000;
        ulong low = 2, high = Math.Min(low + block_size - 1, limit);
        ulong count = 0;

        while (low <= limit)
        {
            bool[] block = new bool[high - low + 1];
            for (int i = 0; i < block.Length; i++) block[i] = true;

            foreach (ulong p in smallPrimes)
            {
                ulong start = (low + p - 1) / p * p;
                if (start < p * p) start = p * p;

                for (ulong j = start; j <= high; j += p)
                    block[j - low] = false;
            }

            for (ulong i = 0; i < (ulong)block.Length; i++)
                if (block[i]) count++;

            low = high + 1;
            high = Math.Min(low + block_size - 1, limit);
        }

        return count;
    }
}

