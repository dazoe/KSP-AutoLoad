/*
 * Created by SharpDevelop.
 * User: Dave
 * Date: 4/3/2024
 * Time: 1:38 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KSP_AutoLoad.Patches {
	/// <summary>
	/// Description of LoadGameDialogPatch.
	/// </summary>
	[HarmonyPatch(typeof(LoadGameDialog))]
	static class LoadGameDialogPatch {
		private static Button autoLoadButton;
		private static bool isPersistent;
		private static bool doAutoLoad;

		static void CreateAutoLoadButton(LoadGameDialog __instance) {
			var loadButtonGO = __instance.gameObject.GetChild("ButtonLoad");
			var autoLoadButtonGO = UnityEngine.Object.Instantiate(loadButtonGO);
			autoLoadButtonGO.name = "ButtonAutoLoad";
			autoLoadButtonGO.transform.parent = loadButtonGO.transform.parent;
			var text = autoLoadButtonGO.GetChild("Text").GetComponent<TextMeshProUGUI>();
			text.text = "#autoLOC_8200121"; // Automatic ;)

			var loadButton = loadButtonGO.GetComponent<Button>();
			autoLoadButton = autoLoadButtonGO.GetComponent<Button>();
			doAutoLoad = false;
			autoLoadButton.onClick.AddListener(() => {
				doAutoLoad = true;
				loadButton.onClick.Invoke();
			});
		}

		[HarmonyPatch("PersistentLoadGame")]
		[HarmonyPrefix()]
		static void PersistentLoadGamePrefix(LoadGameDialog __instance) {
			KSP_AutoLoad.Log("LoadGameDialog:PersistentLoadGame prefix");
			try {
				isPersistent = true;
				CreateAutoLoadButton(__instance);
			} catch (Exception e) {
				KSP_AutoLoad.Log("Exception: {0}", e);
			}
		}

		[HarmonyPatch("LoadGame")]
		[HarmonyPrefix()]
		static void LoadGamePrefix(LoadGameDialog __instance) {
			KSP_AutoLoad.Log("LoadGameDialog:LoadGame prefix");
			try {
				isPersistent = false;
				CreateAutoLoadButton(__instance);
			} catch (Exception e) {
				KSP_AutoLoad.Log("Exception: {0}", e);
			}
		}
		
		[HarmonyPatch("OnSelectionChanged")]
		[HarmonyPostfix()]
		static void OnSelectionChangedPostfix(LoadGameDialog __instance, Button ___btnLoad) {
			KSP_AutoLoad.Log("LoadGameDialog:OnSelectionChanged postfix");
			try {
				autoLoadButton.interactable = ___btnLoad.interactable;
			} catch (Exception e) {
				KSP_AutoLoad.Log("Exception: {0}", e);
			}
		}

		[HarmonyPatch("ConfirmLoadGame")]
		[HarmonyPrefix()]
		static void ConfirmLoadGamePostfix(LoadGameDialog __instance, string ___selectedGame, string ___directory) {
			KSP_AutoLoad.Log("LoadGameDialog:ConfirmLoadGame prefix");
			try {
				if (!doAutoLoad) {
					if (isPersistent) {
						KSP_AutoLoad.SetAutoLoad("", "");
					}
					return;
				}
				string selectedGame = ___selectedGame;
				string directory = ___directory;
				if (isPersistent) {
					directory = selectedGame;
					selectedGame = "persistent";
				}
				KSP_AutoLoad.SetAutoLoad(directory, selectedGame);
			} catch (Exception e) {
				KSP_AutoLoad.Log("Exception: {0}", e);
			}
		}
	}
}
