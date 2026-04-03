using System;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SailwindRegatta
{
    internal static class SupabaseClient
    {
        private static readonly HttpClient _http = new HttpClient();

        internal static async Task InitPlayerAsync(SteamUser user)
        {
            try
            {
                var body = JsonUtility.ToJson(new UpsertPlayerRpcRequest
                {
                    key  = ComputePlayerKey(user.SteamId),
                    name = user.PersonaName
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/upsert_player", body);
                var response = await _http.SendAsync(req);
                string raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.LogError($"Supabase player upsert failed ({(int)response.StatusCode}): {raw}");
                    return;
                }

                // RPC returning a scalar UUID comes back as a JSON string: "\"uuid-here\""
                string playerUuid = raw.Trim().Trim('"');
                if (string.IsNullOrEmpty(playerUuid))
                {
                    Plugin.Log.LogError("Supabase upsert_player RPC returned empty UUID.");
                    return;
                }

                Plugin.Session = new PlayerSession(playerUuid);
                Plugin.Log.LogInfo($"Supabase session ready. PlayerUUID: {Plugin.Session.PlayerUuid}");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase InitPlayerAsync exception: {ex.Message}");
            }
        }

        internal static async Task<string> StartRunAsync(PlayerSession session, int raceId, DateTime startedAt)
        {
            try
            {
                var body = JsonUtility.ToJson(new StartRunRpcRequest
                {
                    player_id = session.PlayerUuid,
                    race_id   = raceId,
                    started_at  = startedAt.ToString("o")  // ISO 8601 round-trip format
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/start_run", body);
                var response = await _http.SendAsync(req);
                string raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.LogError($"Supabase start_run failed ({(int)response.StatusCode}): {raw}");
                    return null;
                }

                // RPC returning a scalar UUID comes back as a JSON string: "\"uuid-here\""
                string runUuid = raw.Trim().Trim('"');
                if (string.IsNullOrEmpty(runUuid))
                {
                    Plugin.Log.LogError("Supabase start_run RPC returned empty UUID.");
                    return null;
                }

                return runUuid;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase StartRunAsync exception: {ex.Message}");
                return null;
            }
        }

        // Calls finish_run RPC. Run UUID acts as proof of ownership; trigger still enforces immutability.
        internal static async Task FinishRunAsync(string runGuid, DateTime finishedAt, int durationSeconds)
        {
            try
            {
                var body = JsonUtility.ToJson(new FinishRunRpcRequest
                {
                    run_id      = runGuid,
                    finished_at = finishedAt.ToString("o"),
                    duration    = durationSeconds
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/finish_run", body);
                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                {
                    string raw = await response.Content.ReadAsStringAsync();
                    Plugin.Log.LogError($"Supabase finish_run failed ({(int)response.StatusCode}): {raw}");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase FinishRunAsync exception: {ex.Message}");
            }
        }

        private static HttpRequestMessage BuildRequest(HttpMethod method, string path, string jsonBody = null)
        {
            var req = new HttpRequestMessage(method, SupabaseConfig.ProjectUrl + path);
            req.Headers.Add("apikey", SupabaseConfig.PublishableKey);

            if (jsonBody != null)
                req.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            return req;
        }

        private static string ComputePlayerKey(string steamId)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(steamId + Secrets.PlayerKeySalt));
                var sb = new StringBuilder(64);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
