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

using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.MismatchedFinder {
	/// <summary>
	/// Conditionally adds a button to pipe networks to find segments that have a different
	/// material or conduit type than the rest of the network.
	/// </summary>
	[SkipSaveFileSerialization]
	public sealed class FindMismatchedPipe : KMonoBehaviour, IRefreshUserMenu {
		/// <summary>
		/// Handles user menu refresh events system-wide.
		/// </summary>
		private static readonly EventSystem.IntraObjectHandler<FindMismatchedPipe>
			ON_REFRESH_MENU = PGameUtils.CreateUserMenuHandler<FindMismatchedPipe>();

		/// <summary>
		/// Checks to see if the conduit does not match.
		/// </summary>
		/// <param name="current">The current conduit's element.</param>
		/// <param name="type">The current conduit's type.</param>
		/// <param name="otherConduit">The conduit that may not be matching.</param>
		/// <returns>true if the conduit does not match, or false otherwise.</returns>
		private static bool IsMismatched(PrimaryElement current, ConduitType type,
				FlowUtilityNetwork.IItem otherConduit) {
			var go = otherConduit.GameObject;
			return go != null && go.TryGetComponent(out PrimaryElement pe) && (pe.ElementID !=
				current.ElementID || otherConduit.ConduitType != type) && current != pe;
		}

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649
		[MyCmpGet]
		private Conduit conduit;

		[MyCmpGet]
		private PrimaryElement pe;
#pragma warning restore CS0649
#pragma warning restore IDE0044

		/// <summary>
		/// Gets the current network for this pipe.
		/// </summary>
		/// <returns>The connected network, or none if the wire is not connected.</returns>
		private FlowUtilityNetwork GetNetwork() {
			int cell = Grid.PosToCell(this);
			return (Grid.IsValidCell(cell) && conduit != null) ? conduit.GetNetworkManager().
				GetNetworkForCell(cell) as FlowUtilityNetwork : null;
		}

		protected override void OnCleanUp() {
			Unsubscribe((int)GameHashes.RefreshUserMenu, ON_REFRESH_MENU);
			base.OnCleanUp();
		}

		/// <summary>
		/// Called to select and center the mismatched pipes.
		/// </summary>
		private void OnFindMismatched() {
			var cnet = GetNetwork();
			if (pe != null && cnet != null) {
				var type = conduit.ConduitType;
				foreach (var otherConduit in cnet.conduits)
					if (IsMismatched(pe, type, otherConduit)) {
						var go = otherConduit.GameObject;
						if (go.TryGetComponent(out KSelectable select))
							SelectTool.Instance.SelectAndFocus(go.transform.position, select,
								Vector3.zero);
						break;
					}
			}
		}

		protected override void OnPrefabInit() {
			base.OnPrefabInit();
			Subscribe((int)GameHashes.RefreshUserMenu, ON_REFRESH_MENU);
		}

		/// <summary>
		/// Called when the info screen for the pipe is refreshed.
		/// </summary>
		public void OnRefreshUserMenu() {
			var cnet = GetNetwork();
			var gi = Game.Instance;
			if (pe != null && cnet != null && gi != null) {
				var type = conduit.ConduitType;
				bool mismatch = false;
				// Search by either conduit type or different element
				foreach (var otherConduit in cnet.conduits)
					if (IsMismatched(pe, type, otherConduit)) {
						mismatch = true;
						break;
					}
				if (mismatch) {
					gi.userMenu?.AddButton(gameObject, new KIconButtonMenu.ButtonInfo(
						"action_follow_cam", MismatchedFinderStrings.UI.USERMENUOPTIONS.
						FIND_PIPE, OnFindMismatched, PAction.MaxAction, null, null, null,
						MismatchedFinderStrings.UI.TOOLTIPS.FIND_PIPE));
				}
			}
		}
	}
}
