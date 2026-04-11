using System.Collections.Generic;
using UnityEngine;

namespace SailwindRegatta
{
    public static class JsonUtils
    {
        public static T[] FromJsonArray<T>(string json)
        {
            var results = new List<T>();
            int depth = 0;
            int start = -1;

            for (int i = 0; i < json.Length; i++)
            {
                if (json[i] == '{')
                {
                    if (depth == 0)
                        start = i;
                    depth++;
                }
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0 && start >= 0)
                    {
                        results.Add(JsonUtility.FromJson<T>(json.Substring(start, i - start + 1)));
                        start = -1;
                    }
                }
            }

            return results.ToArray();
        }
    }
}
