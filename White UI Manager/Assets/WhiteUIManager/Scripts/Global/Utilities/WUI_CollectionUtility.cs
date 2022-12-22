using System.Collections.Generic;

namespace WUI.Utilities
{
    public static class CollectionUtility
    {
        public static void AddItem<T, K>(this WUI_SerializableDictionary<T, List<K>> serializableDictionary, T key, K value)
        {
            if (serializableDictionary.ContainsKey(key))
            {
                serializableDictionary[key].Add(value);

                return;
            }

            serializableDictionary.Add(key, new List<K> { value });
        }
    }
}