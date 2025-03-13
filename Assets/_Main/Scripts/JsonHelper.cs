using System;
using System.Collections.Generic;
using UnityEngine;

// Helper class for serializing Dictionary types which Unity's JsonUtility doesn't support natively
public static class JsonHelper
{
    // Serializable wrapper for Dictionary<BetTypes, int>
    [Serializable]
    public class BetTypeIntDictionary
    {
        [Serializable]
        public class KeyValuePair
        {
            public BetTypes Key;
            public int Value;
        }

        public List<KeyValuePair> Items = new List<KeyValuePair>();

        // Convert from Dictionary to this serializable class
        public static BetTypeIntDictionary FromDictionary(Dictionary<BetTypes, int> dict)
        {
            BetTypeIntDictionary result = new BetTypeIntDictionary();
            foreach (var kvp in dict)
            {
                result.Items.Add(new KeyValuePair { Key = kvp.Key, Value = kvp.Value });
            }
            return result;
        }

        // Convert back to Dictionary
        public Dictionary<BetTypes, int> ToDictionary()
        {
            Dictionary<BetTypes, int> result = new Dictionary<BetTypes, int>();
            foreach (var item in Items)
            {
                result[item.Key] = item.Value;
            }
            return result;
        }
    }

    // Convert Dictionary to JSON
    public static string ToJson(Dictionary<BetTypes, int> dict)
    {
        return JsonUtility.ToJson(FromDictionary(dict));
    }

    // Convert JSON to Dictionary
    public static Dictionary<BetTypes, int> FromJson(string json)
    {
        BetTypeIntDictionary wrapper = JsonUtility.FromJson<BetTypeIntDictionary>(json);
        return wrapper.ToDictionary();
    }

    // Helper method to convert a Dictionary to a serializable form
    public static BetTypeIntDictionary FromDictionary(Dictionary<BetTypes, int> dict)
    {
        return BetTypeIntDictionary.FromDictionary(dict);
    }
}