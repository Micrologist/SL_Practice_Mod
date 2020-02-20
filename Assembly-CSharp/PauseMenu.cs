using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using Rewired;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Token: 0x020001FF RID: 511
public class PauseMenu : MonoBehaviour
{
	// Token: 0x060008FE RID: 2302 RVA: 0x0003EDE4 File Offset: 0x0003CFE4
	private void OnApplicationFocus(bool focus)
	{
		if (focus)
		{
			if (GameManager.GM != null && !CreditsPlayer.IsRolling && this.isInMenu)
			{
				bool flag;
				this.menuMusic.getPaused(out flag);
				if (flag)
				{
					this.menuMusic.setPaused(false);
					return;
				}
				this.menuMusic.start();
				return;
			}
		}
		else
		{
			if (GameManager.GM != null && GameManager.GM.PM.canControl && !CreditsPlayer.IsRolling && !this.isInMenu)
			{
				this.PauseInternal(false);
				return;
			}
			if (this.isInMenu)
			{
				this.menuMusic.setPaused(true);
			}
		}
	}

	// Token: 0x060008FF RID: 2303 RVA: 0x0003EE8C File Offset: 0x0003D08C
	private void Start()
	{
		this.menuMusic = RuntimeManager.CreateInstance(this.MenuOpen);
		this.menuPanel.GetComponent<Closable>().onClosed.AddListener(new UnityAction(this.OnClose));
		this.snapshot = RuntimeManager.CreateInstance("Snapshot:/High/Pause");
	}

	// Token: 0x06000900 RID: 2304 RVA: 0x0003EEDC File Offset: 0x0003D0DC
	private void Update()
	{
		if (GameManager.GM == null || GameManager.GM.playerInput == null)
		{
			Debug.Log("No Gamemanager or GM.playerInput found on this frame: " + Time.frameCount);
			return;
		}
		if (!this.isInMenu && !CreditsPlayer.IsRolling && GameManager.GM.playerInput.GetButtonDown("Pause"))
		{
			this.PauseInternal(true);
		}
	}

	// Token: 0x06000901 RID: 2305 RVA: 0x0003EF48 File Offset: 0x0003D148
	private void PauseInternal(bool shouldPlayMusic)
	{
		if (shouldPlayMusic)
		{
			this.menuMusic.start();
		}
		this.menuPanel.SetActive(true);
		if (GameManager.GM != null & GameManager.GM.PM != null)
		{
			GameManager.GM.PM.canControl = false;
		}
		this.prevTimeScale = Time.timeScale;
		Time.timeScale = 0f;
		this.isInMenu = true;
		Cursor.lockState = CursorLockMode.Confined;
		GameManager.GM.cursors.HideCursors();
		GameManager.GM.cursors.ShowCursor(CursorManager.CursorType.Pause);
		GameObject.Find("PlayerText").GetComponent<Text>().enabled = false;
		GameObject.Find("GrabText").GetComponent<Text>().enabled = false;
		this.snapshot.start();
		this.ToggleFMOD(true);
	}

	// Token: 0x06000902 RID: 2306 RVA: 0x00007A79 File Offset: 0x00005C79
	private void OnDestroy()
	{
		this.menuPanel.GetComponent<Closable>().onClosed.RemoveListener(new UnityAction(this.OnClose));
	}

	// Token: 0x06000903 RID: 2307 RVA: 0x00007A9C File Offset: 0x00005C9C
	public void OnClose()
	{
		GameObject.Find("PlayerText").GetComponent<Text>().enabled = true;
		GameObject.Find("GrabText").GetComponent<Text>().enabled = true;
		this.ResumeGameFunctions();
	}

	// Token: 0x06000904 RID: 2308 RVA: 0x0003F01C File Offset: 0x0003D21C
	private void ResumeGameFunctions()
	{
		if (GameManager.GM != null && GameManager.GM.PM != null)
		{
			GameManager.GM.PM.canControl = true;
		}
		Time.timeScale = this.prevTimeScale;
		this.isInMenu = false;
		Cursor.lockState = CursorLockMode.Locked;
		this.menuMusic.triggerCue();
		GameManager.GM.cursors.HideCursors();
		this.snapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this.ToggleFMOD(false);
	}

	// Token: 0x06000905 RID: 2309 RVA: 0x0003F09C File Offset: 0x0003D29C
	public void OnResumeGameButton()
	{
		this.menuPanel.GetComponent<Closable>().Close();
		Controller lastActiveController = ReInput.controllers.GetLastActiveController();
		GameManager.GM.ignoreNextJump = (lastActiveController.type == ControllerType.Keyboard || lastActiveController.type == ControllerType.Joystick);
	}

	// Token: 0x06000906 RID: 2310 RVA: 0x00007ACE File Offset: 0x00005CCE
	public void RestartLevel()
	{
		this.menuMusic.triggerCue();
		this.snapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this.menuPanel.GetComponent<Closable>().Close();
		GameManager.GM.GetComponent<SaveAndCheckpointManager>().RestartLevel();
	}

	// Token: 0x06000907 RID: 2311 RVA: 0x0003F0E4 File Offset: 0x0003D2E4
	public void GoToMenu()
	{
		this.menuMusic.triggerCue();
		this.snapshot.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		this.menuPanel.GetComponent<Closable>().Close();
		Cursor.lockState = CursorLockMode.Confined;
		Time.timeScale = this.prevTimeScale;
		base.StartCoroutine(this.LoadMainMenuAsync());
	}

	// Token: 0x06000908 RID: 2312 RVA: 0x00007B08 File Offset: 0x00005D08
	public void GoToNextLevel()
	{
		this.menuPanel.GetComponent<Closable>().Close();
		Application.LoadLevel((Application.loadedLevel + 1) % Application.levelCount);
	}

	// Token: 0x06000909 RID: 2313 RVA: 0x00007B2C File Offset: 0x00005D2C
	public void GoToSpecificLevel(int levelNum)
	{
		this.menuPanel.GetComponent<Closable>().Close();
		SceneManager.LoadScene(levelNum);
	}

	// Token: 0x0600090A RID: 2314 RVA: 0x00007B44 File Offset: 0x00005D44
	public void ResetToLastCheckpoint()
	{
		this.menuPanel.GetComponent<Closable>().Close();
		GameManager.GM.GetComponent<SaveAndCheckpointManager>().ResetToLastCheckpoint();
	}

	// Token: 0x0600090B RID: 2315 RVA: 0x00007B65 File Offset: 0x00005D65
	public void ExitGame()
	{
		this.menuPanel.GetComponent<Closable>().Close();
		Application.Quit();
	}

	// Token: 0x0600090C RID: 2316 RVA: 0x00007B7C File Offset: 0x00005D7C
	private IEnumerator LoadMainMenuAsync()
	{
		yield return LevelLoadHelper.LoadSceneAsync("StartScreen_Live", true, null, null);
		yield break;
	}

	// Token: 0x0600090D RID: 2317 RVA: 0x0003F138 File Offset: 0x0003D338
	private void ToggleFMOD(bool isPaused)
	{
		RuntimeManager.GetBus("bus:/VO").setPaused(isPaused);
		RuntimeManager.GetBus("bus:/Music").setPaused(isPaused);
		RuntimeManager.GetBus("bus:/SFX").setPaused(isPaused);
	}

	// Token: 0x04000A22 RID: 2594
	public GameObject menuPanel;

	// Token: 0x04000A23 RID: 2595
	public Texture2D cursorTexture;

	// Token: 0x04000A24 RID: 2596
	public bool isInMenu;

	// Token: 0x04000A25 RID: 2597
	private float prevTimeScale = 1f;

	// Token: 0x04000A26 RID: 2598
	private EventInstance snapshot;

	// Token: 0x04000A27 RID: 2599
	[EventRef]
	public string MenuOpen;

	// Token: 0x04000A28 RID: 2600
	public EventInstance menuMusic;
}
