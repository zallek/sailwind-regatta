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

        internal static async Task<string> UpsertPlayerAsync(SteamUser user)
        {
            try
            {
                bool dev = Plugin.DevMode.Value;
                var body = JsonUtility.ToJson(new UpsertPlayerRpcRequest
                {
                    key  = ComputePlayerKey(user.SteamId, dev),
                    name = user.PersonaName,
                    dev  = dev
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/upsert_player", body);
                var response = await _http.SendAsync(req);
                string raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.LogError($"Supabase upsert_player failed ({(int)response.StatusCode}): {raw}");
                    return null;
                }

                // RPC returning a scalar UUID comes back as a JSON string: "\"uuid-here\""
                string playerUuid = raw.Trim().Trim('"');
                if (string.IsNullOrEmpty(playerUuid))
                {
                    Plugin.Log.LogError("Supabase upsert_player returned empty UUID.");
                    return null;
                }

                return playerUuid;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase InitPlayerAsync exception: {ex.Message}");
                return null;
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
                    Plugin.Log.LogError("Supabase start_run returned empty UUID.");
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
        internal static async Task<bool> FinishRunAsync(string runId, DateTime finishedAt, int durationSeconds, int? boatTypeId)
        {
            try
            {
                string body = boatTypeId.HasValue
                    ? JsonUtility.ToJson(new FinishRunRpcRequest
                    {
                        run_id       = runId,
                        finished_at  = finishedAt.ToString("o"),
                        duration     = durationSeconds,
                        boat_type_id = boatTypeId.Value
                    })
                    : JsonUtility.ToJson(new FinishRunNoBoatRpcRequest
                    {
                        run_id      = runId,
                        finished_at = finishedAt.ToString("o"),
                        duration    = durationSeconds
                    });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/finish_run", body);
                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                {
                    string raw = await response.Content.ReadAsStringAsync();
                    Plugin.Log.LogError($"Supabase finish_run failed ({(int)response.StatusCode}): {raw}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase FinishRunAsync exception: {ex.Message}");
                return false;
            }
        }

        internal static async Task<bool> AbortRunAsync(string runId, DateTime abortedAt)
        {
            try
            {
                var body = JsonUtility.ToJson(new AbortRunRpcRequest
                {
                    run_id     = runId,
                    aborted_at = abortedAt.ToString("o")
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/abort_run", body);
                var response = await _http.SendAsync(req);
                if (!response.IsSuccessStatusCode)
                {
                    string raw = await response.Content.ReadAsStringAsync();
                    Plugin.Log.LogError($"Supabase abort_run failed ({(int)response.StatusCode}): {raw}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase AbortRunAsync exception: {ex.Message}");
                return false;
            }
        }

        internal static async Task<LeaderboardEntryResponse[]> GetLeaderboardAsync(int raceId, int maxResults = 5)
        {
            try
            {
                var body = JsonUtility.ToJson(new GetLeaderboardRpcRequest
                {
                    race_id     = raceId,
                    max_results = maxResults,
                    dev         = Plugin.DevMode.Value
                });

                var req = BuildRequest(HttpMethod.Post, "/rest/v1/rpc/get_leaderboard", body);
                var response = await _http.SendAsync(req);
                string raw = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    Plugin.Log.LogError($"Supabase get_leaderboard failed ({(int)response.StatusCode}): {raw}");
                    return null;
                }

                // PostgREST returns a JSON array at the root — wrap it for JsonUtility.
                var wrapper = JsonUtility.FromJson<LeaderboardWrapper>("{\"items\":" + raw + "}");
                // JsonUtility leaves empty [] as null; callers expect a real zero-length array.
                return wrapper?.items ?? Array.Empty<LeaderboardEntryResponse>();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"Supabase GetLeaderboardAsync exception: {ex.Message}");
                return null;
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

        private static string ComputePlayerKey(string steamId, bool dev = false)
        {
            string salt = dev ? Secrets.PlayerKeySaltDev : Secrets.PlayerKeySalt;
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(steamId + salt));
                var sb = new StringBuilder(64);
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}
