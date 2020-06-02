using System.Collections.Generic;
using UnityEngine;

internal static class ListE
{
    public static T RandomItem<T>(this List<T> list)
    {
        return list[Random.Range(0, list.Count)];
    }
}