using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// Token: 0x0200010E RID: 270
public class ResizeScript : MonoBehaviour
{
	// Token: 0x17000033 RID: 51
	// (get) Token: 0x060004B9 RID: 1209 RVA: 0x0000540E File Offset: 0x0000360E
	// (set) Token: 0x060004BA RID: 1210 RVA: 0x00005416 File Offset: 0x00003616
	public bool isGrabbing
	{
		get
		{
			return this._isGrabbing;
		}
		set
		{
			this._isGrabbing = value;
			this.drawOutlinePostProcessing.enabled = value;
			this.outlinerCamera.gameObject.SetActive(value);
		}
	}

	// Token: 0x060004BB RID: 1211 RVA: 0x000305EC File Offset: 0x0002E7EC
	private Quaternion _GetCurrentObjectRotationCameraSpace()
	{
		Quaternion quaternion = this.grabbedObjectRotationInCameraSpaceOriginal;
		DropTriggerScript component = this.grabbedObject.GetComponent<DropTriggerScript>();
		if (component.grabValues.selfRightingLock)
		{
			Vector3 point = Vector3.up;
			if (component.optionalSelfRightingLocalUpVector != Vector3.zero)
			{
				point = component.optionalSelfRightingLocalUpVector;
			}
			Vector3 vector = base.transform.worldToLocalMatrix.rotation * Vector3.up;
			Vector3 fromDirection = this.grabbedObjectRotationInCameraSpaceOriginal * point;
			Quaternion quaternion2 = Quaternion.FromToRotation(Vector3.up, vector) * Quaternion.FromToRotation(fromDirection, Vector3.up);
			if (this.isLerpingToPosition)
			{
				quaternion2 = Quaternion.Slerp(Quaternion.identity, quaternion2, this._GetLerpProgress());
			}
			return Quaternion.AngleAxis(this.accumulatedPlayerRotationCameraSpaceSelfRightingLock.y, vector) * quaternion2 * this.grabbedObjectRotationInCameraSpaceOriginal;
		}
		return this.grabbedObjectRotationInCameraSpaceOriginal;
	}

	// Token: 0x060004BC RID: 1212 RVA: 0x0000543C File Offset: 0x0000363C
	private float _GetMinGrabDistance()
	{
		return GameManager.GM.player.transform.localScale.x * 0.25f;
	}

	// Token: 0x060004BD RID: 1213 RVA: 0x000306CC File Offset: 0x0002E8CC
	private void Start()
	{
		this.fadeMaterialPool = new MaterialPool(new CustomizeMaterial(this.MakeMaterialFade));
		if (this.drawOutlinePostProcessing)
		{
			this.DrawOutlineMat = this.drawOutlinePostProcessing.mat;
		}
		this._layerMaskIgnoreGrabbed = ~(1 << MoSTLayer.IgnoreRaycast | 1 << MoSTLayer.Grabbed | 1 << MoSTLayer.Player);
		this._layerMaskGrabbed = 1 << MoSTLayer.Grabbed;
		this._layerMaskIgnorePlayer = ~(1 << MoSTLayer.Player | 1 << MoSTLayer.IgnoreRaycast);
		this._layerMaskOnlyPlayer = 1 << MoSTLayer.Player;
		MouseLook[] componentsInChildren = base.transform.root.GetComponentsInChildren<MouseLook>();
		if (componentsInChildren.Length >= 1)
		{
			this.mouseLook1 = componentsInChildren[0];
		}
		if (componentsInChildren.Length >= 2)
		{
			this.mouseLook2 = componentsInChildren[1];
		}
		this.repeatInstantProjectCount = this.repeatInstantProjectMax;
	}

	// Token: 0x060004BE RID: 1214 RVA: 0x000307AC File Offset: 0x0002E9AC
	private void Update()
	{
		if (!GameManager.GM.PM.canControl)
		{
			return;
		}
		if (GameObject.Find("GrabText") == null && GameObject.Find("UI_PAUSE_MENU") != null)
		{
			this.grabText = new GameObject("GrabText")
			{
				transform = 
				{
					parent = GameObject.Find("UI_PAUSE_MENU").transform.Find("Canvas")
				}
			}.AddComponent<Text>();
			RectTransform component = this.grabText.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2((float)(Screen.currentResolution.width / 3), (float)(Screen.currentResolution.height / 3));
			component.pivot = new Vector2(0f, 1f);
			component.anchorMin = new Vector2(0f, 0.6f);
			component.anchorMax = new Vector2(0f, 0.6f);
			component.anchoredPosition = new Vector2(25f, -25f);
			foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
			{
				if (font.name == "BebasNeue Bold")
				{
					this.grabText.font = font;
				}
			}
			this.grabText.text = "hello world";
			this.grabText.fontSize = 30;
		}
		if (this.isGrabbing)
		{
			Text text = this.grabText;
			object[] array2 = new object[16];
			array2[0] = this.grabbedObject.name;
			array2[1] = "\nPosition: ";
			int num = 2;
			Vector3 vector = this.grabbedObject.transform.localPosition;
			array2[num] = vector.x.ToString("0.000");
			array2[3] = ", ";
			int num2 = 4;
			vector = this.grabbedObject.transform.localPosition;
			array2[num2] = vector.y.ToString("0.000");
			array2[5] = ", ";
			int num3 = 6;
			vector = this.grabbedObject.transform.localPosition;
			array2[num3] = vector.z.ToString("0.000");
			array2[7] = "\nRotation: ";
			int num4 = 8;
			vector = this.grabbedObject.transform.localRotation.eulerAngles;
			array2[num4] = vector.x.ToString("0.000");
			array2[9] = ", ";
			int num5 = 10;
			vector = this.grabbedObject.transform.localRotation.eulerAngles;
			array2[num5] = vector.y.ToString("0.000");
			array2[11] = ", ";
			int num6 = 12;
			vector = this.grabbedObject.transform.localRotation.eulerAngles;
			array2[num6] = vector.z.ToString("0.000");
			array2[13] = "\nScale: ";
			int num7 = 14;
			vector = this.grabbedObject.transform.lossyScale;
			array2[num7] = vector.x.ToString("0.0000");
			array2[15] = "x";
			text.text = string.Concat(array2);
		}
		else
		{
			this.grabText.text = "";
		}
		this._UpdateGrabSizeStuffWhenGrabbing();
		this.isReadyToGrab = false;
		this.isObjectBlocked = false;
		this.isRejectProject = false;
		this.isObjectReadyToReturn = false;
		if (this.mouseLook1)
		{
			this.mouseLook1.skipUpdate = false;
		}
		if (this.mouseLook2)
		{
			this.mouseLook2.skipUpdate = false;
		}
		bool flag = false;
		Portal crossedPortal = null;
		if (!this.isGrabbing)
		{
			this.grabbedObject = null;
			this.isReadyToGrab = false;
			GameObject firstGrabbableObject = this.getFirstGrabbableObject(out crossedPortal, out flag);
			if (firstGrabbableObject != null)
			{
				DropTriggerScript component2 = firstGrabbableObject.GetComponent<DropTriggerScript>();
				if (component2)
				{
					this.isPointingAtInstantProjectObj = component2.grabValues.instantProject;
					this.grabbedObject = firstGrabbableObject;
					this.isReadyToGrab = true;
					if (this.grabbedObject.GetComponent<DropTriggerScript>().toReturn)
					{
						this.isObjectReadyToReturn = true;
					}
				}
				else
				{
					Debug.LogError(string.Format("CanGrab object ({0}) doesn't have DropTriggerScript.", firstGrabbableObject.name), firstGrabbableObject);
				}
			}
		}
		if (this.notAllowedToGrab && this.grabbedObject)
		{
			if (this.isGrabbing)
			{
				this.DoDropDownObject();
			}
			this.grabbedObject = null;
			this.isReadyToGrab = false;
		}
		if (!this.grabbedObject)
		{
			if (GameManager.GM.playerInput.GetButtonDown("Grab") && !this.isGrabbing && this.soundEffectScript && !flag)
			{
				this.soundEffectScript.play_denied_sound();
			}
			return;
		}
		if (this.isLerpingToPosition)
		{
			if (this.grabbedObject == null || !this.isGrabbing)
			{
				this.isLerpingToPosition = false;
			}
			else if (this.curLerpingTime < 0.8f)
			{
				this.toResizeWithoutDropping = true;
				this.curLerpingTime += Time.deltaTime;
			}
			else
			{
				this.isLerpingToPosition = false;
				this.toResizeWithoutDropping = true;
			}
		}
		if ((GameManager.GM.playerInput.GetButtonDown("Grab") && !this.isGrabbing) || (this.grabbedObject.GetComponent<DropTriggerScript>().doesRepeatInstantProject && this.repeatInstantProjectCount < this.repeatInstantProjectMax))
		{
			DropTriggerScript component3 = this.grabbedObject.GetComponent<DropTriggerScript>();
			if (component3.toReturn)
			{
				if (!GameManager.GM.TM.isFmodTest)
				{
					this.soundEffectScript.play_return_sound();
				}
				else
				{
					this.soundEffectScript.play_fmod_return_sound(this.grabbedObject.transform);
				}
				component3.ReturnOriginal();
			}
			else
			{
				if (this.grabbedObject.GetComponent<ProjectThroughThisObjWhenCloning>() != null)
				{
					this.grabbedObject.GetComponent<BoxCollider>().enabled = false;
				}
				if (!this.dontLerpObjectsToCenter)
				{
					this.isLerpingToPosition = true;
					this.curLerpingTime = 0f;
				}
				this.DoGrabObject(crossedPortal);
				if (component3.grabValues.instantProject)
				{
					this.DoDropDownObject();
					if (component3.grabValues.instaBridge)
					{
						this.CreateInstaBridge(crossedPortal);
					}
					component3.hasBeenInstantProjected = true;
					if (this.repeatInstantProjectCount < this.repeatInstantProjectMax)
					{
						this.repeatInstantProjectCount++;
					}
				}
				else if (this.grabbedObject != null)
				{
					this.DoDynamicProjectObject();
				}
				if (this.grabbedObject.GetComponent<ProjectThroughThisObjWhenCloning>() != null)
				{
					this.grabbedObject.GetComponent<BoxCollider>().enabled = true;
					component3.grabValues.instantProject = false;
					component3.grabValues.isCloneable = false;
					this.oldObj.GetComponent<BoxCollider>().enabled = true;
					UnityEngine.Object.Destroy(this.grabbedObject.GetComponent<ProjectThroughThisObjWhenCloning>());
				}
				if (this.SpinInstruction != null && component3.grabValues.canSpin)
				{
					this.spinGUIStartinPoint = this.GetBottomRightBoundingInScreenSpace(this.grabbedObject);
				}
				if (this.DropInstruction)
				{
					this.dropGUIStartinPoint = this.GetBottomRightBoundingInScreenSpace(this.grabbedObject);
				}
			}
		}
		else if (GameManager.GM.playerInput.GetButtonDown("Grab") && this.isGrabbing)
		{
			this.DoDropDownObject();
		}
		else if (GameManager.GM.playerInput.GetButtonDown("Grab") && this.isGrabbing && this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.canFollow)
		{
			this.DoDropDownObject();
		}
		else if (this.isGrabbing && this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.canSpin)
		{
			if (GameManager.GM.playerInput.GetButton("Rotate"))
			{
				this.mouseLook1.skipUpdate = true;
				this.mouseLook2.skipUpdate = true;
				this.RotateObjectByLocalAxis();
			}
			if (GameManager.GM.playerInput.GetButtonDown("Rotate") && this.doRotateTutInThisLevel)
			{
				this.StartHoldRotateTimer();
			}
			this.toResizeWithoutDropping = true;
		}
		else if (this.isGrabbing && this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.canFreeze)
		{
			if (GameManager.GM.playerInput.GetButton("Rotate"))
			{
				this.grabbedObject.layer = MoSTLayer.Default;
				this.grabbedObject.GetComponent<DropTriggerScript>().returnChildrenColliderLayers();
				this.SetRotationAsCurrent(this.grabbedObject.transform.rotation);
				this.toResizeWithoutDropping = false;
			}
			else
			{
				if (this.grabbedObject.layer != MoSTLayer.Grabbed)
				{
					this.grabbedObject.layer = MoSTLayer.Grabbed;
					this.grabbedObject.GetComponent<DropTriggerScript>().setChildrenColliderLayers(MoSTLayer.Grabbed);
				}
				this.toResizeWithoutDropping = true;
			}
		}
		else if (this.isGrabbing)
		{
			this.toResizeWithoutDropping = true;
		}
		else if (!this.isGrabbing && this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.isCloneable && GameManager.GM.playerInput.GetButtonDown("Rotate"))
		{
			DropTriggerScript component4 = this.grabbedObject.GetComponent<DropTriggerScript>();
			if (!(component4.cloneChild == null) || !(component4.cloneParent == null))
			{
				this.soundEffectScript.play_clone_retrieve_sound();
				this.RetractClone(component4);
			}
		}
		if (this.holdRotateTutorialTimer > 0f)
		{
			this.holdRotateTutorialTimer -= Time.deltaTime;
			return;
		}
		if (this.holdRotateTutorialTimer <= 0f && this.holdRotateTutorialTimer > -1000f)
		{
			if (this.rotateTut != null)
			{
				this.rotateTut.enabled = false;
			}
			this.holdRotateTutorialTimer = -1001f;
		}
	}

	// Token: 0x060004BF RID: 1215 RVA: 0x0000545D File Offset: 0x0000365D
	private void FixedUpdate()
	{
		this.notAllowedToGrab = false;
		if (this.notAllowedToGrabNextFrame)
		{
			this.notAllowedToGrab = true;
		}
		this.notAllowedToGrabNextFrame = false;
	}

	// Token: 0x060004C0 RID: 1216 RVA: 0x00031144 File Offset: 0x0002F344
	private void CreateInstaBridge(Portal crossedPortal)
	{
		for (int i = 0; i < this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.clonesInBridge - 1; i++)
		{
			this.DoGrabObject(crossedPortal);
			this.DoDropDownObject();
		}
	}

	// Token: 0x060004C1 RID: 1217 RVA: 0x00031180 File Offset: 0x0002F380
	private GrabResult ResizeWithoutDropping()
	{
		if (this.isEnableDebug)
		{
			GameManager.GM.MyLog("Grab Distance:" + this._GetMinGrabDistance() + ",");
		}
		this.ResetObjectToMinDistance(this.grabbedObject, 1f);
		DropTriggerScript component = this.grabbedObject.GetComponent<DropTriggerScript>();
		if (component.grabValues.isAlwaysSameScale)
		{
			float num = component.optionalAlwaysScaleX / this.grabbedObject.transform.lossyScale.x;
			num = Mathf.Lerp(1f, num, 0.1f);
			this.grabbedObject.transform.localScale *= num;
			float mass = Mathf.Pow(num, 3f) * this.grabbedObject.GetComponent<Rigidbody>().mass * 1f;
			this.grabbedObject.GetComponent<Rigidbody>().mass = mass;
		}
		GrabResult grabResult = this.DoDynamicProjectObject();
		if (component.grabValues.hasMaxSize)
		{
			float x = this.grabbedObject.transform.lossyScale.x;
			float optionalMaxScaleX = component.optionalMaxScaleX;
			if (x > optionalMaxScaleX)
			{
				this.grabbedObject.transform.localScale = this.grabbedObject.transform.localScale * (optionalMaxScaleX / x);
			}
		}
		if (this.isRejectProject)
		{
			this.isObjectBlocked = true;
			component.setSelfAndChildrenMaterials(this.blockedMat);
		}
		else
		{
			component.returnSelfAndChildrenRenderMaterials();
		}
		this.lastCrossedPortal = grabResult.CrossedPortal;
		if (component.OnGrabbing != null)
		{
			component.OnGrabbing.Invoke();
		}
		if (this.immediatelyDropObjects)
		{
			this.DoDropDownObject();
		}
		return grabResult;
	}

	// Token: 0x060004C2 RID: 1218 RVA: 0x0000547C File Offset: 0x0000367C
	public bool ProjectedThroughPortal()
	{
		return this.lastCrossedPortal != null;
	}

	// Token: 0x060004C3 RID: 1219 RVA: 0x00031318 File Offset: 0x0002F518
	private GrabResult DoDynamicProjectObject()
	{
		Portal crossedPortal;
		float reductionRatio;
		float num = this.getResizeDistance(this.grabbedObject, out crossedPortal, out reductionRatio);
		if (num == 3.40282347E+38f)
		{
			num = this._maxGrabDistance;
		}
		if (num == 3.40282347E+38f)
		{
			if (this.isEnableDebug)
			{
				GameManager.GM.MyLog("Edge case!");
			}
		}
		else if (num > 0f)
		{
			this._EnsureObjectDetached();
			this.UpdateObjectTransformAndPropertyByDistance(this.grabbedObject, num, crossedPortal, reductionRatio);
			this.DrawIndicatorIfObjectTooSmall(reductionRatio);
			if (this.isEnableDebug)
			{
				GameManager.GM.MyLog("resize dist: " + num);
			}
		}
		else
		{
			GameManager.GM.MyLog("Return value of resize ratio is <= 0");
		}
		return new GrabResult
		{
			CrossedPortal = crossedPortal
		};
	}

	// Token: 0x060004C4 RID: 1220 RVA: 0x0000548A File Offset: 0x0000368A
	private void OnDestroy()
	{
		if (this.fadeMaterialPool != null)
		{
			this.fadeMaterialPool.Destroy();
			this.fadeMaterialPool = null;
		}
	}

	// Token: 0x060004C5 RID: 1221 RVA: 0x000313D0 File Offset: 0x0002F5D0
	private void MakeMaterialFade(Material material)
	{
		material.SetInt("_SrcBlend", 5);
		material.SetInt("_DstBlend", 10);
		material.SetInt("_ZWrite", 0);
		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = 3000;
		material.SetOverrideTag("RenderType", "Fade");
		Color color = material.color;
		color.a *= 0.5f;
		material.color = color;
	}

	// Token: 0x060004C6 RID: 1222 RVA: 0x0003145C File Offset: 0x0002F65C
	private void DrawIndicatorIfObjectTooSmall(float reductionRatio)
	{
		bool flag = false;
		float num = 1f;
		float grabbedObjectTooSmallAmount = this.GetGrabbedObjectTooSmallAmount();
		if (grabbedObjectTooSmallAmount < 1f)
		{
			num = 1f / grabbedObjectTooSmallAmount;
			flag = true;
		}
		if (reductionRatio < 1f)
		{
			num = 1f / reductionRatio;
			flag = true;
		}
		if (flag && this.fadeMaterialPool != null)
		{
			FakeObjectModifiers fakeObjectModifiers = new FakeObjectModifiers
			{
				ShadowCastingMode = new ShadowCastingMode?(ShadowCastingMode.Off)
			};
			this.fadeMaterialPool.FrameReset();
			DrawFakeObjectHelper.DrawGameObject(this.grabbedObject, GameManager.GM.playerCamera, new Vector3(num, num, num), this.fadeMaterialPool, ref fakeObjectModifiers);
		}
	}

	// Token: 0x060004C7 RID: 1223 RVA: 0x000314F0 File Offset: 0x0002F6F0
	private void EnlargenIfTooSmall()
	{
		float grabbedObjectTooSmallAmount = this.GetGrabbedObjectTooSmallAmount();
		if (grabbedObjectTooSmallAmount < 1f)
		{
			float d = 1f / grabbedObjectTooSmallAmount;
			this.grabbedObject.transform.localScale *= d;
		}
	}

	// Token: 0x060004C8 RID: 1224 RVA: 0x00031530 File Offset: 0x0002F730
	private float GetGrabbedObjectTooSmallAmount()
	{
		float minimumSize = this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.MinimumSize;
		Vector3 vector = Vector3.Scale(this.grabbedObject.GetComponent<MeshFilter>().sharedMesh.bounds.size, this.grabbedObject.transform.lossyScale);
		return Mathf.Max(0.0001f, Mathf.Max(vector.x / minimumSize, Mathf.Max(vector.y / minimumSize, vector.z / minimumSize)));
	}

	// Token: 0x060004C9 RID: 1225 RVA: 0x000315B4 File Offset: 0x0002F7B4
	private void SetRotationAsCurrent(Quaternion grabbedObjectRotation)
	{
		this.grabbedObjectRotationInCameraSpaceOriginal = base.transform.worldToLocalMatrix.rotation * grabbedObjectRotation;
		this.accumulatedPlayerRotationCameraSpaceSelfRightingLock = Vector3.zero;
	}

	// Token: 0x060004CA RID: 1226 RVA: 0x000315EC File Offset: 0x0002F7EC
	private void SetPositionAsOriginal(Vector3 worldPosition, Portal crossedPortal, DropTriggerScript dts)
	{
		if (crossedPortal && crossedPortal.Destination)
		{
			worldPosition = PortalHelper.TransformPointRelativeToPortal(crossedPortal.Destination, crossedPortal, worldPosition);
		}
		this.positionInCameraSpaceOriginal = base.transform.InverseTransformPoint(worldPosition);
		Vector3 b = this.positionInCameraSpaceOriginal;
		b.x = 0f;
		this.positionInCameraSpaceOriginalFinal = Vector3.Slerp(Vector3.forward, b, dts.grabValues.AnchorWhereGrabbed);
	}

	// Token: 0x060004CB RID: 1227 RVA: 0x00031660 File Offset: 0x0002F860
	private void SyncGrabbedObjectRotation()
	{
		this.grabbedObject.transform.rotation = base.transform.localToWorldMatrix.rotation * this._GetCurrentObjectRotationCameraSpace();
	}

	// Token: 0x060004CC RID: 1228 RVA: 0x0003169C File Offset: 0x0002F89C
	private void LateUpdate()
	{
		GrabResult grabResult = default(GrabResult);
		this._UpdateGrabSizeStuffWhenGrabbing();
		if (this.isGrabbing)
		{
			this.prevPosition = this.grabbedObject.transform.position;
			if (this.toResizeWithoutDropping)
			{
				grabResult = this.ResizeWithoutDropping();
			}
		}
		this.toResizeWithoutDropping = false;
		if (this.outlinerCamera)
		{
			if (this.grabbedObject && this.isGrabbing && grabResult.CrossedPortal)
			{
				PortalHelper.PositionObjectRelativeToPortal(grabResult.CrossedPortal, GameManager.GM.playerCamera.transform, this.outlinerCamera.transform);
				return;
			}
			this.outlinerCamera.transform.localPosition = Vector3.zero;
			this.outlinerCamera.transform.localRotation = Quaternion.identity;
			this.outlinerCamera.transform.localScale = Vector3.one;
		}
	}

	// Token: 0x060004CD RID: 1229 RVA: 0x00031784 File Offset: 0x0002F984
	private void OnGUI()
	{
		if (this.SpinInstruction != null && this.isGrabbing && this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.canSpin)
		{
			Vector2 vector = new Vector2((float)Screen.width * 0.07f, (float)Screen.width * 0.07f);
			GUI.DrawTexture(new Rect(this.spinGUIStartinPoint.x, this.spinGUIStartinPoint.y, vector.x, vector.y), this.SpinInstruction);
		}
	}

	// Token: 0x060004CE RID: 1230 RVA: 0x00031810 File Offset: 0x0002FA10
	private Vector2 GetBottomRightBoundingInScreenSpace(GameObject targetObject)
	{
		Vector3 center = targetObject.GetComponent<Collider>().bounds.center;
		float magnitude = targetObject.GetComponent<Collider>().bounds.extents.magnitude;
		Vector3 vector = base.transform.right.normalized * magnitude;
		if (this.isEnableDebug)
		{
			Debug.DrawLine(center, center + vector, Color.cyan);
		}
		Vector3 position = center + vector * 0.5f;
		return base.GetComponent<Camera>().WorldToScreenPoint(position);
	}

	// Token: 0x060004CF RID: 1231 RVA: 0x000318A8 File Offset: 0x0002FAA8
	private void RotateObjectByLocalAxis()
	{
		float axis = GameManager.GM.playerInput.GetAxis("Look Vertical");
		float axis2 = GameManager.GM.playerInput.GetAxis("Look Horizontal");
		Vector3 vector;
		if (this.grabbedObject.GetComponent<DropTriggerScript>().optionalSpinYAxisOnly)
		{
			vector = new Vector3(0f, -axis2, 0f);
			vector *= Time.deltaTime * 150f;
		}
		else
		{
			if (Mathf.Abs(axis) > Mathf.Abs(axis2) * 3f)
			{
				vector = new Vector3(axis, 0f, 0f);
			}
			else if (Mathf.Abs(axis) * 3f < Mathf.Abs(axis2))
			{
				vector = new Vector3(0f, -axis2, 0f);
			}
			else
			{
				vector = new Vector3(axis, -axis2, 0f) * 1f;
			}
			vector = base.transform.TransformDirection(vector);
			vector *= Time.deltaTime * 100f;
		}
		this.accumulatedPlayerRotationCameraSpaceSelfRightingLock += vector;
		if (!this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.selfRightingLock)
		{
			Vector3 axis3 = base.transform.worldToLocalMatrix.rotation * Vector3.up;
			this.grabbedObjectRotationInCameraSpaceOriginal = Quaternion.AngleAxis(vector.y, axis3) * this.grabbedObjectRotationInCameraSpaceOriginal;
		}
	}

	// Token: 0x060004D0 RID: 1232 RVA: 0x00031A08 File Offset: 0x0002FC08
	public void RetractClone(DropTriggerScript dts)
	{
		if (dts.isCloneRetracting)
		{
			return;
		}
		if (dts.cloneParent != null)
		{
			base.StartCoroutine(this.LerpCloneToParent(dts));
			dts.isCloneRetracting = true;
		}
		if (dts.cloneChild != null)
		{
			this.RetractClone(dts.cloneChild);
		}
		if (dts.cloneParent != null)
		{
			this.RetractClone(dts.cloneParent);
		}
	}

	// Token: 0x060004D1 RID: 1233 RVA: 0x000054A6 File Offset: 0x000036A6
	private IEnumerator LerpCloneToParent(DropTriggerScript dts)
	{
		Collider[] componentsInChildren = dts.GetComponentsInChildren<Collider>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = false;
		}
		dts.GetComponent<Rigidbody>().isKinematic = true;
		Transform parentTransform = dts.cloneParent.transform;
		Transform myTransform = dts.transform;
		float maxLerpTime = 0.7f;
		float currentLerpTime = 0f;
		Vector3 initialPosition = myTransform.position;
		Vector3 initialScale = myTransform.localScale;
		Quaternion initialRotation = myTransform.rotation;
		while (currentLerpTime < maxLerpTime)
		{
			currentLerpTime += Time.deltaTime;
			float t = Mathf.Pow(currentLerpTime / maxLerpTime, 1.3f);
			myTransform.position = Vector3.Lerp(initialPosition, parentTransform.position, t);
			myTransform.localScale = Vector3.Lerp(initialScale, parentTransform.localScale, t);
			myTransform.rotation = Quaternion.Lerp(initialRotation, parentTransform.rotation, t);
			yield return new WaitForEndOfFrame();
		}
		dts.gameObject.SetActive(false);
		yield return new WaitForSeconds(1f);
		UnityEngine.Object.Destroy(dts.gameObject);
		yield return null;
		yield break;
	}

	// Token: 0x060004D2 RID: 1234 RVA: 0x000054B5 File Offset: 0x000036B5
	private void _EnsureObjectDetached()
	{
		if (this.grabbedObject.transform.root != this.grabbedObject.transform)
		{
			this.grabbedObject.transform.parent = null;
		}
	}

	// Token: 0x060004D3 RID: 1235 RVA: 0x00031A78 File Offset: 0x0002FC78
	private void DoGrabObject(Portal crossedPortal)
	{
		if (this.isReadyToGrab)
		{
			DropTriggerScript component = this.grabbedObject.GetComponent<DropTriggerScript>();
			if (component.grabValues.isCloneable)
			{
				DropTriggerScript dropTriggerScript = component;
				if (this.grabbedObject.GetComponent<ProjectThroughThisObjWhenCloning>() == null)
				{
					this.grabbedObject = UnityEngine.Object.Instantiate<GameObject>(this.grabbedObject, this.grabbedObject.transform.position, this.grabbedObject.transform.rotation);
					component = this.grabbedObject.GetComponent<DropTriggerScript>();
					component.isClone = true;
				}
				else
				{
					GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.grabbedObject, this.grabbedObject.transform.position, this.grabbedObject.transform.rotation);
					gameObject.GetComponent<BoxCollider>().enabled = true;
					this.oldObj = this.grabbedObject;
					this.grabbedObject = gameObject;
					component = this.grabbedObject.GetComponent<DropTriggerScript>();
				}
				dropTriggerScript.cloneChild = component;
				component.cloneParent = dropTriggerScript;
			}
			this.grabbedObject.layer = MoSTLayer.Grabbed;
			component.setChildrenColliderLayers(MoSTLayer.Grabbed);
			Vector3 vector = this.grabbedObject.transform.position;
			this.SetPositionAsOriginal(vector, crossedPortal, component);
			if (crossedPortal && crossedPortal.Destination)
			{
				vector = PortalHelper.TransformPointRelativeToPortal(crossedPortal.Destination, crossedPortal, vector);
			}
			Vector3 vector2 = vector - base.transform.position;
			float initialRatio = this._GetMinGrabDistance() / vector2.magnitude;
			if (this.soundEffectScript)
			{
				this.soundEffectScript.play_pop_sound_grab(component.SizeEstimate());
			}
			if (!this.isGrabbing)
			{
				component.SpawnGrabParticle();
			}
			this._EnsureObjectDetached();
			this.SetScaleAtMinDistance(this.grabbedObject, initialRatio, crossedPortal);
			Quaternion quaternion = this.grabbedObject.transform.rotation;
			if (crossedPortal && crossedPortal.Destination)
			{
				quaternion = PortalHelper.TransformRotation(crossedPortal.Destination, quaternion);
			}
			this.SetRotationAsCurrent(quaternion);
			component.SetObjectPhysicsByIsGrabbed(true);
			this.isGrabbing = true;
			this.grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
			if (this.DrawOutlineMat)
			{
				if (component.grabValues.canSpin)
				{
					this.DrawOutlineMat.color = this.canSpinColor;
				}
				else if (component.grabValues.canFollow)
				{
					this.DrawOutlineMat.color = this.canFollowColor;
				}
				else
				{
					this.DrawOutlineMat.color = this.cannotDoAnythingColor;
				}
			}
			this.outlinerCamera.LerpOutlineDist();
			if (component.OnGrabbed != null)
			{
				component.OnGrabbed.Invoke();
			}
		}
	}

	// Token: 0x060004D4 RID: 1236 RVA: 0x000054EA File Offset: 0x000036EA
	public void CheckIfGrabbingThenDoDropDownObject()
	{
		if (this.isGrabbing)
		{
			this.DoDropDownObject();
		}
	}

	// Token: 0x060004D5 RID: 1237 RVA: 0x00031D04 File Offset: 0x0002FF04
	private void DoDropDownObject()
	{
		Portal crossedPortal;
		float reductionRatio;
		float num = this.getResizeDistance(this.grabbedObject, out crossedPortal, out reductionRatio);
		if (num == 3.40282347E+38f)
		{
			num = this._maxGrabDistance;
			this.isObjectInSky = true;
		}
		else
		{
			this.isObjectInSky = false;
		}
		if (num == 3.40282347E+38f || this.isRejectProject)
		{
			if (this.soundEffectScript)
			{
				this.soundEffectScript.play_denied_sound();
				return;
			}
		}
		else
		{
			Vector3 position = this.grabbedObject.transform.position;
			this._EnsureObjectDetached();
			this.UpdateObjectTransformAndPropertyByDistance(this.grabbedObject, num, crossedPortal, reductionRatio);
			this.EnlargenIfTooSmall();
			this.ReturnObjectToWorld();
			if (this.grabbedObject.GetComponent<DropTriggerScript>().grabValues.canFollow)
			{
				this.setFollowVelocity(position);
			}
			Vector3.Distance(base.transform.position, this.grabbedObject.transform.position);
			if (this.soundEffectScript)
			{
				this.soundEffectScript.play_pop_sound_put(this.grabbedObject.GetComponent<DropTriggerScript>().SizeEstimate());
			}
		}
	}

	// Token: 0x060004D6 RID: 1238 RVA: 0x000054FA File Offset: 0x000036FA
	public void CallReturnObjectToWorld()
	{
		this.ReturnObjectToWorld();
	}

	// Token: 0x060004D7 RID: 1239 RVA: 0x00031E08 File Offset: 0x00030008
	private void ReturnObjectToWorld()
	{
		this.grabbedObject.layer = MoSTLayer.CanGrab;
		DropTriggerScript component = this.grabbedObject.GetComponent<DropTriggerScript>();
		if (component.grabValues.isGrabOnce)
		{
			this.grabbedObject.layer = MoSTLayer.Default;
			component.GrayOut();
		}
		if (component.grabValues.canReturn)
		{
			component.toReturn = true;
		}
		component.returnChildrenColliderLayers();
		component.SetObjectPhysicsByIsGrabbed(false);
		if (this.isObjectInSky)
		{
			this.grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
		}
		else if (component.AllowRandomDropTorque)
		{
			Rigidbody component2 = this.grabbedObject.GetComponent<Rigidbody>();
			if (component2)
			{
				Vector3 a = this.LimitRandomTorqueToCameraPlane ? base.transform.forward : UnityEngine.Random.onUnitSphere;
				if (UnityEngine.Random.Range(0, 2) == 0)
				{
					a = -a;
				}
				component2.AddTorque(a * UnityEngine.Random.Range(this.RandomTorqueOnDrop.x, this.RandomTorqueOnDrop.y), ForceMode.VelocityChange);
			}
		}
		this.isGrabbing = false;
		if (component.OnDropped != null)
		{
			component.OnDropped.Invoke();
		}
	}

	// Token: 0x060004D8 RID: 1240 RVA: 0x00031F1C File Offset: 0x0003011C
	private static bool _CastRayOrSphereForObject(Ray ray, float sphereRadius, out RaycastHit hitInfo, int layerMask, out GameObject hitObject, out GameObject grabObject, out bool wasSingleRay)
	{
		hitObject = null;
		grabObject = null;
		wasSingleRay = true;
		bool flag = false;
		if (Physics.Raycast(ray, out hitInfo, float.PositiveInfinity, layerMask))
		{
			flag = true;
			grabObject = ResizeScript.getNearestCanGrabAncestor(hitInfo.collider.gameObject);
			if (grabObject)
			{
				hitObject = hitInfo.collider.gameObject;
				flag = true;
			}
		}
		if (flag)
		{
			int num = Physics.SphereCastNonAlloc(ray, sphereRadius, ResizeScript.hitWorker, hitInfo.distance + 0.1f, layerMask);
			for (int i = 0; i < num; i++)
			{
				GameObject nearestCanGrabAncestor = ResizeScript.getNearestCanGrabAncestor(ResizeScript.hitWorker[i].collider.gameObject);
				if (nearestCanGrabAncestor)
				{
					Vector3 vector = ResizeScript.hitWorker[i].point - ray.origin;
					RaycastHit raycastHit;
					if (!Physics.Raycast(new Ray(ray.origin, vector.normalized), out raycastHit, vector.magnitude, layerMask) || raycastHit.collider.gameObject == ResizeScript.hitWorker[i].collider.gameObject)
					{
						hitObject = ResizeScript.hitWorker[i].collider.gameObject;
						grabObject = nearestCanGrabAncestor;
						wasSingleRay = false;
						break;
					}
				}
			}
			if (!hitObject)
			{
				hitObject = hitInfo.collider.gameObject;
			}
		}
		return hitObject != null;
	}

	// Token: 0x060004D9 RID: 1241 RVA: 0x0003207C File Offset: 0x0003027C
	private GameObject getFirstGrabbableObject(out Portal crossedPortal, out bool supressDeniedSound)
	{
		supressDeniedSound = false;
		crossedPortal = null;
		RaycastHit raycastHit;
		GameObject gameObject;
		GameObject result;
		bool flag;
		if (ResizeScript._CastRayOrSphereForObject(new Ray(base.transform.position, base.transform.forward), this.CastRadius, out raycastHit, this._layerMaskIgnorePlayer, out gameObject, out result, out flag))
		{
			Portal portal = this._MaybeGetHitPortal(ref raycastHit);
			GameObject gameObject2;
			GameObject gameObject3;
			if (portal && flag && ResizeScript._CastRayOrSphereForObject(PortalHelper.TransformRay(portal, new Ray(raycastHit.point, base.transform.forward)), this.CastRadius, out raycastHit, -5, out gameObject2, out gameObject3, out flag))
			{
				gameObject = gameObject2;
				crossedPortal = portal;
				if (gameObject3 && !portal.transform.IsChildOf(gameObject3.transform) && !portal.Destination.transform.IsChildOf(gameObject3.transform))
				{
					result = gameObject3;
				}
			}
		}
		MOSTTriggerBase mosttriggerBase = null;
		if (gameObject)
		{
			mosttriggerBase = gameObject.GetComponent<MOSTTriggerOnClick>();
			if (!mosttriggerBase)
			{
				mosttriggerBase = gameObject.GetComponentInParent<MOSTTriggerOnClickChildren>();
			}
		}
		supressDeniedSound = (mosttriggerBase && (!mosttriggerBase.hasTriggered || mosttriggerBase.repeatableTrigger));
		return result;
	}

	// Token: 0x060004DA RID: 1242 RVA: 0x0003219C File Offset: 0x0003039C
	public static GameObject getNearestCanGrabAncestor(GameObject obj)
	{
		if (obj.layer == MoSTLayer.CanGrab)
		{
			return obj;
		}
		if (obj.transform.parent == null)
		{
			return null;
		}
		if (obj.layer == MoSTLayer.CannotGrab)
		{
			return null;
		}
		if (obj.CompareTag(ResizeScript.ChildThatIgnoresGrabTag))
		{
			return null;
		}
		return ResizeScript.getNearestCanGrabAncestor(obj.transform.parent.gameObject);
	}

	// Token: 0x060004DB RID: 1243 RVA: 0x00032200 File Offset: 0x00030400
	private float _EstimateObjectSize(GameObject obj)
	{
		Vector3 size = this._EstimateObjectBounds(obj).size;
		return Mathf.Max(size.x, Mathf.Max(size.y, size.z));
	}

	// Token: 0x060004DC RID: 1244 RVA: 0x0003223C File Offset: 0x0003043C
	private Bounds _EstimateObjectBounds(GameObject obj)
	{
		Transform transform = obj.transform;
		Bounds result = new Bounds(transform.position, Vector3.zero);
		Collider component = obj.GetComponent<Collider>();
		if (component)
		{
			result.Encapsulate(component.bounds);
		}
		int childCount = transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = transform.GetChild(i);
			if (child.gameObject.activeSelf)
			{
				result.Encapsulate(this._EstimateObjectBounds(child.gameObject));
			}
		}
		return result;
	}

	// Token: 0x060004DD RID: 1245 RVA: 0x000322C4 File Offset: 0x000304C4
	private float _GetSmallObjectThreshold(GameObject obj)
	{
		float result = this.SmallObjectIgnoreOverlapThreshold;
		DropTriggerScript component = obj.GetComponent<DropTriggerScript>();
		if (component && component.CustomSmallObjectIgnoreOverlapThreshold != 0f)
		{
			result = component.CustomSmallObjectIgnoreOverlapThreshold;
		}
		return result;
	}

	// Token: 0x060004DE RID: 1246 RVA: 0x000322FC File Offset: 0x000304FC
	private float _EnsureObjectDoesntColliderWithPlayerByTestingOverlap(GameObject obj, float resizeDistance)
	{
		if (resizeDistance == float.PositiveInfinity || resizeDistance == 3.40282347E+38f)
		{
			return 1f;
		}
		int i = 0;
		CharacterController component = GameManager.GM.player.GetComponent<CharacterController>();
		Vector3 center = component.center;
		Vector3 vector = component.center - Vector3.up * (component.height * 0.5f - component.radius);
		Vector3 vector2 = component.center + Vector3.up * (component.height * 0.5f - component.radius);
		float num = component.radius;
		num *= component.transform.localScale.x;
		vector = component.transform.TransformPoint(vector);
		vector2 = component.transform.TransformPoint(vector2);
		float num2 = 1f;
		float b = 0f;
		float b2 = 1f;
		this.UpdateObjectTransformAndPropertyByDistance(obj, resizeDistance, null, num2);
		float num3 = 0f;
		float num4 = this._GetSmallObjectThreshold(obj);
		if (num4 > 0f)
		{
			float num5 = this._EstimateObjectSize(obj);
			if (num5 < num4)
			{
				return 1f;
			}
			num3 = num4 / num5;
		}
		while (i < 10)
		{
			int num6 = Physics.OverlapCapsuleNonAlloc(vector, vector2, num, ResizeScript.lotsOfColliders, this._layerMaskGrabbed);
			if (num6 == 0 && i == 0)
			{
				break;
			}
			if (num6 > 0)
			{
				b2 = num2;
				num2 = Mathf.Lerp(num2, b, 0.5f);
			}
			else
			{
				b = num2;
				num2 = Mathf.Lerp(num2, b2, 0.5f);
			}
			this.UpdateObjectTransformAndPropertyByDistance(obj, resizeDistance, null, num2);
			i++;
		}
		if (num2 < num3)
		{
			num2 = num3;
		}
		return num2;
	}

	// Token: 0x060004DF RID: 1247 RVA: 0x00032484 File Offset: 0x00030684
	private float _EnsureObjectDoesntColliderWithPlayer(GameObject obj, List<Vector3> objVertices, float resizeDistance, float furtherestDistance)
	{
		if (resizeDistance == float.PositiveInfinity)
		{
			return 1f;
		}
		Vector3 a = Vector3.Normalize(obj.transform.position - base.transform.position);
		float d = resizeDistance / this._GetMinGrabDistance();
		Matrix4x4 matrix4x = Matrix4x4.TRS(base.transform.position + a * resizeDistance, base.transform.localToWorldMatrix.rotation * this._GetCurrentObjectRotationCameraSpace(), this.scaleAtMinDistance * d);
		for (int i = 0; i < objVertices.Count; i++)
		{
			objVertices[i] = matrix4x.MultiplyPoint(objVertices[i]);
		}
		Vector3 vector = base.transform.position + a * resizeDistance;
		float num = 1f;
		foreach (Vector3 a2 in objVertices)
		{
			Vector3 direction = a2 - vector;
			float magnitude = direction.magnitude;
			RaycastHit raycastHit;
			if (Physics.Raycast(vector, direction, out raycastHit, magnitude, this._layerMaskOnlyPlayer))
			{
				num = Mathf.Min(num, raycastHit.distance / magnitude);
			}
		}
		return num;
	}

	// Token: 0x060004E0 RID: 1248 RVA: 0x000325D0 File Offset: 0x000307D0
	private void _PopulateRaysScreenspace(Camera theCamera, List<Vector3> objWorldSpacePoints)
	{
		Vector3 vector = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 vector2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		foreach (Vector3 position in objWorldSpacePoints)
		{
			Vector3 vector3 = theCamera.WorldToScreenPoint(position);
			if (vector3.x < vector.x)
			{
				vector.x = vector3.x;
			}
			if (vector3.y < vector.y)
			{
				vector.y = vector3.y;
			}
			if (vector3.x > vector2.x)
			{
				vector2.x = vector3.x;
			}
			if (vector3.y > vector2.y)
			{
				vector2.y = vector3.y;
			}
		}
		Ray ray = theCamera.ScreenPointToRay(vector);
		Ray ray2 = theCamera.ScreenPointToRay(vector2);
		Debug.DrawRay(ray.origin, ray.direction, Color.red);
		Debug.DrawRay(ray2.origin, ray2.direction, Color.red);
		int num = (int)((vector2.x - vector.x) / 20f);
		int num2 = (int)((vector2.y - vector.y) / 20f);
		num = Mathf.Clamp(num, 20, 40);
		num2 = Mathf.Clamp(num2, 20, 40);
		float num3 = (vector2.x - vector.x) / (float)num;
		float num4 = (vector2.y - vector.y) / (float)num2;
		for (int i = 0; i < num2; i++)
		{
			float y = vector.y + (float)i * num4;
			for (int j = 0; j < num; j++)
			{
				float x = vector.x + (float)j * num3;
				Vector3 pos = new Vector3(x, y, 0f);
				Ray item = theCamera.ScreenPointToRay(pos);
				this.rayListWorker.Add(item);
			}
		}
	}

	// Token: 0x060004E1 RID: 1249 RVA: 0x000327DC File Offset: 0x000309DC
	private void _PopulateRaysSpherical(Camera theCamera, List<Vector3> objWorldSpacePoints, Vector3 cameraToObjectDirection)
	{
		Vector3 position = base.transform.position;
		Vector3 right = base.transform.right;
		Vector3 up = base.transform.up;
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		float num3 = float.MinValue;
		float num4 = float.MinValue;
		foreach (Vector3 a in objWorldSpacePoints)
		{
			float b = Vector3.SignedAngle(cameraToObjectDirection, a - position, right);
			num2 = Mathf.Min(num2, b);
			num4 = Mathf.Max(num4, b);
			float b2 = Vector3.SignedAngle(cameraToObjectDirection, a - position, up);
			num = Mathf.Min(num, b2);
			num3 = Mathf.Max(num3, b2);
		}
		float num5 = num3 - num;
		int num6 = Mathf.CeilToInt(num5 * 0.5f);
		num6 = Mathf.Clamp(num6, 20, 40);
		float num7 = num4 - num2;
		int num8 = Mathf.CeilToInt(num7 * 0.5f);
		num8 = Mathf.Clamp(num8, 20, 40);
		float num9 = num5 / (float)num6;
		float num10 = num7 / (float)num8;
		for (int i = 0; i <= num8; i++)
		{
			Vector3 point = Quaternion.AngleAxis(Mathf.Lerp(num2, num4, (float)i / (float)num8), right) * cameraToObjectDirection;
			for (int j = 0; j <= num6; j++)
			{
				Vector3 direction = Quaternion.AngleAxis(Mathf.Lerp(num, num3, (float)j / (float)num6), up) * point;
				this.rayListWorker.Add(new Ray(position, direction));
			}
		}
	}

	// Token: 0x060004E2 RID: 1250 RVA: 0x00032970 File Offset: 0x00030B70
	private float getResizeDistance(GameObject obj, out Portal crossedPortal, out float reductionRatio)
	{
		float resizeDistanceWorker = this.getResizeDistanceWorker(obj, out crossedPortal, out reductionRatio, 1f);
		if (resizeDistanceWorker == float.PositiveInfinity || resizeDistanceWorker == 3.40282347E+38f)
		{
			resizeDistanceWorker = this.getResizeDistanceWorker(obj, out crossedPortal, out reductionRatio, 2f);
		}
		return resizeDistanceWorker;
	}

	// Token: 0x060004E3 RID: 1251 RVA: 0x000329AC File Offset: 0x00030BAC
	private float getResizeDistanceWorker(GameObject obj, out Portal crossedPortal, out float reductionRatio, float minDistanceScale)
	{
		this.swHitGrab.Reset();
		this.swTotal.Reset();
		this.swTotal.Start();
		this.Info.DidntHitBack = 0;
		this.Info.DidntHitWall = 0;
		this.Info.DidntHitGrabbed = 0;
		reductionRatio = 1f;
		Vector3 cameraToObjectDirection = this.ResetObjectToMinDistance(obj, minDistanceScale);
		this.rayListWorker.Clear();
		Mesh sharedMesh = obj.GetComponent<MeshFilter>().sharedMesh;
		this.swCalcBounds.Reset();
		this.swCalcBounds.Start();
		this.objWorldSpacePoints.Clear();
		this.objPoints.Clear();
		sharedMesh.GetVertices(this.objPoints);
		this.objWorldSpacePoints.AddRange(this.objPoints);
		int num = 200;
		bool flag = this.objWorldSpacePoints.Count < num;
		Camera component = base.GetComponent<Camera>();
		for (int i = 0; i < this.objWorldSpacePoints.Count; i++)
		{
			this.objWorldSpacePoints[i] = obj.transform.TransformPoint(this.objWorldSpacePoints[i]);
			if (flag)
			{
				Ray item = new Ray(base.transform.position, this.objWorldSpacePoints[i] - base.transform.position);
				this.rayListWorker.Add(item);
			}
		}
		this.Info.ObjectVertexCount = this.rayListWorker.Count;
		if (this.UseSphericalBounds)
		{
			this._PopulateRaysSpherical(component, this.objWorldSpacePoints, cameraToObjectDirection);
		}
		else
		{
			this._PopulateRaysScreenspace(component, this.objWorldSpacePoints);
		}
		for (int j = this.Info.ObjectVertexCount; j < this.rayListWorker.Count; j++)
		{
			Debug.DrawRay(this.rayListWorker[j].origin, this.rayListWorker[j].direction, Color.red);
		}
		this.swCalcBounds.Stop();
		this.Info.CalcBoundsMS = (float)this.swCalcBounds.Elapsed.TotalSeconds * 1000f;
		this.Info.ScreenSpaceVertexCount = this.rayListWorker.Count - this.Info.ObjectVertexCount;
		if (GameManager.GM.debugModeOn)
		{
			GameManager.GM.MyLog("Rays shot: " + this.rayListWorker.Count);
		}
		int num2 = 0;
		int num3 = 0;
		float num4 = float.MaxValue;
		float num5 = float.MaxValue;
		crossedPortal = null;
		float furtherestDistance = 0f;
		float minGrabDistance = this._GetMinGrabDistance();
		for (int k = 0; k < this.rayListWorker.Count; k++)
		{
			Ray forwardRay = this.rayListWorker[k];
			float num6;
			float num7;
			Portal portal;
			if (this.getCurrentRayResizeDistance(minGrabDistance, forwardRay, out num6, out num7, out portal, ref furtherestDistance))
			{
				if (portal)
				{
					num2++;
				}
				num3++;
				if (portal && portal.AllowObjectsOnBoundary)
				{
					num7 = num6;
				}
				if (num6 < num4 && num6 > 0f)
				{
					num4 = num6;
					crossedPortal = portal;
				}
				if (num7 < num5 && num7 > 0f)
				{
					num5 = num7;
				}
			}
		}
		if ((crossedPortal && num2 < num3) || !crossedPortal)
		{
			crossedPortal = null;
			if (num5 != float.PositiveInfinity && num5 < 3.40282347E+38f)
			{
				num4 = num5;
			}
		}
		this.swEnsureNoPlayerCollide.Reset();
		this.swEnsureNoPlayerCollide.Start();
		if (!crossedPortal)
		{
			if (this.UseOverlapForAvoidingPlayerCollision)
			{
				reductionRatio = this._EnsureObjectDoesntColliderWithPlayerByTestingOverlap(obj, num4);
			}
			else
			{
				reductionRatio = this._EnsureObjectDoesntColliderWithPlayer(obj, this.objPoints, num4, furtherestDistance);
			}
		}
		this.swEnsureNoPlayerCollide.Stop();
		this.Info.PlayerNoCollideMS = (float)this.swEnsureNoPlayerCollide.Elapsed.TotalSeconds * 1000f;
		this.Info.ReductionRatio = reductionRatio;
		this.swTotal.Stop();
		this.Info.TotalMS = (float)this.swTotal.Elapsed.TotalSeconds * 1000f;
		this.Info.HitGrabMS = (float)this.swHitGrab.Elapsed.TotalSeconds * 1000f;
		return num4;
	}

	// Token: 0x060004E4 RID: 1252 RVA: 0x00032DEC File Offset: 0x00030FEC
	private Portal _MaybeGetHitPortal(ref RaycastHit hit)
	{
		Portal portal = null;
		if (hit.collider.CompareTag(Portal.PortalHitTestTag))
		{
			portal = hit.collider.gameObject.transform.parent.gameObject.GetComponent<Portal>();
			if (portal && !portal.CanProjectThroughMe)
			{
				portal = null;
			}
		}
		return portal;
	}

	// Token: 0x060004E5 RID: 1253 RVA: 0x00032E40 File Offset: 0x00031040
	private bool getCurrentRayResizeDistance(float minGrabDistance, Ray forwardRay, out float distance, out float distanceNonPortal, out Portal crossedPortal, ref float maxCastDistance)
	{
		this.swHitGrab.Start();
		if (!Physics.Raycast(base.transform.position, forwardRay.direction, float.PositiveInfinity, this._layerMaskGrabbed))
		{
			this.swHitGrab.Stop();
			this.Info.DidntHitGrabbed = this.Info.DidntHitGrabbed + 1;
			distance = -1f;
			distanceNonPortal = float.PositiveInfinity;
			crossedPortal = null;
			return false;
		}
		this.swHitGrab.Stop();
		RaycastHit raycastHit;
		if (!Physics.Raycast(base.transform.position, forwardRay.direction, out raycastHit, float.PositiveInfinity, this._layerMaskIgnoreGrabbed))
		{
			this.Info.DidntHitWall = this.Info.DidntHitWall + 1;
			distance = float.PositiveInfinity;
			distanceNonPortal = float.PositiveInfinity;
			crossedPortal = null;
			return false;
		}
		maxCastDistance = Mathf.Max(new float[]
		{
			raycastHit.distance
		});
		if (raycastHit.collider.CompareTag(ResizeScript.StopProjectionTag))
		{
			this.isRejectProject = true;
		}
		if (this.isEnableDebug)
		{
			Debug.DrawLine(base.transform.position, raycastHit.point, Color.blue);
		}
		Vector3 point = raycastHit.point;
		Vector3 vector = base.transform.position - point;
		float num = 0f;
		Portal portal = this._MaybeGetHitPortal(ref raycastHit);
		crossedPortal = null;
		if (portal)
		{
			Ray ray = PortalHelper.TransformRay(portal, new Ray(raycastHit.point, forwardRay.direction));
			ray.origin += Vector3.Normalize(ray.direction) * 0.1f;
			RaycastHit raycastHit2;
			if (!Physics.Raycast(ray.origin, ray.direction, out raycastHit2, float.PositiveInfinity, this._layerMaskIgnoreGrabbed))
			{
				distance = float.PositiveInfinity;
				distanceNonPortal = float.PositiveInfinity;
				return false;
			}
			crossedPortal = portal;
			num = 0.1f + raycastHit2.distance;
		}
		float num2 = 0f;
		RaycastHit raycastHit3;
		if (Physics.Raycast(point - vector * num2, vector, out raycastHit3, float.PositiveInfinity, this._layerMaskGrabbed))
		{
			float num3 = raycastHit3.distance - num2;
			distance = minGrabDistance * (raycastHit.distance + num) / (raycastHit.distance - num3);
			distanceNonPortal = minGrabDistance * raycastHit.distance / (raycastHit.distance - num3);
			return true;
		}
		this.Info.DidntHitBack = this.Info.DidntHitBack + 1;
		distance = float.PositiveInfinity;
		distanceNonPortal = float.PositiveInfinity;
		return false;
	}

	// Token: 0x060004E6 RID: 1254 RVA: 0x000330AC File Offset: 0x000312AC
	private void setFollowVelocity(Vector3 grabbed_Pos)
	{
		Vector3 normalized = base.transform.parent.GetComponent<CharacterMotor>().GetVelocity().normalized;
		Vector3 vector = (this.grabbedObject.transform.position - this.prevPosition) * 65f;
		float maxLength = 20f;
		this.grabbedObject.GetComponent<Rigidbody>().velocity = Vector3.ClampMagnitude(vector, maxLength);
	}

	// Token: 0x060004E7 RID: 1255 RVA: 0x0003311C File Offset: 0x0003131C
	private void SetScaleAtMinDistance(GameObject obj, float initialRatio, Portal crossedPortal)
	{
		float d = 1f;
		if (crossedPortal)
		{
			d = 1f / PortalHelper.CalculateFromToPortalScale(crossedPortal);
		}
		this.scaleAtMinDistance = obj.transform.localScale * initialRatio * d;
		this.massAtMinDistance = Mathf.Pow(initialRatio, 3f) * this.grabbedObject.GetComponent<Rigidbody>().mass * 1f;
		this.grabbedMinGrabDistance = this._GetMinGrabDistance();
		DropTriggerScript component = obj.GetComponent<DropTriggerScript>();
		if (component.maximumAllowedMass > 0f)
		{
			this.massAtMinDistance = Mathf.Pow(initialRatio, 3f) * component.hiddenMassIgnoringMaximum * 1f;
		}
	}

	// Token: 0x060004E8 RID: 1256 RVA: 0x000331C8 File Offset: 0x000313C8
	private float _GetLerpProgress()
	{
		float num = 1f - this.curLerpingTime / 0.8f;
		return 1f - num * num;
	}

	// Token: 0x060004E9 RID: 1257 RVA: 0x000331F4 File Offset: 0x000313F4
	private Vector3 _GetDirectionToObject()
	{
		Vector3 b = base.transform.TransformVector(Vector3.Normalize(this.positionInCameraSpaceOriginalFinal));
		if (this.isLerpingToPosition)
		{
			Vector3 vector = Vector3.Normalize(this.positionInCameraSpaceOriginal);
			return Vector3.Slerp(base.transform.TransformVector(vector), b, this._GetLerpProgress()).normalized;
		}
		return b.normalized;
	}

	// Token: 0x060004EA RID: 1258 RVA: 0x00033254 File Offset: 0x00031454
	private Vector3 ResetObjectToMinDistance(GameObject obj, float minDistanceScale)
	{
		Vector3 vector = this._GetDirectionToObject();
		obj.transform.rotation = base.transform.localToWorldMatrix.rotation * this._GetCurrentObjectRotationCameraSpace();
		obj.transform.position = base.transform.position + vector * this._GetMinGrabDistance() * minDistanceScale;
		obj.transform.localScale = this.scaleAtMinDistance;
		obj.GetComponent<Rigidbody>().mass = this.massAtMinDistance;
		return vector;
	}

	// Token: 0x060004EB RID: 1259 RVA: 0x000332E4 File Offset: 0x000314E4
	private void UpdateObjectTransformAndPropertyByDistance(GameObject obj, float distance, Portal crossedPortal, float reductionRatio)
	{
		DropTriggerScript component = obj.GetComponent<DropTriggerScript>();
		distance *= 0.99f * component.grabValues.DistanceScaleModifier;
		Vector3 vector = Vector3.Normalize(obj.transform.position - base.transform.position);
		if (crossedPortal)
		{
			float num = 0f;
			float num2 = distance;
			Vector3 vector2 = base.transform.position;
			Vector3 vector3 = vector;
			Quaternion rotation = base.transform.localToWorldMatrix.rotation * this._GetCurrentObjectRotationCameraSpace();
			if (this.isEnableDebug)
			{
				Debug.DrawRay(vector2, vector3 * 3f, Color.magenta);
			}
			Ray ray = new Ray(vector2, vector3);
			float num3;
			if (!crossedPortal.GetPortalPlane().Raycast(ray, out num3))
			{
				num3 = num2;
				Debug.LogWarning("Problem with portal ray.");
			}
			if (this.isEnableDebug)
			{
				Debug.DrawRay(vector2, ray.direction * num3, Color.green);
			}
			Vector3 origin = ray.origin + ray.direction * num3;
			float num4 = num + num3 * PortalHelper.CalculateFromToPortalScale(crossedPortal);
			num2 -= num3;
			Ray ray2 = PortalHelper.TransformRay(crossedPortal, new Ray(origin, vector3));
			vector2 = ray2.origin;
			vector3 = ray2.direction;
			rotation = PortalHelper.TransformRotation(crossedPortal, rotation);
			if (this.isEnableDebug)
			{
				Debug.DrawRay(vector2, num2 * vector3, Color.green);
			}
			float num5 = (num4 + num2) / this._GetMinGrabDistance();
			obj.transform.rotation = rotation;
			obj.transform.position = vector2 + num2 * vector3;
			obj.transform.localScale = this.scaleAtMinDistance * num5;
			this.UpdateObjectMass(obj, num5);
		}
		else
		{
			float num6 = distance / this._GetMinGrabDistance();
			obj.transform.rotation = base.transform.localToWorldMatrix.rotation * this._GetCurrentObjectRotationCameraSpace();
			obj.transform.position = base.transform.position + distance * vector;
			obj.transform.localScale = this.scaleAtMinDistance * num6 * reductionRatio;
			this.UpdateObjectMass(obj, num6);
		}
		obj.GetComponent<Rigidbody>().position = obj.transform.position;
	}

	// Token: 0x060004EC RID: 1260 RVA: 0x00005502 File Offset: 0x00003702
	public float getGrabDistance()
	{
		return this._GetMinGrabDistance();
	}

	// Token: 0x060004ED RID: 1261 RVA: 0x00033538 File Offset: 0x00031738
	private void UpdateObjectMass(GameObject obj, float ratio)
	{
		Rigidbody component = obj.GetComponent<Rigidbody>();
		DropTriggerScript component2 = obj.GetComponent<DropTriggerScript>();
		component.mass = Mathf.Pow(ratio, 3f) * this.massAtMinDistance * 1f;
		if (component2.maximumAllowedMass > 0f)
		{
			component2.hiddenMassIgnoringMaximum = component.mass;
			component.mass = Mathf.Clamp(component.mass, 0f, component2.maximumAllowedMass);
		}
	}

	// Token: 0x060004EE RID: 1262 RVA: 0x000335A8 File Offset: 0x000317A8
	private void _UpdateGrabSizeStuffWhenGrabbing()
	{
		if (this.grabbedMinGrabDistance != this._GetMinGrabDistance())
		{
			float num = this._GetMinGrabDistance() / this.grabbedMinGrabDistance;
			this.grabbedMinGrabDistance = this._GetMinGrabDistance();
			this.scaleAtMinDistance *= num;
			this.massAtMinDistance *= num;
		}
	}

	// Token: 0x060004EF RID: 1263 RVA: 0x0000550A File Offset: 0x0000370A
	public void setGrabDistance(float f)
	{
		Debug.LogError("setGrabDistance no longer supported.");
	}

	// Token: 0x060004F0 RID: 1264 RVA: 0x00005516 File Offset: 0x00003716
	public void TurnPlayerGrabOffNextFrame()
	{
		this.notAllowedToGrab = true;
	}

	// Token: 0x060004F1 RID: 1265 RVA: 0x0000551F File Offset: 0x0000371F
	public void ToggleImmediateDropAndNoLerp()
	{
		this.dontLerpObjectsToCenter = !this.dontLerpObjectsToCenter;
		this.immediatelyDropObjects = !this.immediatelyDropObjects;
	}

	// Token: 0x060004F2 RID: 1266 RVA: 0x0000553F File Offset: 0x0000373F
	public void StartHoldRotateTimer()
	{
		this.holdRotateTutorialTimer = 3f;
	}

	// Token: 0x04000625 RID: 1573
	public bool isEnableDebug;

	// Token: 0x04000626 RID: 1574
	[Tooltip("Try to figure out the spherical-coordinates bounds of grabbed objects so we don't ignore their off-screen parts.")]
	public bool UseSphericalBounds = true;

	// Token: 0x04000627 RID: 1575
	[Tooltip("Avoid the dropped object overlapping with the player capsule by artificially shrinking it.")]
	public bool UseOverlapForAvoidingPlayerCollision = true;

	// Token: 0x04000628 RID: 1576
	[Tooltip("We'll make no attempt to avoid player overlap when dropping objects below this large in size.")]
	public float SmallObjectIgnoreOverlapThreshold = 1f;

	// Token: 0x04000629 RID: 1577
	private const float MASS_CONTANT = 1f;

	// Token: 0x0400062A RID: 1578
	private const float SAMPLE_DENSITY_SCREEN = 20f;

	// Token: 0x0400062B RID: 1579
	private const int MAX_SAMPLE_PER_EDGE = 40;

	// Token: 0x0400062C RID: 1580
	private const int MIN_SAMPLE_PER_EDGE = 20;

	// Token: 0x0400062D RID: 1581
	public ResizeInfo Info;

	// Token: 0x0400062E RID: 1582
	private bool _isGrabbing;

	// Token: 0x0400062F RID: 1583
	private GameObject grabbedObject;

	// Token: 0x04000630 RID: 1584
	public bool isPointingAtInstantProjectObj;

	// Token: 0x04000631 RID: 1585
	private Quaternion grabbedObjectRotationInCameraSpaceOriginal;

	// Token: 0x04000632 RID: 1586
	private Vector3 positionInCameraSpaceOriginal;

	// Token: 0x04000633 RID: 1587
	private Vector3 positionInCameraSpaceOriginalFinal;

	// Token: 0x04000634 RID: 1588
	private Vector3 accumulatedPlayerRotationCameraSpaceSelfRightingLock;

	// Token: 0x04000635 RID: 1589
	public bool isReadyToGrab;

	// Token: 0x04000636 RID: 1590
	public bool isObjectBlocked;

	// Token: 0x04000637 RID: 1591
	public bool isObjectReadyToReturn;

	// Token: 0x04000638 RID: 1592
	private bool notAllowedToGrab;

	// Token: 0x04000639 RID: 1593
	private bool notAllowedToGrabNextFrame;

	// Token: 0x0400063A RID: 1594
	public SoundEffectScript soundEffectScript;

	// Token: 0x0400063B RID: 1595
	private Material DrawOutlineMat;

	// Token: 0x0400063C RID: 1596
	public PostProcessCamera drawOutlinePostProcessing;

	// Token: 0x0400063D RID: 1597
	private Color canSpinColor = new Color(1f, 1f, 1f, 1f);

	// Token: 0x0400063E RID: 1598
	private Color canFollowColor = new Color(1f, 0.3f, 0.3f, 1f);

	// Token: 0x0400063F RID: 1599
	private Color cannotDoAnythingColor = new Color(0f, 0f, 0f, 1f);

	// Token: 0x04000640 RID: 1600
	private MouseLook mouseLook1;

	// Token: 0x04000641 RID: 1601
	private MouseLook mouseLook2;

	// Token: 0x04000642 RID: 1602
	private bool isRejectProject;

	// Token: 0x04000643 RID: 1603
	private bool toResizeWithoutDropping;

	// Token: 0x04000644 RID: 1604
	private const float MinGrabDistance = 0.25f;

	// Token: 0x04000645 RID: 1605
	private float _maxGrabDistance = 400f;

	// Token: 0x04000646 RID: 1606
	private bool isObjectInSky;

	// Token: 0x04000647 RID: 1607
	private int _layerMaskIgnoreGrabbed;

	// Token: 0x04000648 RID: 1608
	private int _layerMaskGrabbed;

	// Token: 0x04000649 RID: 1609
	private int _layerMaskIgnorePlayer;

	// Token: 0x0400064A RID: 1610
	private int _layerMaskOnlyPlayer;

	// Token: 0x0400064B RID: 1611
	public Texture SpinInstruction;

	// Token: 0x0400064C RID: 1612
	public Texture DropInstruction;

	// Token: 0x0400064D RID: 1613
	public PostProcessCamera BlockedPostProcessing;

	// Token: 0x0400064E RID: 1614
	public Material blockedMat;

	// Token: 0x0400064F RID: 1615
	public OutlinerCamera outlinerCamera;

	// Token: 0x04000650 RID: 1616
	private bool isLerpingToPosition;

	// Token: 0x04000651 RID: 1617
	private Vector3 prevPosition = Vector3.zero;

	// Token: 0x04000652 RID: 1618
	public bool dontLerpObjectsToCenter;

	// Token: 0x04000653 RID: 1619
	public bool immediatelyDropObjects;

	// Token: 0x04000654 RID: 1620
	public MOSTTriggerOnXDrops triggerOnDrops;

	// Token: 0x04000655 RID: 1621
	private GameObject oldObj;

	// Token: 0x04000656 RID: 1622
	public int repeatInstantProjectCount;

	// Token: 0x04000657 RID: 1623
	public int repeatInstantProjectMax;

	// Token: 0x04000658 RID: 1624
	private const float HoldRotateTutorialTimerBase = 3f;

	// Token: 0x04000659 RID: 1625
	private float holdRotateTutorialTimer;

	// Token: 0x0400065A RID: 1626
	public bool doRotateTutInThisLevel;

	// Token: 0x0400065B RID: 1627
	public Image rotateTut;

	// Token: 0x0400065C RID: 1628
	[Tooltip("Min/max range for adding a random rotation when an object is dropped.")]
	public Vector2 RandomTorqueOnDrop = Vector2.zero;

	// Token: 0x0400065D RID: 1629
	[Tooltip("Should random torque be limited to a plane, so that the object has less chance of rotating into a wall and bouncing out.")]
	public bool LimitRandomTorqueToCameraPlane = true;

	// Token: 0x0400065E RID: 1630
	[Tooltip("World sized radius for grab object hit testing.")]
	public float CastRadius = 0.1f;

	// Token: 0x0400065F RID: 1631
	private float curLerpingTime;

	// Token: 0x04000660 RID: 1632
	private const float MaxLerpingTime = 0.8f;

	// Token: 0x04000661 RID: 1633
	private Portal lastCrossedPortal;

	// Token: 0x04000662 RID: 1634
	private MaterialPool fadeMaterialPool;

	// Token: 0x04000663 RID: 1635
	private const float AlphaFade = 0.5f;

	// Token: 0x04000664 RID: 1636
	private const float TooSmallEpsilon = 0.0001f;

	// Token: 0x04000665 RID: 1637
	public Vector3 spinGUIStartinPoint;

	// Token: 0x04000666 RID: 1638
	public Vector3 dropGUIStartinPoint;

	// Token: 0x04000667 RID: 1639
	private static RaycastHit[] hitWorker = new RaycastHit[20];

	// Token: 0x04000668 RID: 1640
	private const float Smidgen = 0.1f;

	// Token: 0x04000669 RID: 1641
	private static readonly Collider[] lotsOfColliders = new Collider[100];

	// Token: 0x0400066A RID: 1642
	private const int MaxIterations = 10;

	// Token: 0x0400066B RID: 1643
	private List<Ray> rayListWorker = new List<Ray>();

	// Token: 0x0400066C RID: 1644
	private List<Vector3> objWorldSpacePoints = new List<Vector3>();

	// Token: 0x0400066D RID: 1645
	private List<Vector3> objPoints = new List<Vector3>();

	// Token: 0x0400066E RID: 1646
	private const float RaysPerDegree = 0.5f;

	// Token: 0x0400066F RID: 1647
	private static readonly string StopProjectionTag = "Stop projection";

	// Token: 0x04000670 RID: 1648
	private static readonly string ChildThatIgnoresGrabTag = "ChildThatIgnoresGrab";

	// Token: 0x04000671 RID: 1649
	private readonly Stopwatch swTotal = new Stopwatch();

	// Token: 0x04000672 RID: 1650
	private readonly Stopwatch swEnsureNoPlayerCollide = new Stopwatch();

	// Token: 0x04000673 RID: 1651
	private readonly Stopwatch swCalcBounds = new Stopwatch();

	// Token: 0x04000674 RID: 1652
	private readonly Stopwatch swHitGrab = new Stopwatch();

	// Token: 0x04000675 RID: 1653
	private Vector3 scaleAtMinDistance;

	// Token: 0x04000676 RID: 1654
	private float massAtMinDistance;

	// Token: 0x04000677 RID: 1655
	private float grabbedMinGrabDistance;

	// Token: 0x04000678 RID: 1656
	public Text grabText;
}
