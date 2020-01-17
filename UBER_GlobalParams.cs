// UBER_GlobalParams
using UnityEngine;

[AddComponentMenu("UBER/Global Params")]
[ExecuteInEditMode]
public class UBER_GlobalParams : MonoBehaviour
{
	public const float DEFROST_RATE = 0.3f;

	public const float RAIN_DAMP_ON_FREEZE_RATE = 0.2f;

	public const float FROZEN_FLOW_BUMP_STRENGTH = 0.1f;

	public const float FROST_RATE = 0.3f;

	public const float FROST_RATE_BUMP = 0.5f;

	public const float RAIN_TO_WATER_LEVEL_RATE = 2f;

	public const float RAIN_TO_WET_AMOUNT_RATE = 3f;

	public const float WATER_EVAPORATION_RATE = 0.001f;

	public const float WET_EVAPORATION_RATE = 0.0003f;

	public const float SNOW_FREEZE_RATE = 0.05f;

	public const float SNOW_INCREASE_RATE = 0.01f;

	public const float SNOW_MELT_RATE = 0.03f;

	public const float SNOW_MELT_RATE_BY_RAIN = 0.05f;

	public const float SNOW_DECREASE_RATE = 0.01f;

	[Header("Global Water & Rain")]
	[Tooltip("You can control global water level (multiplied by material value)")]
	[Range(0f, 1f)]
	public float WaterLevel = 1f;

	[Tooltip("You can control global wetness (multiplied by material value)")]
	[Range(0f, 1f)]
	public float WetnessAmount = 1f;

	[Tooltip("Time scale for flow animation")]
	public float flowTimeScale = 1f;

	[Tooltip("Multiplier of water flow ripple normalmap")]
	[Range(0f, 1f)]
	public float FlowBumpStrength = 1f;

	[Tooltip("You can control global rain intensity")]
	[Range(0f, 1f)]
	public float RainIntensity = 1f;

	[Header("Global Snow")]
	[Tooltip("You can control global snow")]
	[Range(0f, 1f)]
	public float SnowLevel = 1f;

	[Tooltip("You can control global frost")]
	[Range(0f, 1f)]
	public float Frost = 1f;

	[Tooltip("Global snow dissolve value")]
	[Range(0f, 4f)]
	public float SnowDissolve = 2f;

	[Tooltip("Global snow dissolve value")]
	[Range(0.001f, 0.2f)]
	public float SnowBumpMicro = 0.08f;

	[Tooltip("Global snow spec (RGB) & Gloss (A)")]
	public Color SnowSpecGloss = new Color(0.1f, 0.1f, 0.1f, 0.15f);

	[Tooltip("Global snow glitter color/spec boost")]
	public Color SnowGlitterColor = new Color(0.8f, 0.8f, 0.8f, 0.2f);

	[Header("Global Snow - cover state")]
	[HideInInspector]
	[Range(0f, 4f)]
	public float SnowDissolveCover = 2f;

	[Tooltip("Global snow dissolve value")]
	[HideInInspector]
	[Range(0.001f, 0.2f)]
	public float SnowBumpMicroCover = 0.08f;

	[Tooltip("Global snow spec (RGB) & Gloss (A)")]
	[HideInInspector]
	public Color SnowSpecGlossCover = new Color(0.1f, 0.1f, 0.1f, 0.15f);

	[Tooltip("Global snow glitter color/spec boost")]
	[HideInInspector]
	public Color SnowGlitterColorCover = new Color(0.8f, 0.8f, 0.8f, 0.2f);

	[Header("Global Snow - melt state")]
	[HideInInspector]
	[Range(0f, 4f)]
	public float SnowDissolveMelt = 0.3f;

	[Tooltip("Global snow dissolve value")]
	[HideInInspector]
	[Range(0.001f, 0.2f)]
	public float SnowBumpMicroMelt = 0.02f;

	[Tooltip("Global snow spec (RGB) & Gloss (A)")]
	[HideInInspector]
	public Color SnowSpecGlossMelt = new Color(0.15f, 0.15f, 0.15f, 0.6f);

	[Tooltip("Global snow glitter color/spec boost")]
	[HideInInspector]
	public Color SnowGlitterColorMelt = new Color(0.1f, 0.1f, 0.1f, 0.03f);

	[Header("Rainfall/snowfall controller")]
	public bool Simulate;

	[Range(0f, 1f)]
	public float fallIntensity;

	[Tooltip("Temperature (influences melt/freeze/evaporation speed) - 0 means water freeze")]
	[Range(-50f, 50f)]
	public float temperature = 20f;

	[Tooltip("Wind (1 means 4x faster evaporation and freeze rate)")]
	[Range(0f, 1f)]
	public float wind;

	[Tooltip("Speed of surface state change due to the weather dynamics")]
	[Range(0f, 1f)]
	public float weatherTimeScale = 1f;

	[Tooltip("We won't melt ice nor decrease water level while snow level is >5%")]
	public bool FreezeWetWhenSnowPresent = true;

	[Tooltip("Increase global Water level when snow appears")]
	public bool AddWetWhenSnowPresent = true;

	[Space(10f)]
	[Tooltip("Set to show and adjust below particle systems")]
	public bool UseParticleSystem = true;

	[Tooltip("GameObject with particle system attached controlling rain")]
	public GameObject rainGameObject;

	[Tooltip("GameObject with particle system attached controlling snow")]
	public GameObject snowGameObject;

	private Vector4 __Time;

	private float lTime;

	private bool paricleSystemActive;

	private ParticleSystem psRain;

	private ParticleSystem psSnow;

	private void Update()
	{
		AdvanceTime(Time.deltaTime);
	}

	private void Start()
	{
		SetupIt();
	}

	public void SetupIt()
	{
		Shader.SetGlobalFloat("_UBER_GlobalDry", 1f - WaterLevel);
		Shader.SetGlobalFloat("_UBER_GlobalDryConst", 1f - WetnessAmount);
		Shader.SetGlobalFloat("_UBER_GlobalRainDamp", 1f - RainIntensity);
		Shader.SetGlobalFloat("_UBER_RippleStrength", FlowBumpStrength);
		Shader.SetGlobalFloat("_UBER_GlobalSnowDamp", 1f - SnowLevel);
		Shader.SetGlobalFloat("_UBER_Frost", 1f - Frost);
		Shader.SetGlobalFloat("_UBER_GlobalSnowDissolve", SnowDissolve);
		Shader.SetGlobalFloat("_UBER_GlobalSnowBumpMicro", SnowBumpMicro);
		Shader.SetGlobalColor("_UBER_GlobalSnowSpecGloss", SnowSpecGloss);
		Shader.SetGlobalColor("_UBER_GlobalSnowGlitterColor", SnowGlitterColor);
	}

	public void AdvanceTime(float amount)
	{
		SimulateDynamicWeather(amount * weatherTimeScale);
		amount *= flowTimeScale;
		__Time.x += amount / 20f;
		__Time.y += amount;
		__Time.z += amount * 2f;
		__Time.w += amount * 3f;
		Shader.SetGlobalVector("UBER_Time", __Time);
	}

	public void SimulateDynamicWeather(float dt)
	{
		if (dt == 0f || !Simulate)
		{
			return;
		}
		float rainIntensity = RainIntensity;
		float propA = temperature;
		float propA2 = flowTimeScale;
		float flowBumpStrength = FlowBumpStrength;
		float waterLevel = WaterLevel;
		float wetnessAmount = WetnessAmount;
		float snowLevel = SnowLevel;
		float snowDissolve = SnowDissolve;
		float snowBumpMicro = SnowBumpMicro;
		Color snowSpecGloss = SnowSpecGloss;
		Color snowGlitterColor = SnowGlitterColor;
		float num = wind * 4f + 1f;
		float num2 = FreezeWetWhenSnowPresent ? Mathf.Clamp01((0.05f - SnowLevel) / 0.05f) : 1f;
		if (temperature > 0f)
		{
			float num3 = temperature + 10f;
			RainIntensity = fallIntensity * num2;
			flowTimeScale += dt * num3 * 0.3f * num2;
			if (flowTimeScale > 1f)
			{
				flowTimeScale = 1f;
			}
			FlowBumpStrength += dt * num3 * 0.3f * num2;
			if (FlowBumpStrength > 1f)
			{
				FlowBumpStrength = 1f;
			}
			WaterLevel += RainIntensity * dt * 2f * num2;
			if (WaterLevel > 1f)
			{
				WaterLevel = 1f;
			}
			WetnessAmount += RainIntensity * dt * 3f * num2;
			if (WetnessAmount > 1f)
			{
				WetnessAmount = 1f;
			}
			float num4 = Mathf.Abs(dt * num3 * 0.03f + dt * RainIntensity * 0.05f);
			SnowDissolve = TargetValue(SnowDissolve, SnowDissolveMelt, num4 * 2f);
			SnowBumpMicro = TargetValue(SnowBumpMicro, SnowBumpMicroMelt, num4 * 0.1f);
			SnowSpecGloss.r = TargetValue(SnowSpecGloss.r, SnowSpecGlossMelt.r, num4);
			SnowSpecGloss.g = TargetValue(SnowSpecGloss.g, SnowSpecGlossMelt.g, num4);
			SnowSpecGloss.b = TargetValue(SnowSpecGloss.b, SnowSpecGlossMelt.b, num4);
			SnowSpecGloss.a = TargetValue(SnowSpecGloss.a, SnowSpecGlossMelt.a, num4);
			SnowGlitterColor.r = TargetValue(SnowGlitterColor.r, SnowGlitterColorMelt.r, num4);
			SnowGlitterColor.g = TargetValue(SnowGlitterColor.g, SnowGlitterColorMelt.g, num4);
			SnowGlitterColor.b = TargetValue(SnowGlitterColor.b, SnowGlitterColorMelt.b, num4);
			SnowGlitterColor.a = TargetValue(SnowGlitterColor.a, SnowGlitterColorMelt.a, num4);
			Frost -= dt * num3 * 0.3f * num2;
			if (Frost < 0f)
			{
				Frost = 0f;
			}
			SnowLevel -= dt * num3 * 0.01f;
			if (SnowLevel < 0f)
			{
				SnowLevel = 0f;
			}
		}
		else
		{
			float num5 = temperature - 10f;
			RainIntensity += dt * num5 * 0.2f;
			if (RainIntensity < 0f)
			{
				RainIntensity = 0f;
			}
			flowTimeScale += dt * num5 * 0.3f * num;
			if (flowTimeScale < 0f)
			{
				flowTimeScale = 0f;
			}
			if (FlowBumpStrength > 0.1f)
			{
				FlowBumpStrength += dt * num5 * 0.5f * flowTimeScale;
				if (FlowBumpStrength < 0.1f)
				{
					FlowBumpStrength = 0.1f;
				}
			}
			float num6 = Mathf.Abs(dt * num5 * 0.05f) * fallIntensity;
			SnowDissolve = TargetValue(SnowDissolve, SnowDissolveCover, num6 * 2f);
			SnowBumpMicro = TargetValue(SnowBumpMicro, SnowBumpMicroCover, num6 * 0.1f);
			SnowSpecGloss.r = TargetValue(SnowSpecGloss.r, SnowSpecGlossCover.r, num6);
			SnowSpecGloss.g = TargetValue(SnowSpecGloss.g, SnowSpecGlossCover.g, num6);
			SnowSpecGloss.b = TargetValue(SnowSpecGloss.b, SnowSpecGlossCover.b, num6);
			SnowSpecGloss.a = TargetValue(SnowSpecGloss.a, SnowSpecGlossCover.a, num6);
			SnowGlitterColor.r = TargetValue(SnowGlitterColor.r, SnowGlitterColorCover.r, num6);
			SnowGlitterColor.g = TargetValue(SnowGlitterColor.g, SnowGlitterColorCover.g, num6);
			SnowGlitterColor.b = TargetValue(SnowGlitterColor.b, SnowGlitterColorCover.b, num6);
			SnowGlitterColor.a = TargetValue(SnowGlitterColor.a, SnowGlitterColorCover.a, num6);
			Frost -= dt * num5 * 0.3f;
			if (Frost > 1f)
			{
				Frost = 1f;
			}
			SnowLevel -= fallIntensity * (dt * num5 * 0.01f);
			if (SnowLevel > 1f)
			{
				SnowLevel = 1f;
			}
			if (AddWetWhenSnowPresent && WaterLevel < SnowLevel)
			{
				WaterLevel = SnowLevel;
			}
		}
		WaterLevel -= num * (temperature + 273f) * 0.001f * flowTimeScale * dt * num2;
		if (WaterLevel < 0f)
		{
			WaterLevel = 0f;
		}
		WetnessAmount -= num * (temperature + 273f) * 0.0003f * flowTimeScale * dt * num2;
		if (WetnessAmount < 0f)
		{
			WetnessAmount = 0f;
		}
		RefreshParticleSystem();
		bool flag = false;
		if (compareDelta(rainIntensity, RainIntensity))
		{
			flag = true;
		}
		else if (compareDelta(propA, temperature))
		{
			flag = true;
		}
		else if (compareDelta(propA2, flowTimeScale))
		{
			flag = true;
		}
		else if (compareDelta(flowBumpStrength, FlowBumpStrength))
		{
			flag = true;
		}
		else if (compareDelta(waterLevel, WaterLevel))
		{
			flag = true;
		}
		else if (compareDelta(wetnessAmount, WetnessAmount))
		{
			flag = true;
		}
		else if (compareDelta(snowLevel, SnowLevel))
		{
			flag = true;
		}
		else if (compareDelta(snowDissolve, SnowDissolve))
		{
			flag = true;
		}
		else if (compareDelta(snowBumpMicro, SnowBumpMicro))
		{
			flag = true;
		}
		else if (compareDelta(snowSpecGloss, SnowSpecGloss))
		{
			flag = true;
		}
		else if (compareDelta(snowGlitterColor, SnowGlitterColor))
		{
			flag = true;
		}
		if (flag)
		{
			SetupIt();
		}
	}

	private bool compareDelta(float propA, float propB)
	{
		return Mathf.Abs(propA - propB) > 1E-06f;
	}

	private bool compareDelta(Color propA, Color propB)
	{
		if (Mathf.Abs(propA.r - propB.r) > 1E-06f)
		{
			return true;
		}
		if (Mathf.Abs(propA.g - propB.g) > 1E-06f)
		{
			return true;
		}
		if (Mathf.Abs(propA.b - propB.b) > 1E-06f)
		{
			return true;
		}
		if (Mathf.Abs(propA.a - propB.a) > 1E-06f)
		{
			return true;
		}
		return false;
	}

	private float TargetValue(float val, float target_val, float delta)
	{
		if (val < target_val)
		{
			val += delta;
			if (val > target_val)
			{
				val = target_val;
			}
		}
		else if (val > target_val)
		{
			val -= delta;
			if (val < target_val)
			{
				val = target_val;
			}
		}
		return val;
	}

	public void RefreshParticleSystem()
	{
		if (paricleSystemActive != UseParticleSystem)
		{
			if ((bool)rainGameObject)
			{
				rainGameObject.SetActive(UseParticleSystem);
			}
			if ((bool)snowGameObject)
			{
				snowGameObject.SetActive(UseParticleSystem);
			}
			paricleSystemActive = UseParticleSystem;
		}
		if (!UseParticleSystem)
		{
			return;
		}
		if (rainGameObject != null)
		{
			rainGameObject.transform.position = base.transform.position + Vector3.up * 3f;
			if (psRain == null)
			{
				psRain = rainGameObject.GetComponent<ParticleSystem>();
			}
		}
		if (snowGameObject != null)
		{
			snowGameObject.transform.position = base.transform.position + Vector3.up * 3f;
			if (psSnow == null)
			{
				psSnow = snowGameObject.GetComponent<ParticleSystem>();
			}
		}
		if (psRain != null)
		{
			ParticleSystem.EmissionModule emission = psRain.emission;
			ParticleSystem.MinMaxCurve minMaxCurve2 = emission.rateOverTime = new ParticleSystem.MinMaxCurve(fallIntensity * 3000f * Mathf.Clamp01(temperature + 1f));
		}
		if (psSnow != null)
		{
			ParticleSystem.EmissionModule emission2 = psSnow.emission;
			ParticleSystem.MinMaxCurve minMaxCurve4 = emission2.rateOverTime = new ParticleSystem.MinMaxCurve(fallIntensity * 3000f * Mathf.Clamp01(1f - temperature));
		}
	}
}
