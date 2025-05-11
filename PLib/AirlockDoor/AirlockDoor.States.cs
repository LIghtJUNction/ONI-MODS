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

using PeterHan.PLib.Core;
using System;
using System.Reflection;
using UnityEngine;
using PeterHan.PLib.Detours;

namespace PeterHan.AirlockDoor {
	/// <summary>
	/// A version of Door that never permits gas and liquids to pass unless set to open.
	/// </summary>
	public sealed partial class AirlockDoor {
		public sealed class States : GameStateMachine<States, Instance, AirlockDoor> {
			/// <summary>
			/// The base time to vacuum the airlock at maximum pressure.
			/// </summary>
			private const float MAX_VACUUM_TIME = 4.0f;

			/// <summary>
			/// The base time to vacuum the airlock at minimum pressure.
			/// </summary>
			private const float MIN_VACUUM_TIME = 0.4f;

			/// <summary>
			/// The pressure in kg at which the vacuum time maxes out.
			/// </summary>
			private const float PRESSURE_THRESHOLD = 3.0f;

			/// <summary>
			/// Calculates the energy required to vacuum the airlock. The energy requirement
			/// can be reduced by as much as 50% if less gas must be removed.
			/// </summary>
			/// <param name="smi">The door's state machine instance.</param>
			/// <returns>The energy to use in the vacuum state.</returns>
			internal static float CalculateVacuumEnergy(AirlockDoor.Instance smi) {
				return Mathf.Lerp(0.5f, 1.0f, smi.AveragePressure /
					PRESSURE_THRESHOLD) * smi.master.EnergyPerUse;
			}

			/// <summary>
			/// Calculates the time in in-game seconds required to vacuum the airlock.
			/// </summary>
			/// <param name="smi">The door's state machine instance.</param>
			/// <returns>The time to spend in the vacuum state.</returns>
			private static float CalculateVacuumTime(AirlockDoor.Instance smi) {
				return Mathf.Lerp(MIN_VACUUM_TIME, MAX_VACUUM_TIME, smi.AveragePressure /
					PRESSURE_THRESHOLD);
			}

			/// <summary>
			/// Sets up common parameters for door closing states.
			/// </summary>
			/// <param name="suffix">The anim name suffix for this side.</param>
			/// <param name="state">The state to configure.</param>
			/// <returns>The configured state.</returns>
			private static State ConfigureClosingState(string suffix, State state) {
				return state.ToggleTag(GameTags.Transition).
					ToggleLoopingSound("Airlock Closes", (smi) => smi.master.doorClosingSound,
					(smi) => !string.IsNullOrEmpty(smi.master.doorClosingSound)).
					Update((smi, dt) => {
						string sound = smi.master.doorClosingSound;
						if (sound != null)
							smi.master.loopingSounds.UpdateSecondParameter(sound,
								SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().
								GetPositionPercent());
					}, UpdateRate.SIM_33ms).
					PlayAnim("close" + suffix);
			}

			/// <summary>
			/// Sets up common parameters for door opening states.
			/// </summary>
			/// <param name="suffix">The anim name suffix for this side.</param>
			/// <param name="state">The state to configure.</param>
			/// <returns>The configured state.</returns>
			private static State ConfigureOpeningState(string suffix, State state) {
				return state.ToggleTag(GameTags.Transition).
					ToggleLoopingSound("Airlock Opens", (smi) => smi.master.doorOpeningSound,
					(smi) => !string.IsNullOrEmpty(smi.master.doorOpeningSound)).
					Update((smi, dt) => {
						string sound = smi.master.doorOpeningSound;
						if (sound != null)
							smi.master.loopingSounds.UpdateSecondParameter(sound,
								SOUND_PROGRESS_PARAMETER, smi.Get<KBatchedAnimController>().
								GetPositionPercent());
					}, UpdateRate.SIM_33ms).
					PlayAnim("open" + suffix);
			}

			/// <summary>
			/// Updates an airlock door's world state.
			/// </summary>
			/// <param name="smi">The door's state machine instance.</param>
			private static void UpdateWorldState(AirlockDoor.Instance smi) {
				smi.master.UpdateWorldState();
			}

			public override void InitializeStates(out BaseState default_state) {
				serializable = SerializeType.ParamsOnly;
				default_state = notFunctional;
				notFunctional.PlayAnim("off").
					Enter("UpdateWorldState", UpdateWorldState).
					ParamTransition(isLocked, locking, IsTrue).
					Transition(closed, (smi) => smi.master.IsUsable());
				// Start opening if waiting, lock if requested
				closed.PlayAnim("idle").
					EventTransition(GameHashes.FunctionalChanged, notFunctional, (smi) => !smi.master.IsUsable()).
					ParamTransition(waitEnterLeft, left.enter, IsTrue).
					ParamTransition(waitEnterRight, right.enter, IsTrue).
					// If someone teleports a dupe into the airlock...
					ParamTransition(waitExitLeft, left.exit, IsTrue).
					ParamTransition(waitExitRight, right.exit, IsTrue).
					ParamTransition(isLocked, locking, IsTrue).
					Enter("UpdateWorldState", UpdateWorldState);
				// The locked state displays the "no" icon on the door
				locking.PlayAnim("locked_pre").OnAnimQueueComplete(locked).
					Enter("UpdateWorldState", UpdateWorldState);
				locked.PlayAnim("locked").
					ParamTransition(isLocked, unlocking, IsFalse);
				unlocking.PlayAnim("locked_pst").OnAnimQueueComplete(notFunctional);
				left.ConfigureStates("_left", isTraversingLeft, vacuum, new CellOffset(-2, 0));
				right.ConfigureStates("_right", isTraversingRight, vacuum, new CellOffset(2, 0));
				// Clear contaminants, wait for anim, and check
				vacuum.PlayAnim("vacuum", KAnim.PlayMode.Loop).
					ScheduleGoTo(CalculateVacuumTime, vacuum_check).
					Enter("ClearContaminants", (smi) => {
						smi.master.UpdateWorldState();
						smi.ClearContaminants();
					}).Exit("ClearContaminants", (smi) => {
						smi.ClearContaminants();
						Game.Instance.userMenu.Refresh(smi.master.gameObject);
					});
				vacuum_check.ParamTransition(waitExitLeft, left.exit, IsTrue).
					ParamTransition(waitExitRight, right.exit, IsTrue).
					Transition(closed, (smi) => !waitExitLeft.Get(smi) && !waitExitRight.Get(smi));
			}

			/// <summary>
			/// Not operational / broken down / no charge. Door is considered closed.
			/// </summary>
			public State notFunctional;

			/// <summary>
			/// Both sides closed in automatic mode.
			/// </summary>
			public State closed;

			/// <summary>
			/// Both sides closed and cosmetic vacuuming animation is playing.
			/// </summary>
			public State vacuum;

			/// <summary>
			/// Both sides closed and cosmetic vacuuming animation is complete. Only for one
			/// instant, transitions to the correct state.
			/// </summary>
			public State vacuum_check;

			/// <summary>
			/// Both sides closed and lock process started.
			/// </summary>
			public State locking;

			/// <summary>
			/// Both sides closed and locked to access.
			/// </summary>
			public State locked;

			/// <summary>
			/// Both sides closed and unlock process started.
			/// </summary>
			public State unlocking;

			/// <summary>
			/// States for the left door.
			/// </summary>
			public SingleSideStates left;

			/// <summary>
			/// States for the right door.
			/// </summary>
			public SingleSideStates right;

			/// <summary>
			/// True if a Duplicant is waiting to enter from the left.
			/// </summary>
			public BoolParameter waitEnterLeft;

			/// <summary>
			/// True if a Duplicant is waiting to enter from the right.
			/// </summary>
			public BoolParameter waitEnterRight;

			/// <summary>
			/// True if a Duplicant is waiting to exit to the left.
			/// </summary>
			public BoolParameter waitExitLeft;

			/// <summary>
			/// True if a Duplicant is waiting to exit to the right.
			/// </summary>
			public BoolParameter waitExitRight;

			/// <summary>
			/// True if the door is currently locked by automation or toggle.
			/// No Duplicants can pass if locked.
			/// </summary>
			public BoolParameter isLocked;

			/// <summary>
			/// True if a Duplicant is traversing the left door.
			/// </summary>
			public BoolParameter isTraversingLeft;

			/// <summary>
			/// True if a Duplicant is traversing the right door.
			/// </summary>
			public BoolParameter isTraversingRight;

			/// <summary>
			/// States with one door locked and the other door operating.
			/// </summary>
			public sealed class SingleSideStates : State {
				/// <summary>
				/// How long to wait before closing the door.
				/// </summary>
				public const float EXIT_DELAY = 0.5f;

				/// <summary>
				/// Opening pod bay door for entry.
				/// </summary>
				public State enter;

				/// <summary>
				/// Open and waiting for Duplicants to pass.
				/// </summary>
				public State waitEnter;

				/// <summary>
				/// Open and waiting to close, 0.5s with no Duplicant passing.
				/// </summary>
				public State waitEnterClose;

				/// <summary>
				/// Closed and close animation playing.
				/// </summary>
				public State closing;

				/// <summary>
				/// Opening door for exit.
				/// </summary>
				public State exit;

				/// <summary>
				/// Open and waiting for Duplicants to pass.
				/// </summary>
				public State waitExit;

				/// <summary>
				/// Open and waiting to close, 0.5s with no Duplicant passing.
				/// </summary>
				public State waitExitClose;

				/// <summary>
				/// Closed and close animation playing.
				/// </summary>
				public State clearing;

				/// <summary>
				/// Initializes all states for one side.
				/// </summary>
				/// <param name="suffix">The anim name suffix for this side.</param>
				/// <param name="isTraversing">The parameter controlling if a Duplicant is traversing this side.</param>
				/// <param name="vacuum">The state to go to when finished.</param>
				/// <param name="offset">The cell offset to sample for pressure averaging.</param>
				internal void ConfigureStates(string suffix, BoolParameter isTraversing,
						State vacuum, CellOffset offset) {
					string open = "static" + suffix;

					ConfigureOpeningState(suffix, enter).
						Enter("RemoveEnergy", (smi) => {
							smi.WithdrawEnergy();
							Game.Instance.userMenu.Refresh(smi.master.gameObject);
						}).OnAnimQueueComplete(waitEnter).
						Exit((smi) => smi.ResetPressure());
					waitEnter.PlayAnim(open).
						Enter("UpdateWorldState", (smi) => {
							smi.master.UpdateWorldState();
							smi.CheckDuplicantStatus();
						}).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						ParamTransition(isTraversing, waitEnterClose, IsFalse);
					waitEnterClose.PlayAnim(open).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						ScheduleGoTo(EXIT_DELAY, closing).
						ParamTransition(isTraversing, waitEnter, IsTrue);
					ConfigureClosingState(suffix, closing).
						Enter("UpdateWorldState", UpdateWorldState).
						ParamTransition(isTraversing, waitEnter, IsTrue).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						OnAnimQueueComplete(vacuum);
					ConfigureOpeningState(suffix, exit).
						Exit((smi) => smi.ResetPressure()).
						OnAnimQueueComplete(waitExit);
					waitExit.PlayAnim(open).
						Enter("UpdateWorldState", (smi) => {
							smi.master.UpdateWorldState();
							smi.CheckDuplicantStatus();
						}).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						ParamTransition(isTraversing, waitExitClose, IsFalse);
					waitExitClose.PlayAnim(open).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						ScheduleGoTo(EXIT_DELAY, closing).
						ParamTransition(isTraversing, waitExit, IsTrue);
					ConfigureClosingState(suffix, clearing).
						Enter("UpdateWorldState", UpdateWorldState).
						ParamTransition(isTraversing, waitExit, IsTrue).
						Update("CheckIsBlocked", (smi, _) => smi.CheckAndAverage(offset)).
						OnAnimQueueComplete(vacuum);
				}
			}
		}

		/// <summary>
		/// The instance parameters of this state machine.
		/// </summary>
		public sealed class Instance : States.GameInstance {
			private delegate void TryOffsetNew(Pickupable instance, CellOffset offset);

			private delegate void TryOffsetOld(Pickupable instance);

			private static readonly TryOffsetNew TRY_OFFSET_NEW;

			private static readonly TryOffsetOld TRY_OFFSET_OLD;

			// FIXME New parameter introduced in June Quality of Life update U47-560136
			static Instance() {
				TryOffsetNew tryNew;
				try {
					tryNew = typeof(Pickupable).Detour<TryOffsetNew>(nameof(Pickupable.
						TryToOffsetIfBuried));
					TRY_OFFSET_OLD = null;
				} catch (DetourException) {
					tryNew = null;
					TRY_OFFSET_OLD = typeof(Pickupable).Detour<TryOffsetOld>(nameof(Pickupable.
						TryToOffsetIfBuried));
				}
				TRY_OFFSET_NEW = tryNew;
			}

			/// <summary>
			/// The layer to check for Duplicants.
			/// </summary>
			private readonly int minionLayer;

			/// <summary>
			/// The layer to check for dropped items.
			/// </summary>
			private readonly int pickupableLayer;

			/// <summary>
			/// The number of samples taken of the tiles just outside the airlock.
			/// </summary>
			private int pressureSamples;

			/// <summary>
			/// The total liquid/gas pressure of the tiles just outside the airlock during
			/// the open stages.
			/// </summary>
			private float totalPressure;

			/// <summary>
			/// Gets the average pressure sampled while the airlock was open.
			/// </summary>
			public float AveragePressure => (pressureSamples <= 0) ? 0.0f : totalPressure /
				pressureSamples;

			public Instance(AirlockDoor door) : base(door) {
				minionLayer = (int)PGameUtils.GetObjectLayer(nameof(ObjectLayer.Minion),
					ObjectLayer.Minion);
				pickupableLayer = (int)PGameUtils.GetObjectLayer(nameof(ObjectLayer.
					Pickupables), ObjectLayer.Pickupables);
				pressureSamples = 0;
				totalPressure = 0.0f;
			}

			/// <summary>
			/// Updates the traversing parameter if a Duplicant is currently passing
			/// through the door and averages the pressure of the specified side.
			/// </summary>
			/// <param name="offset">The offset of the cell to sample.</param>
			internal void CheckAndAverage(CellOffset offset) {
				CheckDuplicantStatus();
				SamplePressure(Grid.OffsetCell(master.GetBaseCell(), offset));
			}

			/// <summary>
			/// Updates the traversing parameter if a Duplicant is currently passing
			/// through the door.
			/// </summary>
			public void CheckDuplicantStatus() {
				int baseCell = master.building.GetCell();
				sm.isTraversingLeft.Set(HasMinion(Grid.CellLeft(baseCell)) ||
					HasMinion(Grid.CellUpLeft(baseCell)), smi);
				sm.isTraversingRight.Set(HasMinion(Grid.CellRight(baseCell)) ||
					HasMinion(Grid.CellUpRight(baseCell)), smi);
			}

			/// <summary>
			/// Ejects other pickupables and unwanted elements from the door.
			/// </summary>
			internal void ClearContaminants() {
				int baseCell = master.building.GetCell();
				int cellFarLeft = Grid.OffsetCell(baseCell, -2, 0), cellFarRight = Grid.
					OffsetCell(baseCell, 2, 0);
				bool validLeft = Grid.IsValidCell(cellFarLeft), validRight = Grid.IsValidCell(
					cellFarRight);
				if (validLeft && !validRight)
					cellFarLeft = cellFarRight;
				else if (!validLeft && validRight)
					cellFarRight = cellFarLeft;
				if (validLeft || validRight) {
					// Middle
					EjectAll(baseCell, cellFarRight);
					EjectAll(Grid.CellAbove(baseCell), cellFarRight);
					// Left
					EjectAll(Grid.CellLeft(baseCell), cellFarLeft);
					EjectAll(Grid.CellUpLeft(baseCell), cellFarLeft);
					// Right
					EjectAll(Grid.CellRight(baseCell), cellFarRight);
					EjectAll(Grid.CellUpRight(baseCell), cellFarRight);
				}
			}

			/// <summary>
			/// Ejects an item towards the target cell.
			/// </summary>
			/// <param name="item">The item to eject.</param>
			/// <param name="newCell">The location to move the items.</param>
			private void Eject(GameObject item, int newCell) {
				var position = Grid.CellToPosCCC(newCell, Grid.SceneLayer.Move);
				// Adjust for material's bounding box
				if (item.TryGetComponent(out KCollider2D collider))
					position.y += item.transform.GetPosition().y - collider.
						bounds.min.y;
				item.transform.SetPosition(position);
				// Start falling if pushed off the edge
				if (GameComps.Fallers.Has(item))
					GameComps.Fallers.Remove(item);
				GameComps.Fallers.Add(item, Vector2.zero);
				if (item.TryGetComponent(out Pickupable pickupable)) {
					if (TRY_OFFSET_NEW != null)
						TRY_OFFSET_NEW(pickupable, CellOffset.none);
					else
						TRY_OFFSET_OLD(pickupable);
				}
			}

			/// <summary>
			/// Ejects all dropped items in the specified cell to the specified new cell.
			/// </summary>
			/// <param name="cell">The cell to check for items.</param>
			/// <param name="newCell">The location to move the items.</param>
			private void EjectAll(int cell, int newCell) {
				GameObject go;
				if (Grid.IsValidCell(cell) && (go = Grid.Objects[cell, pickupableLayer]) !=
						null && go.TryGetComponent(out Pickupable pickupable)) {
					var node = pickupable.objectLayerListItem;
					while (node != null) {
						var item = node.gameObject;
						node = node.nextItem;
						// Ignore living entities
						if (item != null && item.GetSMI<DeathMonitor.Instance>()?.IsDead() !=
								false)
							Eject(item, newCell);
					}
				}
			}

			/// <summary>
			/// Checks the cell for a Duplicant.
			/// </summary>
			/// <param name="cell">The cell to check.</param>
			/// <returns>Whether a Duplicant occupies this cell.</returns>
			private bool HasMinion(int cell) {
				return Grid.IsValidBuildingCell(cell) && Grid.Objects[cell, minionLayer] !=
					null;
			}

			/// <summary>
			/// Resets the pressure sample accumulator.
			/// </summary>
			internal void ResetPressure() {
				totalPressure = 0.0f;
				pressureSamples = 0;
			}

			/// <summary>
			/// Samples the specified cell and the cell above it, determining the total liquid
			/// and gas pressure and averaging it with the current total.
			/// </summary>
			/// <param name="cell">The cell to check.</param>
			private void SamplePressure(int cell) {
				if (Grid.IsValidCell(cell) && !Grid.Solid[cell]) {
					int above = Grid.CellAbove(cell);
					totalPressure += Grid.Mass[cell];
					pressureSamples++;
					if (Grid.IsValidCell(above) && !Grid.Solid[above]) {
						totalPressure += Grid.Mass[above];
						pressureSamples++;
					}
				}
			}

			/// <summary>
			/// Withdraws energy when the door opens to admit a Duplicant.
			/// </summary>
			internal void WithdrawEnergy() {
				// Door was closed and has started to open, withdraw energy
				master.energyAvailable = Math.Max(0.0f, master.energyAvailable -
					States.CalculateVacuumEnergy(smi));
				master.UpdateMeter();
			}
		}
	}
}
