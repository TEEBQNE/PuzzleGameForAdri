using System;
using System.Collections.Generic;

public static class ListExtensions
{
    static Random rnd = null;

    /*
    * extends list to add a shuffle function and swap function
    */
    public static void Shuffle<T>(this IList<T> list)
    {
        if (rnd == null)
            rnd = new Random();

        for (var i = 0; i < list.Count; i++)
            list.Swap(i, rnd.Next(i, list.Count));
    }

    public static void Swap<T>(this IList<T> list, int i, int j)
    {
        var temp = list[i];
        list[i] = list[j];
        list[j] = temp;
    }

    public static T RandomElement<T>(this IList<T> list)
    {
        if (rnd == null)
            rnd = new Random();
        return list[rnd.Next(list.Count)];
    }

    public static T RandomElement<T>(this IList<T> list, int maxElement)
    {
        Random rnd = new Random();
        return list[rnd.Next(maxElement)];
    }
}