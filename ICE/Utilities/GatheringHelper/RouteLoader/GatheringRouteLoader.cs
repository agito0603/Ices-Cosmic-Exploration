using ICE.Utilities.Cosmic_Helper;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace ICE.Utilities.GatheringHelper.RouteLoader;

public static class GatheringRouteLoader
{
    private static readonly JsonSerializerOptions ReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new Vector3Converter() }
    };

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new Vector3Converter() }
    };

    // route_id -> route
    private static Dictionary<uint, GatheringRouteFile>? _cache;

    // ── Loading ──────────────────────────────────────────────────────────────

    public static Dictionary<uint, GatheringRouteFile> LoadAllRoutes()
    {
        if (_cache != null)
            return _cache;

        _cache = new Dictionary<uint, GatheringRouteFile>();

        LoadEmbeddedRoutes(_cache);
        LoadDiskRoutes(_cache);

        PluginLog.Information($"Loaded {_cache.Count} gathering routes");
        return _cache;
    }

    private static void LoadEmbeddedRoutes(Dictionary<uint, GatheringRouteFile> target)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resources = assembly.GetManifestResourceNames()
            .Where(r => r.Contains("GatheringRoutes") && r.EndsWith(".json"));

        foreach (var resourceName in resources)
        {
            try
            {
                using var stream = assembly.GetManifestResourceStream(resourceName)!;
                using var reader = new StreamReader(stream);
                var route = JsonSerializer.Deserialize<GatheringRouteFile>(reader.ReadToEnd(), ReadOptions);

                if (route == null) continue;

                if (target.ContainsKey(route.RouteId))
                {
                    PluginLog.Warning($"Duplicate route_id {route.RouteId} in {resourceName}, skipping");
                    continue;
                }

                target[route.RouteId] = route;
                PluginLog.Verbose($"Embedded route {route.RouteId} loaded");
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load embedded route {resourceName}: {ex.Message}");
            }
        }
    }

    private static void LoadDiskRoutes(Dictionary<uint, GatheringRouteFile> target)
    {
        var basePath = GetBasePath();
        if (!Directory.Exists(basePath))
            return;

        var files = Directory.GetFiles(basePath, "*.json", SearchOption.AllDirectories);
        int loaded = 0, overridden = 0;

        foreach (var file in files)
        {
            try
            {
                var route = JsonSerializer.Deserialize<GatheringRouteFile>(
                    File.ReadAllText(file), ReadOptions);

                if (route == null) continue;

                bool isOverride = target.ContainsKey(route.RouteId);
                target[route.RouteId] = route;

                if (isOverride) overridden++;
                else loaded++;
            }
            catch (Exception ex)
            {
                PluginLog.Error($"Failed to load route {file}: {ex.Message}");
            }
        }

        PluginLog.Information($"Loaded {loaded} disk routes, {overridden} overrides from {basePath}");
    }

    // ── Saving ───────────────────────────────────────────────────────────────

    public static void SaveRoute(GatheringRouteFile route)
    {
        if (!CosmicMoonRegistry.TryGetMoon(route.TerritoryId, out var moon))
        {
            PluginLog.Error($"SaveRoute: unknown territory {route.TerritoryId} for route {route.RouteId}");
            return;
        }

        var dir = Path.Combine(GetBasePath(), $"{moon.TerritoryId}_{moon.DisplayName}");
        Directory.CreateDirectory(dir);

        var path = Path.Combine(dir, $"route_{route.RouteId}.json");
        route.DateModified = DateTime.UtcNow;

        File.WriteAllText(path, JsonSerializer.Serialize(route, WriteOptions));

        // Update cache immediately
        _cache ??= new();
        _cache[route.RouteId] = route;

        PluginLog.Verbose($"Saved route {route.RouteId} -> {path}");
    }

    // ── Stubs ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates stub route files for any gathering missions in SheetMissionDict
    /// that don't already have a route file. Node is left null until captured in-mission.
    /// </summary>
    public static List<uint> CreateMissingStubs(bool dryRun = false)
    {
        var routes = LoadAllRoutes();
        var created = new List<uint>();

        foreach (var (missionId, info) in CosmicHelper.SheetMissionDict)
        {
            if (!info.Jobs.Contains(16) && !info.Jobs.Contains(17))
                continue;

            if (routes.ContainsKey(missionId))
                continue;

            uint jobId = info.Jobs.Contains(17) ? 17u : 16u;

            PluginLog.Information($"Missing stub: route {missionId}, territory {info.TerritoryId}, job {jobId}");

            if (dryRun)
                continue;

            var stub = new GatheringRouteFile
            {
                RouteId = info.Gather_MapKey,
                TerritoryId = info.TerritoryId,
                GatheringJobId = jobId,
                Author = string.IsNullOrWhiteSpace(C.AuthorName) ? "Ice" : C.AuthorName,
                Nodes = null
            };

            SaveRoute(stub);
            created.Add(info.Gather_MapKey);
        }

        PluginLog.Information(dryRun
            ? $"Dry run: {created.Count} stubs would be created"
            : $"Created {created.Count} stub routes");

        return created;
    }

    // ── Queries ──────────────────────────────────────────────────────────────

    public static GatheringRouteFile? GetRoute(uint routeId)
    {
        var routes = LoadAllRoutes();
        return routes.TryGetValue(routeId, out var route) ? route : null;
    }

    public static bool HasNode(uint routeId) => GetRoute(routeId)?.Nodes is { Count: > 0 };

    public static List<GatheringRouteFile> GetRoutesForTerritory(uint territoryId) =>
        LoadAllRoutes().Values.Where(r => r.TerritoryId == territoryId).ToList();

    public static List<GatheringRouteFile> GetRoutesForJob(uint jobId) =>
        LoadAllRoutes().Values.Where(r => r.GatheringJobId == jobId).ToList();

    public static List<GatheringRouteFile> GetIncompleteRoutes() =>
        LoadAllRoutes().Values.Where(r => r.Nodes is null or { Count: 0 }).ToList();

    // ── Cache ─────────────────────────────────────────────────────────────────

    public static void ClearCache() => _cache = null;

    // ── Internals ────────────────────────────────────────────────────────────

    private static string GetBasePath() =>
        !string.IsNullOrEmpty(C.CustomRoutePath)
            ? C.CustomRoutePath
            : Path.Combine(Svc.PluginInterface.ConfigDirectory.FullName, "GatheringRoutes");
}
