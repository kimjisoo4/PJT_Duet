using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace StudioScor.Utilities
{
    public static partial class SUtility
    {
        public static int LastIndex<T>(this IReadOnlyCollection<T> array)
        {
            return array.Count - 1;
        }
       public static int LastIndex<T>(this T[] array)
        {
            return array.Length - 1;
        }
        public static int LastIndex<T>(this IReadOnlyList<T> list)
        {
            return list.Count - 1;
        }
        public static int LastIndex<T>(this List<T> list)
        {
            return list.Count - 1;
        }

        public static T[] GetRandomElements<T>(T[] array, int count)
        {
            if (count > array.Count())
                return null;

            T[] copy = (T[])array.Clone();

            System.Random rnd = new System.Random();
            int n = copy.Length;

            for(int i = 0; i < count; i++)
            {
                int randomIndex = rnd.Next(i, n);

                T temp = copy[i];
                copy[i] = copy[randomIndex];
                copy[randomIndex] = temp;
            }

            T[] result = new T[count];
            Array.Copy(copy, result, count);

            return result;
        }
        public static List<T> Shuffle<T>(this List<T> list)
        {
            int random1, random2;
            T temp;

            for (int i = 0; i < list.Count; ++i)
            {
                random1 = UnityEngine.Random.Range(0, list.Count);
                random2 = UnityEngine.Random.Range(0, list.Count);

                temp = list[random1];
                list[random1] = list[random2];
                list[random2] = temp;
            }

            return list;
        }
    }
}
