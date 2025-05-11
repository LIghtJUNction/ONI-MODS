﻿/*
 * Copyright 2022 Peter Han
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all copies or
 * substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using HarmonyLib;
using System.Reflection;

namespace PeterHan.NoSplashScreen {
	/// <summary>
	/// Patches which will be applied via annotations for No Splash Screen.
	/// </summary>
	public class NoSplashScreenPatches : KMod.UserMod2 {
		public override void OnLoad(Harmony harmony) {
			var assembly = Assembly.GetExecutingAssembly();
			base.OnLoad(harmony);
			Debug.Log("Mod NoSplashScreen initialized, assembly version " + assembly.
				GetName()?.Version?.ToString() ?? "Unknown");
		}

		/// <summary>
		/// Applied to SplashMessageScreen.
		/// </summary>
		[HarmonyPatch(typeof(SplashMessageScreen), "OnPrefabInit")]
		public static class SplashMessageScreen_OnPrefabInit_Patch {
			/// <summary>
			/// Applied after OnPrefabInit runs.
			/// </summary>
			internal static void Postfix(SplashMessageScreen __instance) {
				var obj = __instance.gameObject;
				if (obj != null) {
					obj.SetActive(false);
					UnityEngine.Object.Destroy(obj);
				}
			}
		}
	}
}
