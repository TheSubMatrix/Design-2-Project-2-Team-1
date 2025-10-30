using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class VFXBuilder
{
    readonly VFXPooler vfxSpawner;
    VisualEffectAsset m_asset;
    Vector3 m_position = Vector3.zero;
    Quaternion m_rotation = Quaternion.identity;
    Transform m_followTarget;
    Vector3 m_scale = Vector3.one;
    readonly Dictionary<string, object> m_parameters = new();
    bool m_autoReturn = true;
    float m_lifetime = -1f;

    public VFXBuilder(VFXPooler vfxSpawner)
    {
        this.vfxSpawner = vfxSpawner;
    }

    public VFXBuilder WithAsset(VisualEffectAsset asset)
    {
        m_asset = asset;
        return this;
    }

    public VFXBuilder AtPosition(Vector3 position)
    {
        m_position = position;
        return this;
    }

    public VFXBuilder AtPosition(float x, float y, float z)
    {
        m_position = new Vector3(x, y, z);
        return this;
    }

    public VFXBuilder WithRotation(Quaternion rotation)
    {
        m_rotation = rotation;
        return this;
    }

    public VFXBuilder WithRotation(Vector3 eulerAngles)
    {
        m_rotation = Quaternion.Euler(eulerAngles);
        return this;
    }

    public VFXBuilder FollowTransform(Transform target)
    {
        m_followTarget = target;
        return this;
    }

    public VFXBuilder WithScale(Vector3 scale)
    {
        m_scale = scale;
        return this;
    }

    public VFXBuilder WithScale(float uniformScale)
    {
        m_scale = Vector3.one * uniformScale;
        return this;
    }

    public VFXBuilder WithParameter(string name, object value)
    {
        m_parameters[name] = value;
        return this;
    }

    public VFXBuilder WithAutoReturn(bool autoReturn)
    {
        m_autoReturn = autoReturn;
        return this;
    }

    public VFXBuilder WithLifetime(float lifetime)
    {
        m_lifetime = lifetime;
        return this;
    }

    public VisualEffect Play()
    {
        if (m_asset == null)
        {
            Debug.LogError("VFXBuilder: No asset specified. Use WithAsset() before calling Play().");
            return null;
        }

        if (!vfxSpawner.ParticlePools.Dictionary.TryGetValue(m_asset, out VFXPool pool))
        {
            Debug.LogError($"VFXBuilder: No pool found for asset {m_asset.name}");
            return null;
        }

        VisualEffect vfx = pool.Pool.Get();
        if (vfx == null)
        {
            Debug.LogError($"VFXBuilder: Failed to get VFX from pool for asset {m_asset.name}");
            return null;
        }

        // VFX stays as child of pooler - never reparent
        vfx.transform.SetParent(vfxSpawner.transform);
        vfx.transform.position = m_position;
        vfx.transform.rotation = m_rotation;
        vfx.transform.localScale = m_scale;

        // Set parameters
        foreach (KeyValuePair<string, object> param in m_parameters)
        {
            SetVFXParameter(vfx, param.Key, param.Value);
        }

        // Play the effect
        vfx.Play();

        // Handle transform following if target is set
        VFXFollower follower = vfx.GetComponent<VFXFollower>();
        if (follower == null)
        {
            follower = vfx.gameObject.AddComponent<VFXFollower>();
        }

        if (m_followTarget != null)
        {
            follower.SetTarget(m_followTarget);
        }
        else
        {
            follower.ClearTarget();
        }

        // Handle auto-return only for non-looping effects
        bool isLooping = IsVFXLooping(vfx);
        if (m_autoReturn && !isLooping)
        {
            float duration = m_lifetime > 0 ? m_lifetime : GetVFXDuration(vfx);
            if (duration > 0)
            {
                vfxSpawner.ReturnVFXAfterDelay(vfx, m_asset, duration);
            }
        }

        // Reset builder for next use
        Reset();

        return vfx;
    }

    static void SetVFXParameter(VisualEffect vfx, string name, object value)
    {
        switch (value)
        {
            case int intValue:
                vfx.SetInt(name, intValue);
                break;
            case float floatValue:
                vfx.SetFloat(name, floatValue);
                break;
            case Vector3 vec3Value:
                vfx.SetVector3(name, vec3Value);
                break;
            case Vector4 vec4Value:
                vfx.SetVector4(name, vec4Value);
                break;
            case Color colorValue:
                vfx.SetVector4(name, colorValue);
                break;
            case bool boolValue:
                vfx.SetBool(name, boolValue);
                break;
            case Texture textureValue:
                vfx.SetTexture(name, textureValue);
                break;
            default:
                Debug.LogWarning($"VFXBuilder: Unsupported parameter type {value.GetType()} for parameter {name}");
                break;
        }
    }

    static float GetVFXDuration(VisualEffect vfx)
    {
        return vfx.HasFloat("Duration") ? vfx.GetFloat("Duration") : 2f;
    }

    static bool IsVFXLooping(VisualEffect vfx)
    {
        // Check if the VFX has a boolean parameter for looping
        if (vfx.HasBool("Loop"))
        {
            return vfx.GetBool("Loop");
        }
        
        // Check spawn system info for playing state that indicates looping
        try
        {
            VFXSpawnerState state = vfx.GetSpawnSystemInfo(0);
            // If spawn rate is > 0 and it's playing indefinitely, it's likely looping
            return state.playing && state.spawnCount == 0;
        }
        catch
        {
            return false;
        }
    }

    void Reset()
    {
        m_asset = null;
        m_position = Vector3.zero;
        m_rotation = Quaternion.identity;
        m_followTarget = null;
        m_scale = Vector3.one;
        m_parameters.Clear();
        m_autoReturn = true;
        m_lifetime = -1f;
    }
}