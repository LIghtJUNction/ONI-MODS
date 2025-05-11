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

using Klei.AI;
using PeterHan.PLib.Actions;
using PeterHan.PLib.Core;
using UnityEngine;

namespace PeterHan.Resculpt {
	/// <summary>
	/// A behavior for Artable items that adds a Resculpt button in the UI.
	/// </summary>
	[SkipSaveFileSerialization]
	public sealed class Resculptable : KMonoBehaviour {
		/// <summary>
		/// Private string declared in Artable that is never changed
		/// </summary>
		private const string DEFAULT_ARTWORK_ID = "Default";

		/// <summary>
		/// The icon sprite shown on the repaint or resculpt button.
		/// </summary>
		[SerializeField]
		public string ButtonIcon;

		/// <summary>
		/// The text shown on the repaint or resculpt button. If null, the Resculpt text is
		/// used.
		/// </summary>
		[SerializeField]
		public string ButtonText;

#pragma warning disable IDE0044 // Add readonly modifier
#pragma warning disable CS0649
		[MyCmpReq]
		private Artable artable;

		[MyCmpReq]
		private KPrefabID prefabID;
#pragma warning restore CS0649
#pragma warning restore IDE0044 // Add readonly modifier

		protected override void OnCleanUp() {
			Unsubscribe((int)GameHashes.RefreshUserMenu);
			base.OnCleanUp();
		}

		protected override void OnPrefabInit() {
			base.OnPrefabInit();
			Subscribe((int)GameHashes.RefreshUserMenu, OnRefreshUserMenu);
		}

		/// <summary>
		/// Triggered when the user requests a resculpt of the decor item.
		/// </summary>
		private void OnResculpt() {
			var stages = Db.Get().ArtableStages;
			string currentStatus;
			Database.ArtableStage currentStage;
			if (artable != null && (currentStatus = artable.CurrentStage) !=
					DEFAULT_ARTWORK_ID && (currentStage = stages.TryGet(currentStatus)) !=
					null) {
				var eligible = ListPool<string, Resculptable>.Allocate();
				var possible = stages.GetPrefabStages(prefabID.PrefabTag);
				int currentIndex = 0;
				try {
					var allowedSkill = currentStage.statusItem;
					// Populate with valid stages
					foreach (var stage in possible)
						if (stage.statusItem == allowedSkill) {
							// Search for the current one if possible
							if (stage.id == currentStatus)
								currentIndex = eligible.Count;
							eligible.Add(stage.id);
						}
					int n = eligible.Count;
					if (n > 1) {
						var attrs = this.GetAttributes().Get(Db.Get().BuildingAttributes.Decor);
						// Remove the decor bonus (SetStage adds it back)
						attrs.Modifiers.RemoveAll(modifier => modifier.Description ==
							"Art Quality");
						// Next entry
						artable.SetStage(eligible[(currentIndex + 1) % n], true);
					}
				} finally {
					eligible.Recycle();
				}
			}
		}

		/// <summary>
		/// Triggered when the user requests a rotate of the decor item.
		/// </summary>
		private void OnRotateClicked() {
			var rotatable = gameObject.GetComponent<Rotatable>();
			if (rotatable != null) {
				rotatable.Rotate();

				// Buildings with even width values jump one tile when rotating and must be moved back
				var building = gameObject.GetComponentSafe<Building>()?.Def;
				if (building != null && building.WidthInCells % 2 == 0)
					transform.position += rotatable.GetOrientation() != Orientation.Neutral ?
						Vector3.right : Vector3.left;
			}
		}

		/// <summary>
		/// Called when the info screen for the decor item is refreshed.
		/// </summary>
		private void OnRefreshUserMenu(object _) {
			var um = Game.Instance?.userMenu;
			if (artable != null && artable.CurrentStage != DEFAULT_ARTWORK_ID && um != null) {
				string text = ButtonText, icon = ButtonIcon;
				// Set default name if not set
				if (string.IsNullOrEmpty(text))
					text = ResculptStrings.RESCULPT_BUTTON;
				if (string.IsNullOrEmpty(icon))
					icon = ResculptStrings.RESCULPT_SPRITE;
				var button = new KIconButtonMenu.ButtonInfo(icon, text, OnResculpt,
					PAction.MaxAction, null, null, null, ResculptStrings.RESCULPT_TOOLTIP);
				um.AddButton(gameObject, button);

				if (gameObject.GetComponent<Rotatable>() != null) {
					var rotationButton = new KIconButtonMenu.ButtonInfo(ResculptStrings.
						ROTATE_SPRITE, ResculptStrings.ROTATE_BUTTON, OnRotateClicked,
						Action.BuildMenuKeyO, tooltipText: ResculptStrings.ROTATE_TOOLTIP);
					um.AddButton(gameObject, rotationButton);
				}
			}
		}
	}
}
