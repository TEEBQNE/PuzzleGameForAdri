using System;
using System.Collections.Generic;

/// <summary>
/// Static random numbers
/// </summary>
public static class StaticRandomNumber
{
    static System.Random RandomNumbers = new System.Random();

    public static int GetRandomInt(int min, int max)
    {
        // return the random int from [min, max)
        return RandomNumbers.Next(min, max);
    }

    public static float GetRandomFloat(float min, float max)
    {
        return (float)(RandomNumbers.NextDouble() * (max - min) + min);
    }
}

/// <summary>
/// Singleton class to generate random numbers based on monobehaviour calling it
/// </summary>
public class RandomNumber : Singleton<RandomNumber>
{
    // holds all number generators for the game
    private Dictionary<string, System.Random> RandomNumberGenerators = new Dictionary<string, System.Random>();

    /// <summary>
    /// Creates a new random number generator for a new monohebaviour
    /// </summary>
    /// <param name="mono"></param>
    private void GenerateNewNumberGenerator(string mono)
    {
        RandomNumberGenerators.Add(mono, new System.Random());
    }

    /// <summary>
    /// Get random number between [min, max)
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public int GetRandomInt(string mono, int min, int max)
    {
        // if the key does not exist, generate a new random generator
        if(!RandomNumberGenerators.ContainsKey(mono))
            GenerateNewNumberGenerator(mono);

        // return the random int from [min, max)
        return RandomNumberGenerators[mono].Next(min, max);
    }

    /// <summary>
    /// Get a random long between [min, max)
    /// From: https://gist.github.com/subena22jf/c7bb027ea99127944981
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public long GetRandomLong(string mono, long min, long max)
    {
        // if the key does not exist, generate a new random generator
        if (!RandomNumberGenerators.ContainsKey(mono))
            GenerateNewNumberGenerator(mono);

        ulong uRange = (ulong)(max - min);
        ulong ulongRand;
        do
        {
            byte[] buf = new byte[8];
            RandomNumberGenerators[mono].NextBytes(buf);
            ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
        } while (ulongRand > ulong.MaxValue - ((ulong.MaxValue % uRange) + 1) % uRange);

        return (long)(ulongRand % uRange) + min;
    }

    /// <summary>
    /// Get random number between [min, max)
    /// </summary>
    /// <param name="mono"></param>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public float GetRandomFloat(string mono, float min, float max)
    {
        // if the key does not exist, generate a new random generator
        if (!RandomNumberGenerators.ContainsKey(mono))
            GenerateNewNumberGenerator(mono);

        // return the random float from [min, max)
        return (float)(RandomNumberGenerators[mono].NextDouble() * (max - min) + min);
    }
}