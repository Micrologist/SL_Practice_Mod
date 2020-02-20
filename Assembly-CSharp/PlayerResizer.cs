using System;
using UnityEngine;

// Token: 0x0200021D RID: 541
public class PlayerResizer : MonoBehaviour
{
	// Token: 0x06000996 RID: 2454 RVA: 0x00007F3A File Offset: 0x0000613A
	private void Awake()
	{
		this._CacheScale();
		this.teleportStored = false;
	}

	// Token: 0x06000997 RID: 2455 RVA: 0x00040DB8 File Offset: 0x0003EFB8
	private void _EnsureNearPlaneCached()
	{
		if (this.nearPlane == null)
		{
			Camera playerCamera = GameManager.GM.playerCamera;
			this.nearPlane = new float?(playerCamera.nearClipPlane);
		}
	}

	// Token: 0x06000998 RID: 2456 RVA: 0x00040DF0 File Offset: 0x0003EFF0
	private void _CacheScale()
	{
		GameObject gameObject = base.gameObject;
		this.originalLocalScale = gameObject.transform.localScale;
		FPSInputController component = gameObject.GetComponent<FPSInputController>();
		this.crouchAmount = component.crouchAmount;
		this.runSpeed = component.runSpeed;
		this.walkSpeed = component.walkSpeed;
		CharacterMotor component2 = gameObject.GetComponent<CharacterMotor>();
		this.jumpBaseHeight = component2.jumping.baseHeight;
		this.jumpExtraHeight = component2.jumping.extraHeight;
		this.gravity = component2.movement.gravity;
		this.maxGroundAcceleration = component2.movement.maxGroundAcceleration;
		this.maxAirAcceleration = component2.movement.maxAirAcceleration;
		CharacterController component3 = gameObject.GetComponent<CharacterController>();
		this.stepOffset = component3.stepOffset;
		this.skinWidth = component3.skinWidth;
		PlayerMovementSoundController componentInChildren = gameObject.GetComponentInChildren<PlayerMovementSoundController>();
		this.sizeRatio = componentInChildren.GetSizeRatio();
	}

	// Token: 0x06000999 RID: 2457 RVA: 0x00007F49 File Offset: 0x00006149
	public void ReturnToNormalSize()
	{
		base.gameObject.transform.localScale = this.originalLocalScale;
		this.Poke();
	}

	// Token: 0x0600099A RID: 2458 RVA: 0x00040ED0 File Offset: 0x0003F0D0
	public void EnsurePlayerNoLargerThanStandardSize()
	{
		if (base.gameObject.transform.localScale.sqrMagnitude > this.originalLocalScale.sqrMagnitude)
		{
			this.ReturnToNormalSize();
		}
	}

	// Token: 0x0600099B RID: 2459 RVA: 0x00040F08 File Offset: 0x0003F108
	public void Poke()
	{
		this._EnsureNearPlaneCached();
		GameObject gameObject = base.gameObject;
		float num = gameObject.transform.localScale.x / this.originalLocalScale.x;
		FPSInputController component = gameObject.GetComponent<FPSInputController>();
		component.crouchAmount = this.crouchAmount * num;
		component.runSpeed = this.runSpeed * num;
		component.walkSpeed = this.walkSpeed * num;
		CharacterMotor component2 = gameObject.GetComponent<CharacterMotor>();
		component2.jumping.baseHeight = this.jumpBaseHeight * num;
		component2.jumping.extraHeight = this.jumpExtraHeight * num;
		component2.movement.gravity = this.gravity * num;
		component2.movement.maxGroundAcceleration = this.maxGroundAcceleration * num;
		component2.movement.maxAirAcceleration = this.maxAirAcceleration * num;
		CharacterController component3 = gameObject.GetComponent<CharacterController>();
		component3.stepOffset = this.stepOffset * num;
		component3.skinWidth = this.skinWidth * num;
		GameManager.GM.playerCamera.nearClipPlane = this.nearPlane.Value * num;
		gameObject.GetComponentInChildren<PlayerMovementSoundController>().SetSizeRatio(this.sizeRatio * num);
	}

	// Token: 0x0600099D RID: 2461 RVA: 0x00041020 File Offset: 0x0003F220
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.F5))
		{
			this.StoreTeleport();
		}
		if (Input.GetKeyDown(KeyCode.F6))
		{
			this.TeleportToStored();
		}
		if (Input.GetKey(KeyCode.F2))
		{
			this.ScalePlayer(Time.deltaTime / 2f);
		}
		if (Input.GetKey(KeyCode.F1))
		{
			this.ScalePlayer(-Time.deltaTime / 2f);
		}
		if (Input.GetKeyDown(KeyCode.F3))
		{
			this.ReturnToNormalSize();
		}
		if (Input.GetKeyDown(KeyCode.F7) && Time.time - GameManager.GM.lastManualLoadTime >= 3f)
		{
			GameManager.GM.lastManualLoadTime = Time.time;
			GameManager.GM.GetComponent<SaveAndCheckpointManager>().ResetToLastCheckpoint();
		}
		if (Input.GetKeyDown(KeyCode.F8) && Time.time - GameManager.GM.lastManualLoadTime >= 3f)
		{
			GameManager.GM.lastManualLoadTime = Time.time;
			GameManager.GM.GetComponent<SaveAndCheckpointManager>().RestartLevel();
		}
	}

	// Token: 0x0600099E RID: 2462 RVA: 0x00041120 File Offset: 0x0003F320
	private void StoreTeleport()
	{
		Transform transform = base.gameObject.transform;
		MouseLook component = GameManager.GM.playerCamera.GetComponent<MouseLook>();
		base.gameObject.GetComponent<FPSInputController>().teleportStoreTime = Time.time;
		GameManager.GM.teleportPosition = transform.localPosition;
		GameManager.GM.teleportScale = transform.localScale;
		GameManager.GM.teleportRotation = transform.localRotation;
		GameManager.GM.teleportCameraRotationY = component.rotationY;
		GameManager.GM.teleportStored = true;
	}

	// Token: 0x0600099F RID: 2463 RVA: 0x000411AC File Offset: 0x0003F3AC
	private void TeleportToStored()
	{
		if (!GameManager.GM.teleportStored)
		{
			return;
		}
		base.gameObject.GetComponent<FPSInputController>().teleportTime = Time.time;
		Transform transform = base.gameObject.transform;
		MouseLook component = GameManager.GM.playerCamera.GetComponent<MouseLook>();
		transform.localPosition = GameManager.GM.teleportPosition;
		transform.localScale = GameManager.GM.teleportScale;
		transform.localRotation = GameManager.GM.teleportRotation;
		component.SetRotationY(GameManager.GM.teleportCameraRotationY);
		this.Poke();
	}

	// Token: 0x060009A0 RID: 2464 RVA: 0x00007F67 File Offset: 0x00006167
	private void ScalePlayer(float amount)
	{
		base.gameObject.transform.localScale *= 1f + amount;
		this.Poke();
	}

	// Token: 0x04000A72 RID: 2674
	private Vector3 originalLocalScale;

	// Token: 0x04000A73 RID: 2675
	private float crouchAmount;

	// Token: 0x04000A74 RID: 2676
	private float runSpeed;

	// Token: 0x04000A75 RID: 2677
	private float walkSpeed;

	// Token: 0x04000A76 RID: 2678
	private float jumpBaseHeight;

	// Token: 0x04000A77 RID: 2679
	private float jumpExtraHeight;

	// Token: 0x04000A78 RID: 2680
	private float gravity;

	// Token: 0x04000A79 RID: 2681
	private float maxGroundAcceleration;

	// Token: 0x04000A7A RID: 2682
	private float maxAirAcceleration;

	// Token: 0x04000A7B RID: 2683
	private float stepOffset;

	// Token: 0x04000A7C RID: 2684
	private float skinWidth;

	// Token: 0x04000A7D RID: 2685
	private float? nearPlane;

	// Token: 0x04000A7E RID: 2686
	private float sizeRatio;

	// Token: 0x04000A7F RID: 2687
	private Vector3 teleportPosition;

	// Token: 0x04000A80 RID: 2688
	private Vector3 teleportScale;

	// Token: 0x04000A81 RID: 2689
	private Quaternion teleportRotation;

	// Token: 0x04000A82 RID: 2690
	private bool teleportStored;

	// Token: 0x04000A83 RID: 2691
	private float teleportCameraRotationY;
}
