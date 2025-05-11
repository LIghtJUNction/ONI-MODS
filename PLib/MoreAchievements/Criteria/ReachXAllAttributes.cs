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

using Database;
using PeterHan.PLib.Core;
using System;

namespace PeterHan.MoreAchievements.Criteria {
	/// <summary>
	/// Requires a single Duplicant to reach the specified value in all attributes.
	/// </summary>
	public sealed class ReachXAllAttributes : ColonyAchievementRequirement, AchievementRequirementSerialization_Deprecated {
		/// <summary>
		/// The attribute value required.
		/// </summary>
		private float required;

		public ReachXAllAttributes(float required) {
			if (required.IsNaNOrInfinity())
				throw new ArgumentOutOfRangeException(nameof(required));
			this.required = Math.Max(0.0f, required);
		}

		public void Deserialize(IReader reader) {
			required = Math.Max(0.0f, reader.ReadSingle());
		}

		public override string GetProgress(bool complete) {
			return string.Format(AchievementStrings.JACKOFALLTRADES.PROGRESS, required);
		}

		public override bool Success() {
			var inst = AchievementStateComponent.Instance;
			return inst != null && inst.BestVarietyValue >= required;
		}
	}
}
