using UnityEngine;

public static class Extensions {
    public static void Shuffle<T>(this T[] array) {
        var n = array.Length;
        while (n > 1) {
            n--;
            var k = Random.Range(0, n + 1);
            T value = array[k];
            array[k] = array[n];
            array[n] = value;
        }
    }
}
