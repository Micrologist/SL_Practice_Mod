using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// Token: 0x02000202 RID: 514
[RequireComponent(typeof(Button))]
public class PlayCreditsButton : MonoBehaviour
{
	// Token: 0x06000917 RID: 2327
	private void Awake()
	{
		base.GetComponent<Button>().onClick.AddListener(new UnityAction(this.PlayCredits));
		base.GetComponent<Button>().GetComponentInChildren<Text>().text = "PRACTICE MOD";
	}

	// Token: 0x06000918 RID: 2328
	private void PlayCredits()
	{
		base.GetComponentInChildren<Text>().text = "BY MICROLOGIST";
	}

	// Token: 0x06000919 RID: 2329 RVA: 0x00007BE5 File Offset: 0x00005DE5
	private void PlayCreditsInternal()
	{
		Button component = base.GetComponent<Button>();
		component.interactable = false;
		UnityEngine.Object.FindObjectOfType<CreditsPlayer>().RollCredits();
		component.interactable = true;
	}

	// Token: 0x04000A2B RID: 2603
	private bool isLoadingCredits;
}
