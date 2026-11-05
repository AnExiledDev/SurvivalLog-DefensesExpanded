using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefensesExpanded.Core;
using GameCore.HotUpdate;
using GameCore.HotUpdate.Battle;
using GameCore.HotUpdate.Battle.Show;
using GameCore.Scripts;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using Numerics = System.Numerics;
using Object = UnityEngine.Object;

namespace DefensesExpanded.Adapters;

/// <summary>
/// Gives each defense its own model. Its furniture row names a prefab the game does not have; when the game asks for
/// it, it gets a copy of the stock defense's prefab, so every part the game looks for is there: the head bar, the
/// collider, the nav obstacle, the animator, the state switcher and the effects. The stock meshes under the four state
/// groups (intact, two damaged, broken) are swapped for the defense's own, built from the model file inside the DLL,
/// which has a group for each state; the game shows one at a time as the defense wears down. If a model cannot be
/// built, the game gets the stock prefab, so the defense still works, looking like the stock one.
/// </summary>
internal static class DefenseModel
{
    private const string Folder = "Assets/RuntimeAssets/Prefabs/Furniture/";
    private const float HeadBarAboveTop = 0.35f;

    /// <summary>The stock prefabs' state groups, which the model file names its groups after.</summary>
    private static readonly string[] States = { "State01", "State02", "State03", "StateBroken" };

    /// <summary>The electric trap's arcs, which the Tesla Coil throws from its top.</summary>
    private const string ArcName = "FX_ElectricArc";
    private const string BoltsName = "PS_Bolts";
    private const float BoltsBelowTop = 0.08f;

    /// <summary>The chainsaw's spinning blade sparks and range ring, which a nail gun does not have.</summary>
    private static readonly string[] BladeOnly = { "Sparks_Rim", "RangeRing" };

    private static readonly IReadOnlyDictionary<string, Numerics.Vector3> Movers = States.ToDictionary(name => name, _ => Numerics.Vector3.UnitY);

    private static readonly Look[] Looks = DefenseRows.All.Select(kind => new Look(kind.Name, kind.ResName, kind.StockPrefab, kind.ModelFile)).ToArray();

    private static Action<string> _log = _ => { };
    private static GameObject? _holder;

    public static void Apply(Harmony harmony, Action<string> log)
    {
        _log = log;

        Patch(harmony, AccessTools.Method(typeof(BattleResource), "LoadPrefabWithFallback", new[] { typeof(string) }), prefix: nameof(GiveOurModel));
        Patch(harmony, AccessTools.Method(typeof(PreLoaderDataExtension), "AddChild"), prefix: nameof(SkipPreloadingOurModel));
    }

    private static Look? LookAt(string? path)
    {
        for (var i = 0; i < Looks.Length; i++)
            if (Looks[i].Path == path) return Looks[i];

        return null;
    }

    private static bool GiveOurModel(string __0, ref GameObject __result)
    {
        if (LookAt(__0) is not { } look) return true;

        try
        {
            __result = Template(look) ?? BattleResource.LoadPrefabWithFallback(look.StockPath);
        }
        catch (Exception ex)
        {
            if (!look.HasFailed) _log($"the {look.Name}'s model could not be built, so it has the stock look: {ex}");

            look.HasFailed = true;
            __result = BattleResource.LoadPrefabWithFallback(look.StockPath);
        }

        return false;
    }

    /// <summary>The game preloads every furniture prefab a level uses from its asset bundles, and ours are in none.</summary>
    private static bool SkipPreloadingOurModel(string __1) => LookAt(__1) == null;

    private static GameObject? Template(Look look)
    {
        if (look.Template != null && !look.Template.WasCollected) return look.Template;

        if (look.HasFailed) return null;

        var stock = BattleResource.LoadPrefabWithFallback(look.StockPath);

        if (stock == null)
        {
            _log($"the prefab {look.StockPath} is missing, so the {look.Name} has no model");
            look.HasFailed = true;

            return null;
        }

        look.Model ??= Glb.Read(Resource(look.ModelFile), Movers);

        if (_holder == null || _holder.WasCollected)
        {
            _holder = new GameObject("DefensesExpanded models");
            _holder.SetActive(false);
            Object.DontDestroyOnLoad(_holder);
        }

        var copy = Object.Instantiate(stock, _holder.transform);

        copy.name = look.ResName;
        Build(copy, look, look.Model);
        look.Template = copy;

        return copy;
    }

    private static void Build(GameObject copy, Look look, Model model)
    {
        var root = copy.transform;
        var groups = new Dictionary<string, Transform>();

        for (var c = 0; c < root.childCount; c++)
        {
            var child = root.GetChild(c);

            if (States.Contains(child.name) && !groups.ContainsKey(child.name)) groups[child.name] = child;
        }

        var missing = States.Where(name => !groups.ContainsKey(name)).ToList();

        if (missing.Count > 0) throw new InvalidOperationException("the stock prefab has no " + string.Join(", ", missing));

        var parts = model.Parts.ToDictionary(part => part.Name);
        var lacking = States.Where(name => !parts.ContainsKey(name)).ToList();

        if (lacking.Count > 0) throw new InvalidOperationException("the model file has no " + string.Join(", ", lacking));

        var stockRenderers = groups.Values.SelectMany(group => group.GetComponentsInChildren<MeshRenderer>(true)).ToList();
        var stockMaterial = stockRenderers.Select(renderer => renderer.sharedMaterial).FirstOrDefault(material => material != null);

        if (stockMaterial == null) throw new InvalidOperationException("the stock prefab has no material to copy");

        var layer = stockRenderers[0].gameObject.layer;

        _log($"building the {look.Name} on {look.StockPath}: {stockRenderers.Count} stock meshes, layer {layer}, shader {stockMaterial.shader.name}");

        // Only the state groups' meshes go; the ivy, the effects and everything else stay, so the game finds what it looks for.
        foreach (var group in groups.Values)
        {
            foreach (var renderer in group.GetComponentsInChildren<SkinnedMeshRenderer>(true)) Object.DestroyImmediate(renderer);

            foreach (var renderer in group.GetComponentsInChildren<MeshRenderer>(true)) Object.DestroyImmediate(renderer);

            foreach (var filter in group.GetComponentsInChildren<MeshFilter>(true)) Object.DestroyImmediate(filter);
        }

        foreach (var part in root.GetComponentsInChildren<Transform>(true).Where(part => BladeOnly.Contains(part.name)).ToList())
            Object.DestroyImmediate(part.gameObject);

        // The stock dyes point at the stock meshes; the game leaves a furniture without a palette undyed.
        var palette = copy.GetComponent<FurnitureDyePalette>();

        if (palette != null) Object.DestroyImmediate(palette);

        var materials = model.Materials.Select(material => MakeMaterial(stockMaterial, material, model.Images)).ToArray();
        var scale = root.lossyScale;

        // The intact model stands with its foot on the floor; the other states share its lift, so they stand where it did.
        var (min, max) = PartBounds(parts["State01"]);
        var lift = -min.Y;
        var made = new List<MeshRenderer>();

        foreach (var name in States)
        {
            var part = parts[name];
            var holder = new GameObject(look.ResName + "_" + name) { layer = layer };

            holder.transform.SetParent(groups[name], false);
            holder.transform.localRotation = Quaternion.identity;
            holder.transform.localScale = new UnityEngine.Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
            holder.transform.localPosition = Divide(new UnityEngine.Vector3(part.Pivot.X, part.Pivot.Y + lift, part.Pivot.Z), scale);

            for (var i = 0; i < part.Meshes.Count; i++) made.Add(MakeMesh(holder.transform, $"{name}_{i}", part.Meshes[i], materials[part.Meshes[i].Material], layer));
        }

        var bottom = root.TransformPoint(new UnityEngine.Vector3(min.X, 0f, min.Z));
        var top = root.TransformPoint(new UnityEngine.Vector3(max.X, max.Y - min.Y, max.Z));

        foreach (var collider in copy.GetComponentsInChildren<BoxCollider>(true)) Fit(collider, bottom, top);

        var height = top.y - bottom.y;
        var centre = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(part => part.name == "CenterPoint");

        if (centre != null) centre.position = new UnityEngine.Vector3(centre.position.x, bottom.y + height / 2f, centre.position.z);

        RaiseArcs(root, top.y);

        var animator = copy.GetComponent<ModelAnimator>();

        if (animator != null)
        {
            var list = new Il2CppSystem.Collections.Generic.List<MeshRenderer>();

            foreach (var renderer in made) list.Add(renderer);

            // The ivy stays the game's own.
            var ours = new HashSet<IntPtr>(made.Select(renderer => renderer.Pointer));

            foreach (var renderer in copy.GetComponentsInChildren<MeshRenderer>(true))
                if (!ours.Contains(renderer.Pointer)) list.Add(renderer);

            animator.Renderers = list;
            animator.Skinneds = new Il2CppSystem.Collections.Generic.List<SkinnedMeshRenderer>();

            if (animator.HeadBar != null)
            {
                var bar = animator.HeadBar.transform;

                bar.position = new UnityEngine.Vector3(bar.position.x, top.y + HeadBarAboveTop, bar.position.z);
            }
        }
        else
        {
            _log($"the stock prefab has no model animator; the {look.Name} may not light up when picked");
        }

        _log($"{look.Name} model built: {model.TriangleCount} triangles, {made.Count} meshes, {model.Materials.Count} materials, {height:0.00} m tall");
    }

    /// <summary>Moves the stock arcs so their bolts leave from the top of the model, not from the middle of it.</summary>
    private static void RaiseArcs(Transform root, float topY)
    {
        var arcs = root.GetComponentsInChildren<Transform>(true).FirstOrDefault(part => part.name == ArcName);

        if (arcs == null) return;

        var bolts = arcs.GetComponentsInChildren<Transform>(true).FirstOrDefault(part => part.name == BoltsName);
        var boltsAbove = bolts == null ? 0f : bolts.position.y - arcs.position.y;

        arcs.position = new UnityEngine.Vector3(arcs.position.x, topY - BoltsBelowTop - boltsAbove, arcs.position.z);
    }

    private static (Numerics.Vector3 Min, Numerics.Vector3 Max) PartBounds(ModelPart part)
    {
        var min = new Numerics.Vector3(float.MaxValue);
        var max = new Numerics.Vector3(float.MinValue);

        foreach (var mesh in part.Meshes)
            foreach (var position in mesh.Positions)
            {
                min = Numerics.Vector3.Min(min, position + part.Pivot);
                max = Numerics.Vector3.Max(max, position + part.Pivot);
            }

        if (min.X > max.X) throw new InvalidOperationException($"the model's {part.Name} has no meshes");

        return (min, max);
    }

    private static MeshRenderer MakeMesh(Transform parent, string name, ModelMesh source, Material material, int layer)
    {
        var piece = new GameObject(name) { layer = layer };

        piece.transform.SetParent(parent, false);

        var mesh = new Mesh { name = name };

        if (source.Positions.Length > 65535) mesh.indexFormat = IndexFormat.UInt32;

        mesh.vertices = source.Positions.Select(Unity).ToArray();
        mesh.normals = source.Normals.Select(Unity).ToArray();
        mesh.uv = source.Uvs.Select(uv => new UnityEngine.Vector2(uv.X, uv.Y)).ToArray();
        mesh.triangles = source.Triangles;
        mesh.RecalculateBounds();
        mesh.UploadMeshData(true);

        piece.AddComponent<MeshFilter>().sharedMesh = mesh;

        var renderer = piece.AddComponent<MeshRenderer>();

        renderer.sharedMaterial = material;

        return renderer;
    }

    private static Material MakeMaterial(Material stock, ModelMaterial source, IReadOnlyList<byte[]> images)
    {
        var material = new Material(stock) { name = "DefensesExpanded_" + source.Name };

        foreach (var map in new[] { "_BumpMap", "_MetallicGlossMap", "_OcclusionMap", "_EmissionMap", "_DetailAlbedoMap", "_DetailNormalMap", "_SpecGlossMap" })
            if (material.HasProperty(map)) material.SetTexture(map, null);

        foreach (var keyword in new[] { "_NORMALMAP", "_METALLICSPECGLOSSMAP", "_OCCLUSIONMAP", "_EMISSION", "_DETAIL_MULX2", "_SPECGLOSSMAP" }) material.DisableKeyword(keyword);

        Texture texture = source.Image is int image ? MakeTexture(source.Name, Png.Decode(images[image])) : Texture2D.whiteTexture;

        foreach (var map in new[] { "_BaseMap", "_MainTex" })
        {
            if (!material.HasProperty(map)) continue;

            material.SetTexture(map, texture);
            material.SetTextureScale(map, UnityEngine.Vector2.one);
            material.SetTextureOffset(map, UnityEngine.Vector2.zero);
        }

        // glTF colours are linear; Unity takes material colours as sRGB and converts them itself.
        var colour = new Color(source.BaseColor.X, source.BaseColor.Y, source.BaseColor.Z, source.BaseColor.W).gamma;

        foreach (var name in new[] { "_BaseColor", "_Color" })
            if (material.HasProperty(name)) material.SetColor(name, colour);

        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", source.Metallic);

        foreach (var name in new[] { "_Smoothness", "_Glossiness" })
            if (material.HasProperty(name)) material.SetFloat(name, 1f - source.Roughness);

        var bright = Math.Max(source.Emissive.X, Math.Max(source.Emissive.Y, source.Emissive.Z));

        if (bright > 0f && material.HasProperty("_EmissionColor"))
        {
            var glow = new Color(source.Emissive.X / bright, source.Emissive.Y / bright, source.Emissive.Z / bright).gamma * bright;

            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.None;
            material.SetColor("_EmissionColor", glow);
        }

        return material;
    }

    /// <summary>Png rows run top down and Unity's bottom up, and the uvs flip the same way: the two cancel.</summary>
    private static Texture2D MakeTexture(string name, Picture picture)
    {
        var texture = new Texture2D(picture.Width, picture.Height, TextureFormat.RGBA32, true, false)
        {
            name = "DefensesExpanded_" + name,
            wrapMode = TextureWrapMode.Repeat,
            filterMode = FilterMode.Trilinear,
        };
        var pixels = new Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppStructArray<Color32>(picture.Width * picture.Height);

        for (var p = 0; p < pixels.Length; p++)
            pixels[p] = new Color32(picture.Rgba[p * 4], picture.Rgba[p * 4 + 1], picture.Rgba[p * 4 + 2], picture.Rgba[p * 4 + 3]);

        texture.SetPixels32(pixels);
        texture.Apply(true, true);

        return texture;
    }

    private static void Fit(BoxCollider collider, UnityEngine.Vector3 worldMin, UnityEngine.Vector3 worldMax)
    {
        var min = new UnityEngine.Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var max = new UnityEngine.Vector3(float.MinValue, float.MinValue, float.MinValue);

        foreach (var corner in Corners(worldMin, worldMax))
        {
            var point = collider.transform.InverseTransformPoint(corner);

            min = UnityEngine.Vector3.Min(min, point);
            max = UnityEngine.Vector3.Max(max, point);
        }

        collider.center = (min + max) / 2f;
        collider.size = max - min;
    }

    private static IEnumerable<UnityEngine.Vector3> Corners(UnityEngine.Vector3 a, UnityEngine.Vector3 b)
    {
        for (var i = 0; i < 8; i++) yield return new UnityEngine.Vector3((i & 1) == 0 ? a.x : b.x, (i & 2) == 0 ? a.y : b.y, (i & 4) == 0 ? a.z : b.z);
    }

    private static UnityEngine.Vector3 Divide(UnityEngine.Vector3 v, UnityEngine.Vector3 by) => new(v.x / by.x, v.y / by.y, v.z / by.z);

    private static UnityEngine.Vector3 Unity(Numerics.Vector3 v) => new(v.X, v.Y, v.Z);

    private sealed class Look
    {
        public Look(string name, string resName, string stockPrefab, string modelFile)
        {
            Name = name;
            ResName = resName;
            ModelFile = modelFile;
            Path = Folder + resName;
            StockPath = Folder + stockPrefab;
        }

        public string Name { get; }

        public string ResName { get; }

        public string ModelFile { get; }

        public string Path { get; }

        public string StockPath { get; }

        public GameObject? Template { get; set; }

        public Model? Model { get; set; }

        public bool HasFailed { get; set; }
    }

    internal static byte[] Resource(string name)
    {
        using var stream = typeof(DefenseModel).Assembly.GetManifestResourceStream(name) ?? throw new FileNotFoundException("the mod's DLL is missing " + name);
        using var copy = new MemoryStream();

        stream.CopyTo(copy);

        return copy.ToArray();
    }

    private static void Patch(Harmony harmony, System.Reflection.MethodInfo? target, string? prefix = null)
    {
        if (target == null)
        {
            _log("cannot find a model loading method; the defenses may look like the stock ones");

            return;
        }

        try
        {
            harmony.Patch(target, prefix: prefix == null ? null : new HarmonyMethod(typeof(DefenseModel), prefix));
        }
        catch (Exception ex)
        {
            _log($"cannot hook {target.DeclaringType?.Name}.{target.Name}: {ex.Message}");
        }
    }
}
