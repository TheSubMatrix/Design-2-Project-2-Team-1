using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

[System.Flags]
public enum LightChannelsToAnimate
{
	Red = 1 << 0,    // 1
	Green = 1 << 1,  // 2
	Blue = 1 << 2,    // 4
	Intensity = 1 << 3 //8
}
public enum LightWaveFunctions
{
	Sinus = 0,
	Triangle =1,
	Square =2,
	Sawtooth =3,
	InvertedSaw =4,
	Noise =5
}
[RequireComponent(typeof(Light))]
public class LightAnimator : MonoBehaviour {
	[FormerlySerializedAs("m_colorChannel")] [FormerlySerializedAs("colorChannel")] [EnumButtons]
	public LightChannelsToAnimate m_channelToAnimate;
	[FormerlySerializedAs("waveFunction")] public LightWaveFunctions m_waveFunction= LightWaveFunctions.Sinus; 
	[FormerlySerializedAs("offset")] public float m_offset =0.0f; // constant offset
	[FormerlySerializedAs("amplitude")] public float m_amplitude = 1.0f; // amplitude of the wave
	[FormerlySerializedAs("phase")] public float m_phase = 0.0f; // start point inside on wave cycle
	[FormerlySerializedAs("frequency")] public float m_frequency = 0.5f; // cycle frequency per second
	Color originalColor;
	float originalIntensity;
	Light m_light;
	void Start () 
	{
		m_light = GetComponent<Light>();
		originalColor = m_light.color;
		originalIntensity = m_light.intensity;
	}
	void Update () {
		float wave = EvalWave();
		Color newColor = originalColor;
		float newIntensity = originalIntensity;
		if ((m_channelToAnimate & LightChannelsToAnimate.Red) != 0)
		{
			newColor.r = Mathf.Clamp01(originalColor.r * wave);
		}
		if ((m_channelToAnimate & LightChannelsToAnimate.Green) != 0)
		{
			newColor.g = Mathf.Clamp01(originalColor.g * wave);
		}
		if ((m_channelToAnimate & LightChannelsToAnimate.Blue) != 0)
		{
			newColor.b = Mathf.Clamp01(originalColor.b * wave);
		}
		if ((m_channelToAnimate & LightChannelsToAnimate.Intensity) != 0)
		{
			newIntensity = Mathf.Max(0, originalIntensity * wave);
		}
    
		m_light.color = newColor;
		m_light.intensity = newIntensity;
	
	}
	
	private float EvalWave() 
	{
		float x = (Time.time + m_phase)*m_frequency;
		x -= Mathf.Floor(x); // normalized value (0..1)
	    float y = m_waveFunction switch
	    {
		    LightWaveFunctions.Sinus => Mathf.Sin(x * 2f * Mathf.PI),
		    LightWaveFunctions.Triangle when x < 0.5f => 4.0f * x - 1.0f,
		    LightWaveFunctions.Triangle => -4.0f * x + 3.0f,
		    LightWaveFunctions.Square when x < 0.5f => 1.0f,
		    LightWaveFunctions.Square => -1.0f,
		    LightWaveFunctions.Sawtooth => x,
		    LightWaveFunctions.InvertedSaw => 1.0f - x,
		    LightWaveFunctions.Noise => 1f - (Random.value * 2f),
		    _ => 1.0f
	    };
	    return (y*m_amplitude)+m_offset;     
	}
}