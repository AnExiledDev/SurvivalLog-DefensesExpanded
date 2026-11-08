using System;
using System.Collections.Generic;
using DefensesExpanded.Core;
using UnityEngine;

namespace DefensesExpanded.Adapters;

/// <summary>
/// Turns a turret's head toward the zombie it last shot and kicks it back a little on each shot. The game tells us of
/// each shot through its strike effect (<see cref="FxPatches"/>); between shots the head keeps tracking that zombie
/// until it is gone. The head's barrel points along the model's +Z, so its heading is measured from there.
/// </summary>
internal static class TurretAim
{
    private const float TurnDegreesPerSecond = 300f;
    private const float RecoilSeconds = 0.15f;
    private const float RecoilMeters = 0.05f;

    // A turret not heard from in this long (packed up, broken, the run left) is dropped.
    private const float ForgetAfterSeconds = 30f;

    private static readonly Dictionary<IntPtr, Aimer> Aimers = new();
    private static readonly List<IntPtr> Gone = new();

    /// <summary>Records a shot from a turret at a zombie, by their show objects.</summary>
    public static void Fired(GameObject? turret, GameObject? zombie, float now)
    {
        if (turret == null) return;

        if (!Aimers.TryGetValue(turret.Pointer, out var aimer))
        {
            aimer = new Aimer(turret, FindHeads(turret.transform));
            Aimers[turret.Pointer] = aimer;
        }

        aimer.Target = zombie == null ? null : zombie.transform;
        aimer.FiredAt = now;
    }

    /// <summary>Moves every known turret's heads for this frame. Returns at once when there are none.</summary>
    public static void Update(float now, float deltaSeconds)
    {
        if (Aimers.Count == 0) return;

        foreach (var pair in Aimers)
        {
            var aimer = pair.Value;

            if (aimer.Turret == null || aimer.Turret.WasCollected || aimer.Heads.Length == 0 || now - aimer.FiredAt > ForgetAfterSeconds)
            {
                Gone.Add(pair.Key);

                continue;
            }

            if (aimer.Target != null && aimer.Target.WasCollected) aimer.Target = null;

            var kick = now - aimer.FiredAt < RecoilSeconds ? RecoilMeters * (1f - (now - aimer.FiredAt) / RecoilSeconds) : 0f;

            for (var h = 0; h < aimer.Heads.Length; h++)
            {
                var head = aimer.Heads[h];

                if (head == null || head.WasCollected || !head.gameObject.activeInHierarchy) continue;

                if (aimer.Target != null)
                {
                    var toward = head.parent.InverseTransformPoint(aimer.Target.position) - aimer.Rest[h];

                    if (toward.x * toward.x + toward.z * toward.z > 0.0001f)
                        aimer.Yaw[h] = Aim.Turn(aimer.Yaw[h], Aim.Heading(toward.x, toward.z), TurnDegreesPerSecond * deltaSeconds);
                }

                var turn = Quaternion.Euler(0f, aimer.Yaw[h], 0f);

                head.localRotation = turn;
                head.localPosition = aimer.Rest[h] - turn * Vector3.forward * kick;
            }
        }

        if (Gone.Count == 0) return;

        foreach (var key in Gone) Aimers.Remove(key);

        Gone.Clear();
    }

    private static Transform[] FindHeads(Transform root)
    {
        var heads = new List<Transform>();

        foreach (var part in root.GetComponentsInChildren<Transform>(true))
            if (part.name.EndsWith(DefenseModel.HeadSuffix, StringComparison.Ordinal)) heads.Add(part);

        return heads.ToArray();
    }

    private sealed class Aimer
    {
        public Aimer(GameObject turret, Transform[] heads)
        {
            Turret = turret;
            Heads = heads;
            Rest = new Vector3[heads.Length];
            Yaw = new float[heads.Length];

            for (var h = 0; h < heads.Length; h++)
            {
                Rest[h] = heads[h].localPosition;
                Yaw[h] = heads[h].localEulerAngles.y;
            }
        }

        public GameObject Turret { get; }
        public Transform[] Heads { get; }
        public Vector3[] Rest { get; }
        public float[] Yaw { get; }
        public Transform? Target { get; set; }
        public float FiredAt { get; set; }
    }
}
