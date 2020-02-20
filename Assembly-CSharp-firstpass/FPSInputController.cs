using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

// Token: 0x0200002F RID: 47
[RequireComponent(typeof(CharacterMotor))]
[AddComponentMenu("Character/FPS Input Controller")]
public class FPSInputController : MonoBehaviour
{
	// Token: 0x06000112 RID: 274 RVA: 0x00002F3B File Offset: 0x0000113B
	private void Awake()
	{
		this.motor = base.GetComponent<CharacterMotor>();
	}

	// Token: 0x06000113 RID: 275 RVA: 0x00013CD0 File Offset: 0x00011ED0
	private void Start()
	{
		this.cameraObj = base.GetComponentInChildren<Camera>().gameObject;
		this.defaultFarClippingPlane = this.cameraObj.GetComponent<Camera>().farClipPlane;
		this.standCameraY = this.cameraObj.transform.localPosition.y;
		this.crouchCameraY = this.standCameraY - this.crouchAmount;
		this.flashLight = new GameObject("Flashlight");
		this.flashLight.transform.parent = base.transform;
		this.flashLight.transform.localPosition = new Vector3(0f, base.GetComponentInChildren<Camera>().transform.localPosition.y, 0f);
		Light light = this.flashLight.AddComponent<Light>();
		light.range = 10000f;
		light.intensity = 0.5f;
		this.namesDoor = new List<string>
		{
			"DOOR_OPEN_002_A_PULL_OFF_BACK_GAMEPLAY",
			"DOOR_OPEN_002_PULL_OFF_BACK_GAMEPLAY",
			"DOOR_OPEN_002_PULL_OFF_COMMON_GAMEPLAY"
		};
		this.namesSign = new List<string>
		{
			"DOOR_SIGN_001_CHAMBER_GAMEPLAY",
			"EXIT_SIGN_001_COMMON_GAMEPLAY"
		};
		this.namesCube = new List<string>
		{
			"TOY_BLOCK_CHAMBER_GAMEPLAY",
			"ILLUSION_CUBE_001_MUSEUM_GAMEPLAY",
			"TOYCUBE"
		};
	}

	// Token: 0x06000114 RID: 276 RVA: 0x00013E28 File Offset: 0x00012028
	private void Update()
	{
		if (!GameManager.GM.PM.canControl)
		{
			this.motor.inputMoveDirection = Vector3.zero;
			return;
		}
		if (Input.GetKeyDown(KeyCode.K))
		{
			GameManager.GM.noClip = !GameManager.GM.noClip;
			this.noClipSpeed = 10f;
		}
		this.directionVector = new Vector3(GameManager.GM.playerInput.GetAxis("Move Horizontal"), 0f, GameManager.GM.playerInput.GetAxis("Move Vertical"));
		if (this.directionVector != Vector3.zero)
		{
			float num = this.directionVector.magnitude;
			this.directionVector /= num;
			num = Mathf.Min(1f, num);
			num *= num;
			this.directionVector *= num;
		}
		if (this.moveInCameraDirection)
		{
			Quaternion rotation = GameManager.GM.playerCamera.transform.rotation;
			rotation.eulerAngles = new Vector3(0f, rotation.eulerAngles.y, 0f);
			this.motor.inputMoveDirection = rotation * this.directionVector;
		}
		else
		{
			this.motor.inputMoveDirection = base.transform.rotation * this.directionVector;
		}
		bool button = GameManager.GM.playerInput.GetButton("Jump");
		Controller lastActiveController = ReInput.controllers.GetLastActiveController();
		bool flag = GameManager.GM.playerInput.GetButton("UISubmit") && (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Joystick);
		this.motor.inputJump = (button && !GameManager.GM.ignoreNextJump);
		if (!button && !flag && GameManager.GM.ignoreNextJump)
		{
			GameManager.GM.ignoreNextJump = false;
		}
		if (Input.GetKey(KeyCode.LeftShift))
		{
			this.motor.setMotorMaxSpeed(this.runSpeed);
		}
		else
		{
			this.motor.setMotorMaxSpeed(this.walkSpeed);
		}
		Vector3 localPosition = this.cameraObj.transform.localPosition;
		if (!this.cannotCrouch)
		{
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C))
			{
				localPosition.y = Mathf.Lerp(this.cameraObj.transform.localPosition.y, this.crouchCameraY, this.lerpSpeed * Time.deltaTime);
			}
			else
			{
				localPosition.y = Mathf.Lerp(this.cameraObj.transform.localPosition.y, this.standCameraY, this.lerpSpeed * Time.deltaTime);
			}
			this.cameraObj.transform.localPosition = localPosition;
		}
		if (Debug.isDebugBuild)
		{
			if (Input.GetKeyDown(KeyCode.LeftBracket))
			{
				this.walkSpeed -= 0.2f;
			}
			if (Input.GetKeyDown(KeyCode.RightBracket))
			{
				this.walkSpeed += 0.2f;
			}
		}
		if (GameObject.Find("PlayerText") == null && GameObject.Find("UI_PAUSE_MENU") != null)
		{
			GameObject gameObject = new GameObject("PlayerText");
			gameObject.transform.parent = GameObject.Find("UI_PAUSE_MENU").transform.Find("Canvas");
			gameObject.AddComponent<CanvasGroup>().interactable = false;
			this.playerText = gameObject.AddComponent<Text>();
			RectTransform component = this.playerText.GetComponent<RectTransform>();
			component.sizeDelta = new Vector2((float)(Screen.currentResolution.width / 3), (float)(Screen.currentResolution.height / 3));
			component.pivot = new Vector2(0f, 1f);
			component.anchorMin = new Vector2(0f, 1f);
			component.anchorMax = new Vector2(0f, 1f);
			component.anchoredPosition = new Vector2(25f, -25f);
			foreach (Font font in Resources.FindObjectsOfTypeAll<Font>())
			{
				if (font.name == "BebasNeue Bold")
				{
					this.playerText.font = font;
				}
			}
			this.playerText.text = "hello world";
			this.playerText.fontSize = 30;
		}
		this.playerText.text = string.Concat(new string[]
		{
			"Player\n",
			this.getLocationString(),
			"\n",
			this.GetPlayerRotationString(),
			"\n",
			this.GetPlayerScaleString(),
			"\n",
			this.GetVelocityString()
		});
		if (Time.time - this.teleportStoreTime <= 1f)
		{
			Text text = this.playerText;
			text.text += "\nteleport stored";
		}
		else if (Time.time - this.teleportTime <= 1f)
		{
			Text text2 = this.playerText;
			text2.text += "\nteleport";
		}
		if (Input.GetKeyDown(KeyCode.F))
		{
			this.flashlightOn = !this.flashlightOn;
		}
		this.flashLight.GetComponent<Light>().enabled = this.flashlightOn;
		if (this.flashlightOn)
		{
			Text text3 = this.playerText;
			text3.text += "\nFlashlight";
		}
		if (Input.GetKeyDown(KeyCode.F4))
		{
			GameManager.GM.unlimitedRenderRange = !GameManager.GM.unlimitedRenderRange;
		}
		if (GameManager.GM.noClip || GameManager.GM.unlimitedRenderRange)
		{
			this.cameraObj.GetComponent<Camera>().farClipPlane = 10000f;
			if (GameManager.GM.unlimitedRenderRange)
			{
				Text text4 = this.playerText;
				text4.text += "\nUNLIMITED VIEW DISTANCE";
			}
		}
		else
		{
			this.cameraObj.GetComponent<Camera>().farClipPlane = this.defaultFarClippingPlane;
		}
		if (GameManager.GM.noClip)
		{
			Text text5 = this.playerText;
			text5.text += "\nNOCLIP";
			this.motor.enabled = false;
			this.yInput = 0f;
			this.noClipSpeed += Input.mouseScrollDelta.y;
			this.noClipSpeed = Mathf.Max(0f, this.noClipSpeed);
			if (Input.GetKey(KeyCode.Space))
			{
				this.yInput += 1f;
			}
			if (Input.GetKey(KeyCode.C) || Input.GetKey(KeyCode.LeftControl))
			{
				this.yInput -= 1f;
			}
			this.directionVector.y = this.yInput;
			this.motor.transform.Translate(this.directionVector * Time.deltaTime * this.noClipSpeed);
		}
		else
		{
			this.motor.enabled = true;
		}
		if (GameObject.Find("DebugText") == null && GameObject.Find("UI_PAUSE_MENU") != null)
		{
			GameObject gameObject2 = new GameObject("DebugText");
			gameObject2.transform.parent = GameObject.Find("UI_PAUSE_MENU").transform.Find("Canvas");
			gameObject2.AddComponent<CanvasGroup>().interactable = false;
			this.debugText = gameObject2.AddComponent<Text>();
			RectTransform component2 = this.debugText.GetComponent<RectTransform>();
			component2.sizeDelta = new Vector2((float)(Screen.currentResolution.width / 3 * 2), (float)Screen.currentResolution.height);
			component2.pivot = new Vector2(0f, 1f);
			component2.anchorMin = new Vector2(0f, 1f);
			component2.anchorMax = new Vector2(0f, 1f);
			component2.anchoredPosition = new Vector2((float)(Screen.currentResolution.width / 3), -25f);
			foreach (Font font2 in Resources.FindObjectsOfTypeAll<Font>())
			{
				if (font2.name == "Arial")
				{
					this.debugText.font = font2;
				}
			}
			this.debugText.text = "hello world";
			this.debugText.fontSize = 15;
			this.debugText.horizontalOverflow = HorizontalWrapMode.Wrap;
		}
		if (Input.GetKeyDown(KeyCode.F12))
		{
			this.debugOutput = !this.debugOutput;
		}
		this.debugText.enabled = this.debugOutput;
		if (Input.GetKeyDown(KeyCode.F10))
		{
			this.debugText.text = "";
			foreach (GameObject gameObject3 in Resources.FindObjectsOfTypeAll<GameObject>())
			{
				if (gameObject3.layer == LayerMask.NameToLayer("CanGrab") && gameObject3.name != "")
				{
					Text text6 = this.debugText;
					text6.text = text6.text + gameObject3.name + ", ";
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			this.SpawnObjectByCategory(1);
			return;
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			this.SpawnObjectByCategory(2);
			return;
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			this.SpawnObjectByCategory(3);
		}
		if (Input.GetKeyDown(KeyCode.Alpha4))
		{
			this.SpawnObjectByCategory(4);
		}
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			this.SpawnObjectByCategory(5);
		}
		if (Input.GetKeyDown(KeyCode.F9))
		{
			GameManager.GM.cloneOnNextDrop = !GameManager.GM.cloneOnNextDrop;
		}
		if (Input.GetKeyDown(KeyCode.F10))
		{
			GameManager.GM.DeleteClone();
		}
		if (GameManager.GM.cloneOnNextDrop)
		{
			Text text7 = this.playerText;
			text7.text += "\nCloning next drop";
		}
	}

	// Token: 0x06000116 RID: 278 RVA: 0x000147F8 File Offset: 0x000129F8
	public string getLocationString()
	{
		Vector3 localPosition = this.motor.transform.localPosition;
		return string.Concat(new object[]
		{
			"Position: ",
			FPSInputController.DoRound(localPosition.x),
			", ",
			FPSInputController.DoRound(localPosition.y),
			", ",
			FPSInputController.DoRound(localPosition.z)
		});
	}

	// Token: 0x06000117 RID: 279 RVA: 0x00014864 File Offset: 0x00012A64
	public string GetVelocityString()
	{
		Vector3 velocity = this.motor.GetComponent<CharacterController>().velocity;
		return string.Concat(new object[]
		{
			"Horizontal Velocity: ",
			FPSInputController.DoRound(Mathf.Sqrt(velocity.x * velocity.x + velocity.z * velocity.z)),
			" m/s \nVertical Velocity: ",
			FPSInputController.DoRound(velocity.y),
			" m/s"
		});
	}

	// Token: 0x06000118 RID: 280 RVA: 0x000148DC File Offset: 0x00012ADC
	public string GetPlayerScaleString()
	{
		return "Scale: " + this.motor.transform.localScale.x.ToString("0.0000") + "x";
	}

	// Token: 0x06000119 RID: 281 RVA: 0x00002F72 File Offset: 0x00001172
	public static string DoRound(float myNumber)
	{
		return myNumber.ToString("0.000");
	}

	// Token: 0x0600011A RID: 282 RVA: 0x0001491C File Offset: 0x00012B1C
	public string GetPlayerRotationString()
	{
		Vector3 eulerAngles = this.motor.transform.localRotation.eulerAngles;
		return string.Concat(new string[]
		{
			"Rotation: ",
			FPSInputController.DoRound(this.cameraObj.transform.rotation.eulerAngles.x),
			", ",
			FPSInputController.DoRound(eulerAngles.y),
			", ",
			FPSInputController.DoRound(eulerAngles.z)
		});
	}

	// Token: 0x0600011B RID: 283 RVA: 0x00002F80 File Offset: 0x00001180
	private void OnDestroy()
	{
		GameManager.GM.noClip = false;
		GameManager.GM.unlimitedRenderRange = false;
	}

	// Token: 0x0600011C RID: 284 RVA: 0x000149A8 File Offset: 0x00012BA8
	public void SpawnObjectByCategory(int categoryId)
	{
		this.debugText.text = "Trying to spawn object from category " + categoryId;
		if (categoryId > 0)
		{
			List<string> list;
			switch (categoryId)
			{
			case 1:
				list = this.namesCube;
				break;
			case 2:
				list = this.namesSign;
				break;
			case 3:
				list = this.namesDoor;
				break;
			case 4:
				list = new List<string>
				{
					"SODA_CAN_INTERACTIVE_BREAKABLE"
				};
				break;
			case 5:
				list = new List<string>
				{
					"GAS_CANNISTER_002_BACK_GAMEPLAY",
					"WINDOW_BOARD_002_KITCHEN_PROP"
				};
				break;
			default:
				list = new List<string>();
				break;
			}
			foreach (string text in list)
			{
				Text text2 = this.debugText;
				text2.text = text2.text + "\n Looking for: " + text;
				foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
				{
					if (!gameObject.name.ToUpper().Contains("HALF") && gameObject.layer == LayerMask.NameToLayer("CanGrab") && gameObject.name.ToUpper().Contains(text.ToUpper()))
					{
						Text text3 = this.debugText;
						text3.text = text3.text + "\n Found " + gameObject.name;
						GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject, this.cameraObj.transform.position + this.cameraObj.transform.forward * this.motor.transform.localScale.x, Quaternion.identity);
						gameObject2.transform.localScale = Vector3.one;
						gameObject2.active = true;
						gameObject2.GetComponent<Rigidbody>().Sleep();
						return;
					}
				}
			}
		}
		Text text4 = this.debugText;
		text4.text += "\nDIDN'T FIND SHIT";
	}

	// Token: 0x0400023D RID: 573
	public CharacterMotor motor;

	// Token: 0x0400023E RID: 574
	public float runSpeed;

	// Token: 0x0400023F RID: 575
	public float walkSpeed;

	// Token: 0x04000240 RID: 576
	private float standCameraY;

	// Token: 0x04000241 RID: 577
	private float crouchCameraY;

	// Token: 0x04000242 RID: 578
	public float crouchAmount = 1f;

	// Token: 0x04000243 RID: 579
	private GameObject cameraObj;

	// Token: 0x04000244 RID: 580
	private float lerpSpeed = 3f;

	// Token: 0x04000245 RID: 581
	public bool moveInCameraDirection;

	// Token: 0x04000246 RID: 582
	public Vector3 directionVector;

	// Token: 0x04000247 RID: 583
	public bool cannotCrouch;

	// Token: 0x04000248 RID: 584
	public float yInput;

	// Token: 0x04000249 RID: 585
	public float noClipSpeed = 1f;

	// Token: 0x0400024A RID: 586
	public float defaultFarClippingPlane;

	// Token: 0x0400024B RID: 587
	public GameObject myGO;

	// Token: 0x0400024C RID: 588
	public float teleportStoreTime;

	// Token: 0x0400024D RID: 589
	public float teleportTime;

	// Token: 0x0400024E RID: 590
	public GameObject flashLight;

	// Token: 0x0400024F RID: 591
	public bool flashlightOn;

	// Token: 0x04000250 RID: 592
	public Text playerText;

	// Token: 0x04000251 RID: 593
	public Text debugText;

	// Token: 0x04000252 RID: 594
	public List<string> namesCube;

	// Token: 0x04000253 RID: 595
	public List<string> namesSign;

	// Token: 0x04000254 RID: 596
	public List<string> namesDoor;

	// Token: 0x04000255 RID: 597
	public bool debugOutput;
}
