# Plan: Flag ShipyardExpansion-Modified Boats on Leaderboard

## Context

ShipyardExpansion (SE) is a BepInEx mod that unlocks extra masts, sail size/angle controls, and custom parts beyond the vanilla Sailwind Shipyard. These customizations give a meaningful speed advantage. The regatta mod should flag such boats on the leaderboard so players can assess results fairly.

**Key constraint:** SE writes its save keys (`SEboatSails.{sceneIndex}`, `com.nandbrew.shipyardexpansion.{sceneIndex}.partCount`) for ALL boats whenever the game is saved, even with zero modifications. Detection must be **content-based**, not key-existence-based.

---

## Detection Strategy

Two independent checks are combined with OR logic. Both parse `GameState.modData` directly — no SE assembly reference or reflection needed.

### Check 1 — Sail modifications

Key: `SEboatSails.{sceneIndex}`  
Format: `{mastIdx}({prefabIdx},{scaleX},{scaleY},{angle}[,{flipped}]]{...})(...)...`

For an unmodified boat, every sail entry has `scaleX = 1`, `scaleY = 1`, `angle = 0`, `flipped` absent or `False`.

Flag as SE-modded if any sail entry satisfies:
- `|scaleX - 1.0| > 0.001` (enlarged/shrunk)
- `|scaleY - 1.0| > 0.001`
- `|angle| > 0.1` (rotated)
- `flipped == "True"` (jib flipped to opposite side)

### Check 2 — Extra part options active

Key: `com.nandbrew.shipyardexpansion.{sceneIndex}.partCount`  
Value: integer string, e.g. `"12"` → vanilla part count N

At runtime, `BoatCustomParts.availableParts` contains vanilla parts (indices 0…N-1) followed by SE-added parts (indices N…). If the list length > N and any extra part has `activeOption != 0`, a non-default SE option is in use (extra mast, extra sail slot, etc.).

---

## Implementation Steps

### 1. New file `SeDetector.cs`

```csharp
public static class SeDetector
{
    const string SE_ID = "com.nandbrew.shipyardexpansion";

    public static bool IsSeModded(int sceneIndex, BoatCustomParts parts)
        => HasModifiedSails(sceneIndex) || HasActiveSeOptions(sceneIndex, parts);

    static bool HasModifiedSails(int sceneIndex)
    {
        if (!GameState.modData.TryGetValue("SEboatSails." + sceneIndex, out var raw))
            return false;
        // Parse each sail entry: prefabIdx,scaleX,scaleY,angle[,flipped]
        foreach (var token in ParseSailTokens(raw))
        {
            if (Math.Abs(token.scaleX - 1f) > 0.001f) return true;
            if (Math.Abs(token.scaleY - 1f) > 0.001f) return true;
            if (Math.Abs(token.angle)       > 0.1f)   return true;
            if (token.flipped)                         return true;
        }
        return false;
    }

    static bool HasActiveSeOptions(int sceneIndex, BoatCustomParts parts)
    {
        var countKey = $"{SE_ID}.{sceneIndex}.partCount";
        if (!GameState.modData.TryGetValue(countKey, out var raw)
            || !int.TryParse(raw, out int vanillaCount))
            return false;
        for (int i = vanillaCount; i < parts.availableParts.Count; i++)
            if (parts.availableParts[i].activeOption != 0)
                return true;
        return false;
    }
}
```

`ParseSailTokens` extracts all `prefabIdx,scaleX,scaleY,angle[,flipped]` groups from the SE string.

### 2. Extend `Run` — `Run.cs`

Add:
```csharp
public bool IsSeModded { get; set; }
```

### 3. Set flag at race start — `RaceManager.cs` (lines 165–186)

Where `Run.BoatTypeId` is captured (first steering-wheel use), add:
```csharp
var boatCustomParts = rudder.shipRigidbody.GetComponent<BoatCustomParts>();
currentRun.IsSeModded = SeDetector.IsSeModded(boatSceneIndex, boatCustomParts);
```

### 4. Include in submission — `SupabaseModels.cs` + `SupabaseClient.cs`

- Add `[JsonProperty("is_se_modded")] public bool IsSeModded { get; set; }` to the run submission model.
- Populate from `run.IsSeModded` before the POST.

### 5. Supabase schema change

```sql
ALTER TABLE runs ADD COLUMN is_se_modded BOOLEAN NOT NULL DEFAULT FALSE;
```

Update `get_leaderboard` RPC to `SELECT` and return `is_se_modded`.

### 6. Extend `LeaderboardEntryResponse` — `SupabaseModels.cs`

```csharp
[JsonProperty("is_se_modded")]
public bool IsSeModded { get; set; }
```

### 7. Update leaderboard display — `RaceLeaderboardUI.cs` (lines 83–99)

In `Refresh()`, append `" [SE]"` to the boat name (or player name) when `entry.IsSeModded`:
```
1. PlayerName
    12h 34m  Dhow [SE]
```

---

## Critical Files

| File | Change |
|------|--------|
| `SeDetector.cs` | New — content-based detection |
| `Run.cs` | Add `IsSeModded` property |
| `RaceManager.cs` | Set flag when boat type is captured |
| `SupabaseModels.cs` | Submission model + `LeaderboardEntryResponse` |
| `SupabaseClient.cs` | Include `is_se_modded` in POST body |
| `RaceLeaderboardUI.cs` | Render `[SE]` marker in text |
| Supabase DB | Add column, update RPC |

---

## Verification

1. **SE installed, boat modified (sail scaled up or extra mast active)** → submitted entry has `is_se_modded = true`, leaderboard shows `[SE]`.
2. **SE installed, boat never modified** → all sails at default, no extra options active → `is_se_modded = false`, no marker.
3. **SE not installed** → both modData keys absent → `HasModifiedSails` and `HasActiveSeOptions` return false → no marker, no errors.
4. **Legacy leaderboard entries** (before column added) → DB default `false` → display normally.