using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class DustParticle : MonoBehaviour
{
    void Awake()
    {
        var ps = GetComponent<ParticleSystem>();
        var renderer = GetComponent<ParticleSystemRenderer>();

        // --- Main ---
        var main = ps.main;
        main.loop              = true;
        main.playOnAwake       = true;
        main.maxParticles      = 300;
        main.simulationSpace   = ParticleSystemSimulationSpace.World;
        main.startLifetime     = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSpeed        = new ParticleSystem.MinMaxCurve(0.5f, 2f);
        main.startSize         = new ParticleSystem.MinMaxCurve(0.3f, 1.2f);
        main.startRotation     = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
        main.gravityModifier   = 0.02f;
        main.startColor        = new Color(0.87f, 0.72f, 0.53f, 0.5f);

        // --- Emission ---
        var emission = ps.emission;
        emission.rateOverTime  = 60f;

        // --- Shape ---
        var shape = ps.shape;
        shape.shapeType        = ParticleSystemShapeType.Box;
        shape.scale            = new Vector3(8f, 0.5f, 1f);

        // --- Color Over Lifetime (fade in/out) ---
        var col = ps.colorOverLifetime;
        col.enabled = true;
        var grad = new Gradient();
        grad.SetKeys(
            new[] { new GradientColorKey(new Color(0.87f, 0.72f, 0.53f), 0f),
                    new GradientColorKey(new Color(0.75f, 0.60f, 0.42f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.6f, 0.2f),
                    new GradientAlphaKey(0.6f, 0.7f),
                    new GradientAlphaKey(0f, 1f) }
        );
        col.color = new ParticleSystem.MinMaxGradient(grad);

        // --- Size Over Lifetime ---
        var sol = ps.sizeOverLifetime;
        sol.enabled = true;
        sol.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.2f, 1f),
            new Keyframe(1f, 0.8f)
        ));

        // --- Velocity (wind) ---
        var vol = ps.velocityOverLifetime;
        vol.enabled = true;
        vol.space   = ParticleSystemSimulationSpace.World;
        vol.x       = new ParticleSystem.MinMaxCurve(2.5f);
        vol.y       = new ParticleSystem.MinMaxCurve(0.2f);

        // --- Noise (turbulence) ---
        var noise = ps.noise;
        noise.enabled   = true;
        noise.strength  = 1.2f;
        noise.frequency = 0.5f;
        noise.scrollSpeed = 0.3f;
        noise.quality   = ParticleSystemNoiseQuality.Medium;

        // --- Rotation Over Lifetime ---
        var rol = ps.rotationOverLifetime;
        rol.enabled = true;
        rol.z = new ParticleSystem.MinMaxCurve(-20f * Mathf.Deg2Rad, 20f * Mathf.Deg2Rad);

        // --- Material ---
        var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                  ?? Shader.Find("Particles/Standard Unlit")
                  ?? Shader.Find("Standard");
        var mat = new Material(shader);
        if (mat.HasProperty("_Surface"))  mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_ZWrite"))   mat.SetFloat("_ZWrite", 0f);
        mat.renderQueue = 3000;
        mat.SetColor("_BaseColor", new Color(0.87f, 0.72f, 0.53f, 0.5f));
        renderer.material     = mat;
        renderer.renderMode   = ParticleSystemRenderMode.Billboard;

        ps.Play();
    }
}
