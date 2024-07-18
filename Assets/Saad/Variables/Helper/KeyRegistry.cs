using System.Collections.Generic;

namespace ProjectCore.Helpers
{
    public static class KeyRegistry
    {
        private static HashSet<string> keys = new HashSet<string>();
        private static int counter = 0;

        public static string GenerateKey(string prefix = "DBInt")
        {
            string uniqueKey;
            do
            {
                uniqueKey = $"{prefix}_{counter}";
                counter++;
            } while (keys.Contains(uniqueKey));

            keys.Add(uniqueKey);
            return uniqueKey;
        }

        public static void UnregisterKey(string key)
        {
            keys.Remove(key);
        }
    }
}
