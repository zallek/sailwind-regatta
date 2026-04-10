using System.IO;
using Microsoft.Win32;

namespace SailwindRegatta
{
    // Holds the local player's Steam identity as read from loginusers.vdf.
    internal class SteamUser
    {
        internal string PersonaName { get; }
        internal string SteamId { get; }

        internal SteamUser(string personaName, string steamId)
        {
            PersonaName = personaName;
            SteamId = steamId;
        }
    }

    // Reads Steam user data directly from Steam's local config files, without Steamworks.NET.
    // Sailwind ships without steam_api64.dll, so Steamworks.NET cannot be initialized.
    // Parsing loginusers.vdf is a lightweight alternative for read-only identity data.
    internal static class SteamUtils
    {
        // Reads the MostRecent user's PersonaName and SteamId from Steam's loginusers.vdf.
        // Returns null if Steam is not installed or the file cannot be parsed.
        internal static SteamUser GetCurrentUser()
        {
            try
            {
                string steamPath = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Valve\Steam", "SteamPath", null) as string;

                if (steamPath == null)
                    return null;

                string vdfPath = Path.Combine(steamPath, "config", "loginusers.vdf");
                if (!File.Exists(vdfPath))
                    return null;

                // VDF structure:
                // "users" { "steamid" { "PersonaName" "Name" ... "MostRecent" "1" } }
                // VDF is two levels deep:
                //   depth 1 = "users" block
                //   depth 2 = individual user block, keyed by Steam ID
                string[] lines = File.ReadAllLines(vdfPath);
                int depth = 0;
                string currentSteamId = null;
                string personaName = null;
                bool mostRecent = false;

                foreach (string raw in lines)
                {
                    string line = raw.Trim();

                    if (line == "{")
                    {
                        depth++;
                        continue;
                    }

                    if (line == "}")
                    {
                        if (depth == 2 && mostRecent && personaName != null)
                            return new SteamUser(personaName, currentSteamId);

                        if (depth == 2)
                        {
                            // Reset for next user block.
                            currentSteamId = null;
                            personaName = null;
                            mostRecent = false;
                        }

                        depth--;
                        continue;
                    }

                    if (depth == 1)
                    {
                        // The only keys at this level are Steam IDs.
                        currentSteamId = ParseQuotedKey(line);
                    }
                    else if (depth == 2)
                    {
                        string key = ParseQuotedKey(line);
                        string value = ParseQuotedValue(line);
                        if (key == null || value == null)
                            continue;

                        if (key == "PersonaName")
                            personaName = value;
                        else if (key == "MostRecent" && value == "1")
                            mostRecent = true;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        // Extracts the first quoted token from a VDF line.
        // "KeyName"    "Value"  →  "KeyName"
        private static string ParseQuotedKey(string line)
        {
            if (line.Length == 0 || line[0] != '"')
                return null;
            int end = line.IndexOf('"', 1);
            return end < 0 ? null : line.Substring(1, end - 1);
        }

        // Extracts the second quoted token from a VDF line.
        // "KeyName"    "Value"  →  "Value"
        private static string ParseQuotedValue(string line)
        {
            if (line.Length == 0 || line[0] != '"')
                return null;
            int keyEnd = line.IndexOf('"', 1);
            if (keyEnd < 0)
                return null;
            int valStart = line.IndexOf('"', keyEnd + 1);
            if (valStart < 0)
                return null;
            int valEnd = line.IndexOf('"', valStart + 1);
            return valEnd < 0 ? null : line.Substring(valStart + 1, valEnd - valStart - 1);
        }
    }
}
