/*
 * Created by SharpDevelop.
 * User: Dave
 * Date: 4/3/2024
 * Time: 11:43 AM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using HarmonyLib;
using KSP.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace KSP_AutoLoad {
	/// <summary>
	/// Description of KSP_AutoLoad.
	/// </summary>
	[KSPAddon(KSPAddon.Startup.Instantly, true)]
	public class KSP_AutoLoad : MonoBehaviour {
		private static string cfgFile = KSPUtil.ApplicationRootPath + "saves/AutoLoad.cfg";
		private bool didAutoLoad;

		public KSP_AutoLoad() {
			Log("Created");
		}

		void doAutoLoad() {
			if (HighLogic.LoadedScene == GameScenes.MAINMENU) {
				if (didAutoLoad) {
					return;
				}
				didAutoLoad = true;
				
				var config = ConfigNode.Load(cfgFile);
				if (config == null) {
					Log("{0} doesn't exist or is invalid", cfgFile);
					return;
				}
				string savegame = config.GetValue("savegame");
				string directory = config.GetValue("directory");
				if (String.IsNullOrEmpty(savegame) || String.IsNullOrEmpty(directory)) {
					Log("AutoLoad not configured");
					return;
				}
				Log("Trying to load {0}/{1}", directory, savegame);
				var gameSFS = GamePersistence.LoadSFSFile(savegame, directory);
				if (gameSFS == null) {
					ScreenMessages.PostScreenMessage(Localizer.Format("#autoLOC_485739", directory), 5f, ScreenMessageStyle.UPPER_LEFT);
				}
				KSPUpgradePipeline.Process(gameSFS, directory, SaveUpgradePipeline.LoadContext.SFS,
					(ConfigNode node) => {
						HighLogic.CurrentGame = GamePersistence.LoadGameCfg(node, directory, true, false);
						if (HighLogic.CurrentGame == null) {
							return;
						}
						GamePersistence.UpdateScenarioModules(HighLogic.CurrentGame);
						GamePersistence.SaveGame(HighLogic.CurrentGame, "persistent", directory, SaveMode.OVERWRITE);
						GameEvents.onGameStatePostLoad.Fire(node);
						HighLogic.SaveFolder = directory;
						HighLogic.CurrentGame.Start();
					},
					(KSPUpgradePipeline.UpgradeFailOption arg1, ConfigNode arg2) => {
						Log("UpgradePipeline failed for: {0}/{1}", directory, savegame);
					});
			}
		}

		private void Awake() {
			Log("Awake");
			DontDestroyOnLoad(this);
			var harmony = new Harmony("KSP_AutoLoad");
			harmony.PatchAll();
			DontDestroyOnLoad(this);
			GameEvents.onLevelWasLoadedGUIReady.Add((scene) => {
				if (scene == GameScenes.MAINMENU) {
					// Wait for background things to finish.
					// Avoiding NullReferenceException at KSP.UI.MessageSystemAppFrame.Reposition
					Invoke("doAutoLoad", 2);
				}
			});
		}

		public static void SetAutoLoad(string directory, string savegame) {
			var config = ConfigNode.Load(cfgFile);
			if (config == null) {
				config = new ConfigNode();
			}
			config.SetValue("directory", directory, true);
			config.SetValue("savegame", savegame, true);
			config.Save(cfgFile);
		}

		public static void Log(string message, params object[] parms) {
			Debug.Log(String.Format("KSP_AutoLoad: " + message, parms));
		}
	}
}
