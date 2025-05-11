﻿/*
 * Copyright 2024 Peter Han
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
using PeterHan.PLib.AVC;
using PeterHan.PLib.Core;
using PeterHan.PLib.Database;
using PeterHan.PLib.PatchManager;
using System.Collections.Generic;
using System.Reflection;
using PeterHan.PLib.Detours;
using PeterHan.PLib.UI;
using UnityEngine;

using SideScreenRef = DetailsScreen.SideScreenRef;

namespace PeterHan.WorkshopProfiles {
	/// <summary>
	/// Patches which will be applied via annotations for Workshop Profiles.
	/// </summary>
	public sealed class WorkshopProfilesPatches : KMod.UserMod2 {
		// TODO Remove when versions less than U52-640445 no longer need to be supported
		private static readonly IDetouredField<ChoreConsumerState, KMonoBehaviour> WORKER =
			PDetours.DetourFieldLazy<ChoreConsumerState, KMonoBehaviour>("worker");

		/// <summary>
		/// Building IDs that should not have workshop profiles.
		/// </summary>
		public static readonly ICollection<string> BLACKLIST_IDS = new List<string>() {
			LiquidPumpingStationConfig.ID
		};

		/// <summary>
		/// A precondition which checks the Workshop Profile list to see if a Duplicant can
		/// use the building.
		/// </summary>
		private static Chore.Precondition IS_ALLOWED;

		/// <summary>
		/// Adds a chore precondition to check if the Duplicant is allowed to use that
		/// building.
		/// </summary>
		/// <param name="chore">The chore to modify.</param>
		private static void AddProfilePrecondition(Chore chore) {
			var choreTypes = Db.Get().ChoreTypes;
			var type = chore.choreType;
			// Blacklist all fetch, empty storage, decon, repair, disinfect
			if (type != null && type != choreTypes.EmptyStorage && type != choreTypes.
					Deconstruct && type != choreTypes.Repair && type != choreTypes.Disinfect &&
					!type.Id.Contains("Fetch") && chore.target is Component cmp && cmp !=
					null && IS_ALLOWED.fn != null && cmp.TryGetComponent(
					out WorkshopProfile profiles))
				// Look for WP object
				chore.AddPrecondition(IS_ALLOWED, profiles);
		}

		/// <summary>
		/// Adds the workshop profile component to a building if it needs one. Only should be
		/// used on completed buildings, this mod does not add profiles to every build/
		/// deconstruct errand. Likewise, already assignable buildings are excluded.
		/// </summary>
		/// <param name="go">The building to modify.</param>
		/// <param name="options">The current mod options.</param>
		private static void AddWorkshopProfile(GameObject go, WorkshopProfilesOptions options)
		{
			// The pitcher pump should not have a profile as it only can be used for fetch
			// errands, which are not modified by Workshop Profiles
			if (go.TryGetComponent(out BuildingComplete bc) && !go.TryGetComponent(
					out Assignable _)) {
				string prefabTag = bc.prefabid.PrefabTag.Name;
				if ((go.TryGetComponent(out ComplexFabricator cf) && cf.duplicantOperated) ||
						(bc.isManuallyOperated && !BLACKLIST_IDS.Contains(prefabTag)) ||
						options.AddToBuildings.Contains(prefabTag))
					// Since we do not affect Supply errands, ignore automated buildings like
					// the kiln; BuildingComplete.isManuallyOperated covers grills, research
					// stations, composts...
					go.AddComponent<WorkshopProfile>();
			}
		}

		/// <summary>
		/// Checks to see if a Duplicant can do a chore based on workshop profiles.
		/// </summary>
		/// <param name="context">The context including the Duplicant attempting the chore.</param>
		/// <param name="data">The workshop profile list.</param>
		/// <returns>true if the Duplicant can use the building, or false otherwise.</returns>
		private static bool IsAllowed(ref Chore.Precondition.Context context, object data) {
			bool allow = true;
			var state = context.consumerState;
			KPrefabID prefabID;
			if (data is WorkshopProfile profile && profile != null && state != null &&
					WORKER.Get(state) != null &&
					(prefabID = context.consumerState.prefabid) != null)
				allow = profile.IsAllowed(prefabID.InstanceID);
			return allow;
		}

		[PLibMethod(RunAt.AfterDbInit)]
		internal static void OnDbInit() {
			IS_ALLOWED = new Chore.Precondition {
				id = "IsProfileAllowed",
				description = WorkshopProfilesStrings.DUPLICANTS.CHORES.PRECONDITIONS.
					ALLOWED_BY_PROFILE,
				fn = IsAllowed
			};
		}

		public override void OnLoad(Harmony harmony) {
			base.OnLoad(harmony);
			PUtil.InitLibrary();
			new PPatchManager(harmony).RegisterPatchClass(typeof(WorkshopProfilesPatches));
			LocString.CreateLocStringKeys(typeof(WorkshopProfilesStrings.DUPLICANTS));
			new PLocalization().Register();
			// This can live for the whole game and never needs to be removed
			Components.LiveMinionIdentities.OnRemove += OnRemoveDuplicant;
			new PVersionCheck().Register(this, new SteamVersionChecker());
		}

		/// <summary>
		/// Called when a Duplicant is removed, either by death or things like sandbox delete.
		/// </summary>
		/// <param name="dupe">The Duplicant that was removed.</param>
		private static void OnRemoveDuplicant(MinionIdentity dupe) {
			if (dupe != null && dupe.TryGetComponent(out KPrefabID prefabID)) {
				WorkshopProfile.CleanupCmps();
				foreach (var cmp in WorkshopProfile.Cmps)
					if (cmp != null)
						cmp.RemoveDuplicant(prefabID.InstanceID);
			}
		}

		/// <summary>
		/// Applied to Chore to add workshop profile conditions to all Work chores.
		/// </summary>
		[HarmonyPatch]
		public static class Chore_AddPrecondition_Patch {
			internal static MethodBase TargetMethod() {
				// TODO Remove when versions less than U52-622222 no longer need to be supported
				var type = PPatchTools.GetTypeSafe(nameof(StandardChoreBase)) ?? typeof(Chore);
				return type.GetMethodSafe(nameof(Chore.AddPrecondition), false, PPatchTools.
					AnyArguments);
			}

			/// <summary>
			/// Applied after AddPrecondition runs.
			/// </summary>
			internal static void Postfix(Chore __instance, Chore.Precondition precondition,
					object data) {
				// If the chore is of type Work
				if (precondition.id == ChorePreconditions.instance.IsScheduledTime.id &&
						data == Db.Get().ScheduleBlockTypes.Work)
					AddProfilePrecondition(__instance);
			}
		}

		/// <summary>
		/// Applied to DetailsScreen to add a side screen for workshop profiles.
		/// </summary>
		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public static class DetailsScreen_OnPrefabInit_Patch {
			/// <summary>
			/// Applied after OnPrefabInit runs.
			/// </summary>
			internal static void Postfix(List<SideScreenRef> ___sideScreens,
					DetailsScreen __instance) {
				WorkshopProfileSideScreen.AddSideScreen(___sideScreens,
					PUIUtils.GetSideScreenContent(__instance));
			}
		}

		/// <summary>
		/// Applied to GeneratedBuildings to add workshop profiles to buildings matching
		/// specific patterns.
		/// </summary>
		[HarmonyPatch(typeof(GeneratedBuildings), nameof(GeneratedBuildings.
			LoadGeneratedBuildings))]
		public static class GeneratedBuildings_LoadGeneratedBuildings_Patch {
			/// <summary>
			/// Applied after LoadGeneratedBuildings runs.
			/// </summary>
			internal static void Postfix() {
				GameObject bc;
				var options = WorkshopProfilesOptions.Instance;
				options.PopulateDefaults();
				foreach (var building in Assets.BuildingDefs)
					if (building != null && (bc = building.BuildingComplete) != null)
						AddWorkshopProfile(bc, options);
			}
		}
	}
}
