using System;
using System.Linq;
using Rewired;
using UnityEngine;
using UnityEngine.XR;

// Token: 0x02000030 RID: 48
public class GameManager : MonoBehaviour
{
	// Token: 0x0600011D RID: 285 RVA: 0x00014BCC File Offset: 0x00012DCC
	private void Awake()
	{
		if (GameManager.GM == null)
		{
			GameManager.GM = this;
		}
		this.OnLoadScene();
		if (GameManager.GM != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (GameManager.GM == this)
		{
			this.OnFirstAwake();
		}
		if (this.displayMusicVolume && GameObject.Find("MOSTMusicPlayer") != null)
		{
			this.theMusic = GameObject.Find("MOSTMusicPlayer").GetComponent<AudioSource>();
		}
	}

	// Token: 0x0600011E RID: 286 RVA: 0x00014C4C File Offset: 0x00012E4C
	private void OnFirstAwake()
	{
		base.transform.SetParent(null);
		UnityEngine.Object.DontDestroyOnLoad(this);
		this.PM = new GameManager.PlayerManager();
		this.TM = new GameManager.TestingManager();
		GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.RewiredInputManager);
		this.cursors = gameObject.GetComponent<CursorManager>();
		this.cursors.HideCursors();
		this.playerInput = ReInput.players.GetPlayer("Player0");
	}

	// Token: 0x0600011F RID: 287 RVA: 0x00014CBC File Offset: 0x00012EBC
	private void OnLoadScene()
	{
		if (this.ignoreLoadSceneValues)
		{
			return;
		}
		CharacterController characterController = UnityEngine.Object.FindObjectOfType<CharacterController>();
		if (characterController == null)
		{
			Debug.LogWarning("No Player found! GM.Player not set. Exiting OnLoadScene()");
			return;
		}
		GameManager.GM.player = characterController.gameObject;
		if (XRSettings.enabled && this.VRPlayerPrefab != null)
		{
			if (GameManager.GM.player != null)
			{
				GameManager.GM.player.SetActive(false);
			}
			GameManager.GM.player = UnityEngine.Object.Instantiate<GameObject>(GameManager.GM.VRPlayerPrefab, GameManager.GM.player.transform.position, GameManager.GM.player.transform.rotation);
			GameManager.GM.player.SetActive(true);
		}
		GameManager.GM.MyLog("setting new VR Player camera");
		GameManager.GM.playerCamera = GameManager.GM.player.transform.Find("Main Camera").GetComponent<Camera>();
		GameManager.GM.guiCamera = GameManager.GM.player.transform.Find("GUI Camera").GetComponent<Camera>();
		if (this.displayMusicVolume && GameObject.Find("MOSTMusicPlayer") != null)
		{
			this.theMusic = GameObject.Find("MOSTMusicPlayer").GetComponent<AudioSource>();
		}
		GameManager.GM.voiceLineController = GameObject.Find("VoiceLineController");
	}

	// Token: 0x06000120 RID: 288 RVA: 0x00002352 File Offset: 0x00000552
	public void MyLog(string logText)
	{
	}

	// Token: 0x06000121 RID: 289 RVA: 0x00014E2C File Offset: 0x0001302C
	private void OnGUI()
	{
		if (this.displayMusicVolume && this.theMusic != null)
		{
			GUI.Label(new Rect(10f, 10f, 200f, 20f), "Music volume: " + this.theMusic.volume.ToString());
		}
	}

	// Token: 0x06000123 RID: 291
	public void CloneAsGhost(GameObject objToClone)
	{
		this.DeleteClone();
		if (!objToClone.name.ToUpper().Contains("PULL_OFF"))
		{
			this.clonedObject = UnityEngine.Object.Instantiate<GameObject>(objToClone);
			this.clonedObject.layer = LayerMask.NameToLayer("Default");
			this.clonedObject.GetComponent<Collider>().enabled = false;
			this.clonedObject.GetComponent<Rigidbody>().isKinematic = true;
			this.cloneOnNextDrop = false;
			return;
		}
		if (!objToClone.GetComponent<BoxCollider>())
		{
			return;
		}
		GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
		gameObject.GetComponent<BoxCollider>().enabled = false;
		BoxCollider component = objToClone.GetComponent<BoxCollider>();
		gameObject.transform.localRotation = objToClone.transform.localRotation;
		gameObject.transform.position = objToClone.transform.position - Quaternion.Euler(objToClone.transform.localRotation.eulerAngles) * component.center;
		gameObject.transform.localScale = component.size * objToClone.transform.localScale.x;
		gameObject.GetComponent<MeshRenderer>().material.shader = Shader.Find("Transparent/Diffuse");
		gameObject.GetComponent<MeshRenderer>().material.color = new Color(1f, 0f, 0f, 0.8f);
		GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(gameObject);
		gameObject2.transform.parent = gameObject.transform;
		Mesh mesh = gameObject2.GetComponent<MeshFilter>().mesh;
		mesh.triangles = mesh.triangles.Reverse<int>().ToArray<int>();
		this.clonedObject = gameObject;
		this.cloneOnNextDrop = false;
	}

	// Token: 0x06000124 RID: 292 RVA: 0x00002FB2 File Offset: 0x000011B2
	public void DeleteClone()
	{
		if (this.clonedObject)
		{
			UnityEngine.Object.Destroy(this.clonedObject);
			this.clonedObject = null;
		}
	}

	// Token: 0x04000256 RID: 598
	public static GameManager GM;

	// Token: 0x04000257 RID: 599
	public GameObject VRPlayerPrefab;

	// Token: 0x04000258 RID: 600
	public GameObject RewiredInputManager;

	// Token: 0x04000259 RID: 601
	public bool debugModeOn;

	// Token: 0x0400025A RID: 602
	public bool playMusic = true;

	// Token: 0x0400025B RID: 603
	public bool ignoreLoadSceneValues;

	// Token: 0x0400025C RID: 604
	[HideInInspector]
	public GameObject player;

	// Token: 0x0400025D RID: 605
	[HideInInspector]
	public Camera playerCamera;

	// Token: 0x0400025E RID: 606
	[HideInInspector]
	public Camera guiCamera;

	// Token: 0x0400025F RID: 607
	[HideInInspector]
	public Player playerInput;

	// Token: 0x04000260 RID: 608
	[HideInInspector]
	public bool ignoreNextJump;

	// Token: 0x04000261 RID: 609
	[HideInInspector]
	public CursorManager cursors;

	// Token: 0x04000262 RID: 610
	public bool useNewRewiredSystem;

	// Token: 0x04000263 RID: 611
	public GameManager.PlayerManager PM;

	// Token: 0x04000264 RID: 612
	public GameObject voiceLineController;

	// Token: 0x04000265 RID: 613
	public bool ignoreVoiceLines;

	// Token: 0x04000266 RID: 614
	public GameManager.TestingManager TM;

	// Token: 0x04000267 RID: 615
	public bool displayMusicVolume;

	// Token: 0x04000268 RID: 616
	public AudioSource theMusic;

	// Token: 0x04000269 RID: 617
	[HideInInspector]
	public float cameraFOV = 70f;

	// Token: 0x0400026A RID: 618
	[NonSerialized]
	public string sceneHandOff;

	// Token: 0x0400026B RID: 619
	[NonSerialized]
	public SaveGameState? SaveGameStateHandoff;

	// Token: 0x0400026C RID: 620
	public AudioMaterials AudioMaterials;

	// Token: 0x0400026D RID: 621
	public Vector3 teleportPosition;

	// Token: 0x0400026E RID: 622
	public Vector3 teleportScale;

	// Token: 0x0400026F RID: 623
	public Quaternion teleportRotation;

	// Token: 0x04000270 RID: 624
	public bool teleportStored;

	// Token: 0x04000271 RID: 625
	public float teleportCameraRotationY;

	// Token: 0x04000272 RID: 626
	public float lastManualLoadTime;

	// Token: 0x04000273 RID: 627
	public bool noClip;

	// Token: 0x04000274 RID: 628
	public bool unlimitedRenderRange;

	// Token: 0x04000275 RID: 629
	public bool cloneOnNextDrop;

	// Token: 0x04000276 RID: 630
	public GameObject clonedObject;

	// Token: 0x02000031 RID: 49
	public class PlayerManager
	{
		// Token: 0x04000277 RID: 631
		public bool canControl = true;
	}

	// Token: 0x02000032 RID: 50
	public class TestingManager
	{
		// Token: 0x04000278 RID: 632
		public bool isFmodTest;
	}
}
