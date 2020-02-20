using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

// Token: 0x02000003 RID: 3
public class DropTriggerScript : PhysicsAudio
{
	// Token: 0x06000007 RID: 7 RVA: 0x00002525 File Offset: 0x00000725
	public float GetSizeComparedToOriginal()
	{
		if (this.originalStatus != null)
		{
			return base.transform.localScale.x / this.originalStatus.scale.x;
		}
		return 1f;
	}

	// Token: 0x06000008 RID: 8 RVA: 0x0001BE50 File Offset: 0x0001A050
	private void Awake()
	{
		base._AssignAudioPriorityId(100);
		Transform[] componentsInChildren = base.GetComponentsInChildren<Transform>();
		this.children = new GameObject[componentsInChildren.Length - 1];
		int num = 0;
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i] != base.transform)
			{
				this.children[num] = componentsInChildren[i].gameObject;
				num++;
			}
		}
		this.orgChildLayers = new int[this.children.Length];
		this.orgSelfAndChildMaterials = new Material[this.children.Length + 1];
		this.originalStatus = new DropTriggerScript.OriginalStatus();
		this.originalStatus.CopyFromTransform(base.transform);
		this.originalStatus.CopyFromRigidbody(base.GetComponent<Rigidbody>());
		if ((this.grabValues.canReturn || this.grabValues.willRespawn) && base.GetComponent<CanReturnRightClick>() != null)
		{
			this.grabValues.canReturn = false;
		}
		float num2 = 1.73f;
		if (this.optionalFMODInitialSize != 0f)
		{
			num2 = this.optionalFMODInitialSize;
		}
		else
		{
			Renderer component = base.GetComponent<Renderer>();
			if (component != null && component.enabled)
			{
				num2 = component.bounds.size.magnitude;
			}
			else
			{
				Collider component2 = base.GetComponent<Collider>();
				if (component2 != null && component2.enabled)
				{
					this.optionalFMODInitialSize = component2.bounds.size.magnitude;
				}
			}
		}
		this.FMODScaleXtoSize = num2 / base.transform.lossyScale.x;
		this.startingPos = base.transform.position;
		this.startingRot = base.transform.rotation;
		this.startingScale = base.transform.localScale;
		if (this.maximumAllowedMass > 0f)
		{
			this.hiddenMassIgnoringMaximum = base.GetComponent<Rigidbody>().mass;
		}
	}

	// Token: 0x06000009 RID: 9 RVA: 0x0001C030 File Offset: 0x0001A230
	public void ReturnOriginal()
	{
		if (!this.grabValues.canReturn && !this.grabValues.willRespawn)
		{
			Debug.Log("Why is this being run. THis object can't be returned!!");
		}
		this.originalStatus.CopyToTransform(base.transform);
		this.originalStatus.CopyToRigidbody(base.GetComponent<Rigidbody>());
		this.toReturn = false;
	}

	// Token: 0x0600000A RID: 10 RVA: 0x0001C08C File Offset: 0x0001A28C
	public void setChildrenColliderLayers(int layer)
	{
		for (int i = 0; i < this.children.Length; i++)
		{
			if (!(this.children[i].transform.root != base.transform.root))
			{
				this.orgChildLayers[i] = this.children[i].layer;
				this.children[i].layer = layer;
			}
		}
	}

	// Token: 0x0600000B RID: 11 RVA: 0x0001C0F4 File Offset: 0x0001A2F4
	public void returnChildrenColliderLayers()
	{
		for (int i = 0; i < this.children.Length; i++)
		{
			if (!(this.children[i].transform.root != base.transform.root))
			{
				this.children[i].layer = this.orgChildLayers[i];
			}
		}
	}

	// Token: 0x0600000C RID: 12 RVA: 0x0001C150 File Offset: 0x0001A350
	public void setSelfAndChildrenMaterials(Material newMat)
	{
		if (this.usingNewMat)
		{
			return;
		}
		this.usingNewMat = true;
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			this.orgSelfAndChildMaterials[this.children.Length] = component.material;
			component.material = newMat;
		}
		for (int i = 0; i < this.children.Length; i++)
		{
			if (!(this.children[i].transform.root != base.transform.root))
			{
				component = this.children[i].GetComponent<MeshRenderer>();
				if (component != null)
				{
					this.orgSelfAndChildMaterials[i] = component.material;
					component.material = newMat;
				}
			}
		}
	}

	// Token: 0x0600000D RID: 13 RVA: 0x0001C1FC File Offset: 0x0001A3FC
	public void returnSelfAndChildrenRenderMaterials()
	{
		if (!this.usingNewMat)
		{
			return;
		}
		this.usingNewMat = false;
		MeshRenderer component = base.GetComponent<MeshRenderer>();
		if (component != null)
		{
			component.material = this.orgSelfAndChildMaterials[this.children.Length];
		}
		for (int i = 0; i < this.children.Length; i++)
		{
			component = this.children[i].GetComponent<MeshRenderer>();
			if (component != null)
			{
				component.material = this.orgSelfAndChildMaterials[i];
			}
		}
	}

	// Token: 0x0600000E RID: 14 RVA: 0x0001C278 File Offset: 0x0001A478
	private void Update()
	{
		base._UpdateAudio();
		if (this.OptionalNeedToGrabFirst != null && !this.optionalObjectHasGrabbed && this.OptionalNeedToGrabFirst.gameObject.layer == LayerMask.NameToLayer("Grabbed"))
		{
			this.optionalObjectHasGrabbed = true;
			base.gameObject.layer = LayerMask.NameToLayer("CanGrab");
		}
		if (this.constantForceRelativeToMass)
		{
			base.GetComponent<ConstantForce>().force = new Vector3(0f, base.GetComponent<Rigidbody>().mass * this.constantForceRelativeMultiplier, 0f);
		}
		if (this.timeToResetToNormalCollisionMode > 0f)
		{
			this.timeToResetToNormalCollisionMode -= Time.deltaTime;
			if (this.timeToResetToNormalCollisionMode <= 0f)
			{
				base.GetComponent<Rigidbody>().collisionDetectionMode = this.originalStatus.collisionDetectionMode;
			}
		}
	}

	// Token: 0x0600000F RID: 15 RVA: 0x00002556 File Offset: 0x00000756
	private void OnCollisionEnter(Collision c)
	{
		base._EvaluateImpact(c, true);
	}

	// Token: 0x06000010 RID: 16 RVA: 0x00002560 File Offset: 0x00000760
	public override float SizeEstimate()
	{
		return base.transform.lossyScale.x * this.FMODScaleXtoSize * 7f;
	}

	// Token: 0x06000011 RID: 17 RVA: 0x0001C350 File Offset: 0x0001A550
	public void SetObjectPhysicsByIsGrabbed(bool isGrabbed)
	{
		Rigidbody component = base.GetComponent<Rigidbody>();
		component.isKinematic = false;
		component.velocity = Vector3.zero;
		component.angularVelocity = Vector3.zero;
		if (!this.grabValues.doesntTurnOnGravity)
		{
			component.useGravity = !isGrabbed;
		}
		if (!isGrabbed)
		{
			if (GameManager.GM.cloneOnNextDrop)
			{
				GameManager.GM.CloneAsGhost(base.gameObject);
			}
			component.isKinematic = this.grabValues.isObjectKinematic;
			component.WakeUp();
		}
		this.isGrabbed = isGrabbed;
		if (!isGrabbed)
		{
			component.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			this.timeToResetToNormalCollisionMode = 2f;
		}
	}

	// Token: 0x06000012 RID: 18 RVA: 0x0000257F File Offset: 0x0000077F
	public Vector3 GetSelfRightingLocalUpVector()
	{
		if (this.optionalSelfRightingLocalUpVector == Vector3.zero)
		{
			return base.transform.up;
		}
		return base.transform.TransformDirection(this.optionalSelfRightingLocalUpVector);
	}

	// Token: 0x06000013 RID: 19 RVA: 0x000025B0 File Offset: 0x000007B0
	public Vector3 GetSelfRightingLocalRightVector()
	{
		return base.transform.right;
	}

	// Token: 0x06000014 RID: 20 RVA: 0x000025BD File Offset: 0x000007BD
	private IEnumerator camera_shake_temp()
	{
		float shakeTime = 0.3f;
		Vector2 shakeMinMaxTime = new Vector2(0.05f, 0.1f);
		float curTimeCounter = 0f;
		float timeTillNextShake = 0f;
		while (curTimeCounter < shakeTime)
		{
			curTimeCounter += timeTillNextShake;
			float d = 0.05f;
			Vector3 b = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0f) * d;
			GameManager.GM.playerCamera.transform.localPosition += b;
			timeTillNextShake = UnityEngine.Random.Range(shakeMinMaxTime.x, shakeMinMaxTime.y);
			yield return new WaitForSeconds(timeTillNextShake);
		}
		yield break;
	}

	// Token: 0x06000015 RID: 21 RVA: 0x000025C5 File Offset: 0x000007C5
	public void TurnOffInstantProject()
	{
		this.grabValues.instantProject = false;
	}

	// Token: 0x06000016 RID: 22 RVA: 0x000025D3 File Offset: 0x000007D3
	public void ChangeSelfRightingLock(bool _theBool)
	{
		this.grabValues.selfRightingLock = _theBool;
	}

	// Token: 0x06000017 RID: 23 RVA: 0x000025E1 File Offset: 0x000007E1
	public void GrayOut()
	{
		base.StartCoroutine(this.SlowlyGrayOut());
	}

	// Token: 0x06000018 RID: 24 RVA: 0x000025F0 File Offset: 0x000007F0
	private IEnumerator SlowlyGrayOut()
	{
		Renderer rend = base.GetComponent<Renderer>();
		if (rend == null || rend.material == null)
		{
			yield break;
		}
		float time = 0.5f;
		float curTime = 0f;
		Color orgCol = rend.material.color;
		Color finalCol = orgCol / 2f;
		while (curTime < time)
		{
			curTime += Time.deltaTime;
			rend.material.color = Color.Lerp(orgCol, finalCol, curTime / time);
			yield return null;
		}
		yield break;
	}

	// Token: 0x06000019 RID: 25 RVA: 0x0001C3EC File Offset: 0x0001A5EC
	public void SpawnGrabParticle()
	{
		if (this.grabParticle != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.grabParticle, base.transform.position, Quaternion.identity);
			gameObject.transform.localScale = base.transform.localScale;
			gameObject.GetComponent<ParticleSystem>().startSize = base.transform.localScale.x * this.grabParticleSizeMultiplier;
		}
	}

	// Token: 0x0600001A RID: 26 RVA: 0x000025FF File Offset: 0x000007FF
	public void TurnOnClone()
	{
		this.grabValues.isCloneable = true;
	}

	// Token: 0x0600001B RID: 27 RVA: 0x0001C45C File Offset: 0x0001A65C
	public void ResetTheObject()
	{
		base.GetComponent<Rigidbody>().velocity = new Vector3(0f, 0f, 0f);
		base.GetComponent<Rigidbody>().mass = this.originalStatus.mass;
		base.transform.position = this.originalStatus.pos;
		base.transform.rotation = this.originalStatus.rot;
		base.transform.localScale = this.originalStatus.scale;
	}

	// Token: 0x04000009 RID: 9
	[NonSerialized]
	public bool isGrabbed;

	// Token: 0x0400000A RID: 10
	public DropTriggerScript.GrabValues grabValues;

	// Token: 0x0400000B RID: 11
	[NonSerialized]
	public bool toReturn;

	// Token: 0x0400000C RID: 12
	public GameObject OptionalNeedToGrabFirst;

	// Token: 0x0400000D RID: 13
	private bool optionalObjectHasGrabbed;

	// Token: 0x0400000E RID: 14
	public bool optionalSpinYAxisOnly = true;

	// Token: 0x0400000F RID: 15
	public bool lightmapHelperIgnore;

	// Token: 0x04000010 RID: 16
	public Vector3 optionalSelfRightingLocalUpVector = Vector3.zero;

	// Token: 0x04000011 RID: 17
	public bool optionalSelfRightingIncludesForward;

	// Token: 0x04000012 RID: 18
	public float optionalAlwaysScaleX = 1f;

	// Token: 0x04000013 RID: 19
	public float optionalMaxScaleX = 5f;

	// Token: 0x04000014 RID: 20
	public bool constantForceRelativeToMass;

	// Token: 0x04000015 RID: 21
	public float constantForceRelativeMultiplier;

	// Token: 0x04000016 RID: 22
	public bool doesRepeatInstantProject;

	// Token: 0x04000017 RID: 23
	public bool hasBeenInstantProjected;

	// Token: 0x04000018 RID: 24
	public bool isClone;

	// Token: 0x04000019 RID: 25
	public bool IResetWhenPlayerHitsBarrier;

	// Token: 0x0400001A RID: 26
	public float optionalFMODInitialSize;

	// Token: 0x0400001B RID: 27
	private float FMODScaleXtoSize;

	// Token: 0x0400001C RID: 28
	[Tooltip("Allow dropping the object with some random torque, so it feels more natural.")]
	public bool AllowRandomDropTorque = true;

	// Token: 0x0400001D RID: 29
	public UnityEvent OnDropped;

	// Token: 0x0400001E RID: 30
	public UnityEvent OnGrabbed;

	// Token: 0x0400001F RID: 31
	public UnityEvent OnGrabbing;

	// Token: 0x04000020 RID: 32
	public float CustomSmallObjectIgnoreOverlapThreshold;

	// Token: 0x04000021 RID: 33
	private const float SIZE_MULT = 7f;

	// Token: 0x04000022 RID: 34
	private const float VEL_MULT = 5f;

	// Token: 0x04000023 RID: 35
	private DropTriggerScript.OriginalStatus originalStatus;

	// Token: 0x04000024 RID: 36
	private GameObject[] children;

	// Token: 0x04000025 RID: 37
	private int[] orgChildLayers;

	// Token: 0x04000026 RID: 38
	private Material[] orgSelfAndChildMaterials;

	// Token: 0x04000027 RID: 39
	private bool usingNewMat;

	// Token: 0x04000028 RID: 40
	public GameObject grabParticle;

	// Token: 0x04000029 RID: 41
	public float grabParticleSizeMultiplier;

	// Token: 0x0400002A RID: 42
	public Vector3 startingPos;

	// Token: 0x0400002B RID: 43
	public Quaternion startingRot;

	// Token: 0x0400002C RID: 44
	public Vector3 startingScale;

	// Token: 0x0400002D RID: 45
	[HideInInspector]
	public DropTriggerScript cloneParent;

	// Token: 0x0400002E RID: 46
	[HideInInspector]
	public DropTriggerScript cloneChild;

	// Token: 0x0400002F RID: 47
	[HideInInspector]
	public bool isCloneRetracting;

	// Token: 0x04000030 RID: 48
	public int timesToClone;

	// Token: 0x04000031 RID: 49
	public float maximumAllowedMass = -1f;

	// Token: 0x04000032 RID: 50
	[HideInInspector]
	public float hiddenMassIgnoringMaximum;

	// Token: 0x04000033 RID: 51
	private const float TimeToResetToNormalCollisionMode = 2f;

	// Token: 0x04000034 RID: 52
	private float timeToResetToNormalCollisionMode;

	// Token: 0x02000004 RID: 4
	[Serializable]
	public class GrabValues
	{
		// Token: 0x04000035 RID: 53
		[Tooltip("It true, object not affected by physics when you let go.")]
		public bool isObjectKinematic;

		// Token: 0x04000036 RID: 54
		public bool canSpin = true;

		// Token: 0x04000037 RID: 55
		[Tooltip("Object keeps itself upright when grabbed.")]
		public bool selfRightingLock;

		// Token: 0x04000038 RID: 56
		[Tooltip("Who knows???")]
		public bool canFreeze;

		// Token: 0x04000039 RID: 57
		[Tooltip("For throwable objects")]
		public bool canFollow;

		// Token: 0x0400003A RID: 58
		[Tooltip("Object clones as soon as you grab")]
		public bool isCloneable;

		// Token: 0x0400003B RID: 59
		[Tooltip("Instantly projects and drops when you grab")]
		public bool instantProject;

		// Token: 0x0400003C RID: 60
		[Tooltip("Grab once, but never again.")]
		public bool isGrabOnce;

		// Token: 0x0400003D RID: 61
		[Tooltip("Object stays same scale in real world.")]
		public bool isAlwaysSameScale;

		// Token: 0x0400003E RID: 62
		[Tooltip("Right click and it goes back to its original place.")]
		public bool canReturn;

		// Token: 0x0400003F RID: 63
		[Tooltip("Who knows???")]
		public bool willRespawn;

		// Token: 0x04000040 RID: 64
		[Tooltip("Object has maximum real world size")]
		public bool hasMaxSize;

		// Token: 0x04000041 RID: 65
		[Tooltip("If this is true, gravity on the object's rigidbody will not turn back on after letting go of the object")]
		public bool doesntTurnOnGravity;

		// Token: 0x04000042 RID: 66
		[Tooltip("This var not fully working yet. Normally, when an object is picked up, it will rotate so that, relative to the player, it looks like it's not rotating. If this is true, it doesn't do that")]
		public bool notRotating;

		// Token: 0x04000043 RID: 67
		[Tooltip("This is for cloning and instant project combined. This var will make it so that it projects multiple times in one click, creating a bridge")]
		public bool instaBridge;

		// Token: 0x04000044 RID: 68
		[Tooltip("this is the number of copies that it creates when you click it")]
		public int clonesInBridge = 20;

		// Token: 0x04000045 RID: 69
		[Tooltip("If you walk through barrier, clones get deleted (not hooked up?)")]
		public bool allowClonesToBeDeleted;

		// Token: 0x04000046 RID: 70
		[Tooltip("Additional scale applied to the drop distance, so that it's not pushed right up against the wall. Useful for large objects.")]
		[Range(0.8f, 1f)]
		public float DistanceScaleModifier = 1f;

		// Token: 0x04000047 RID: 71
		[Tooltip("Minimum size for this object in world bounds. Set to zero to ignore minimum size altogether.")]
		public float MinimumSize = 0.05f;

		// Token: 0x04000048 RID: 72
		[Tooltip("Anchor object to where the player grabbed it. Useful for objects that you're trying to make really large. 0 is regular center, 1 is anchor where grabbed.")]
		[Range(0f, 1f)]
		public float AnchorWhereGrabbed;
	}

	// Token: 0x02000005 RID: 5
	private class OriginalStatus
	{
		// Token: 0x0600001E RID: 30 RVA: 0x0000263A File Offset: 0x0000083A
		public void CopyFromTransform(Transform t)
		{
			this.pos = t.position;
			this.rot = t.rotation;
			this.scale = t.lossyScale;
		}

		// Token: 0x0600001F RID: 31 RVA: 0x00002660 File Offset: 0x00000860
		public void CopyToTransform(Transform t)
		{
			t.position = this.pos;
			t.rotation = this.rot;
			t.localScale = this.scale;
		}

		// Token: 0x06000020 RID: 32 RVA: 0x00002686 File Offset: 0x00000886
		public void CopyFromRigidbody(Rigidbody r)
		{
			this.mass = r.mass;
			this.velocity = r.velocity;
			this.isKinematic = r.isKinematic;
			this.collisionDetectionMode = r.collisionDetectionMode;
		}

		// Token: 0x06000021 RID: 33 RVA: 0x000026B8 File Offset: 0x000008B8
		public void CopyToRigidbody(Rigidbody r)
		{
			r.mass = this.mass;
			r.velocity = this.velocity;
			r.isKinematic = this.isKinematic;
			r.collisionDetectionMode = this.collisionDetectionMode;
		}

		// Token: 0x06000022 RID: 34 RVA: 0x000026EA File Offset: 0x000008EA
		public OriginalStatus()
		{
			this.pos = default(Vector3);
			this.scale = default(Vector3);
			this.rot = default(Quaternion);
			this.velocity = default(Vector3);
		}

		// Token: 0x04000049 RID: 73
		public Vector3 pos;

		// Token: 0x0400004A RID: 74
		public Vector3 scale;

		// Token: 0x0400004B RID: 75
		public Quaternion rot;

		// Token: 0x0400004C RID: 76
		public float mass;

		// Token: 0x0400004D RID: 77
		public Vector3 velocity;

		// Token: 0x0400004E RID: 78
		public bool isKinematic;

		// Token: 0x0400004F RID: 79
		public CollisionDetectionMode collisionDetectionMode;
	}
}
