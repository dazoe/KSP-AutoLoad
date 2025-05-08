// Created by Rider.
// User: Dave
// Date: 05/07/2025
// Time: 19:05:48

using System;
using HarmonyLib;

namespace KSP_AutoLoad.Patches {
	[HarmonyPatch(typeof(MainMenu))]
	static class MainMenuPatch {
		[HarmonyPatch("CreateAndStartGame", new Type[]{typeof(string), typeof(Game.Modes), typeof(GameScenes), typeof(EditorFacility)})]
		[HarmonyPrefix()]
		static void CreateAndStartGamePrefix() {
			// User is creating a new game, clear previous autoload
			KSP_AutoLoad.Log("MainMenu:CreateAndStartGame prefix");
			KSP_AutoLoad.SetAutoLoad("", "'");
		}
	}
}
