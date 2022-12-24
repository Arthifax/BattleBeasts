using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RandomUtil
{
    public static T EitherOr<T>(params T[] items) {
        return items[Random.Range(0, items.Length)];
    }
}
