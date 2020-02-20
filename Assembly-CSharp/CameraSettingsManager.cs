using System;
using UnityEngine;

// Token: 0x02000029 RID: 41
public sealed class CameraSettingsManager : InfluenceVolumeManager<CameraSettingsVolume, CameraSettings>
{
	// Token: 0x17000007 RID: 7
	// (get) Token: 0x060000BC RID: 188 RVA: 0x00002D9E File Offset: 0x00000F9E
	public static CameraSettingsManager Instance
	{
		get
		{
			if (CameraSettingsManager.s_Instance == null)
			{
				CameraSettingsManager.s_Instance = new CameraSettingsManager();
			}
			return CameraSettingsManager.s_Instance;
		}
	}

	// Token: 0x060000BD RID: 189 RVA: 0x00002DB6 File Offset: 0x00000FB6
	protected override void ApplySettings(CameraSettingsVolume volume, ref CameraSettings settings, float lerpFactor)
	{
		volume.UpdateSettings(ref settings, lerpFactor);
	}

	// Token: 0x060000BE RID: 190 RVA: 0x0001E35C File Offset: 0x0001C55C
	public void UpdateSettings(Camera camera, Transform trigger)
	{
		CameraSettings cameraSettings = default(CameraSettings);
		base.UpdateSettings(trigger, ref cameraSettings);
		if (cameraSettings.FarPlane != null)
		{
			camera.farClipPlane = cameraSettings.FarPlane.Value;
			if (GameManager.GM && (GameManager.GM.noClip || GameManager.GM.unlimitedRenderRange))
			{
				camera.farClipPlane = 10000f;
			}
		}
		if (cameraSettings.FOV != null)
		{
			camera.fieldOfView = cameraSettings.FOV.Value;
		}
	}

	// Token: 0x060000BF RID: 191 RVA: 0x0001E3EC File Offset: 0x0001C5EC
	public float? GetFarClipPlaneHere(Transform trigger)
	{
		CameraSettings cameraSettings = default(CameraSettings);
		base.UpdateSettings(trigger, ref cameraSettings);
		return cameraSettings.FarPlane;
	}

	// Token: 0x040000FD RID: 253
	private static CameraSettingsManager s_Instance;
}
