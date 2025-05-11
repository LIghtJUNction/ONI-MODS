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

namespace PeterHan.SweepByType {
	/// <summary>
	/// Stores the strings used in the Sweep By Type tool.
	/// </summary>
	static class SweepByTypeStrings {
		// Icon name for the tool
		public const string TOOL_ICON_NAME = "filtered_clear";

		// Title of material select dialog
		public static LocString DIALOG_TITLE = "Select material to sweep";

		// Title of the category containing microchips, artifacts, and so forth
		public static LocString CATEGORY_MISCPICKUPABLE = "Other";

		// Tool name displayed in the hover card when dragging filtered (uses stock game
		// string for the default mode)
		public static LocString TOOL_NAME_FILTERED = "Filtered Sweep Tool";

		// Displayed in the tooltip action menu
		public static LocString TOOLTIP_FILTERED = "SWEEP ONLY SELECTED ITEMS";

		public const string DEFAULT_SWEEP_KEY = "SWEEPBYTYPE.ACTION.DEFAULTSWEEP";

		// Only used in the key binding menu to allow binding the original Sweep tool
		public static LocString DEFAULT_SWEEP_TITLE = "Default Sweep Tool";

		// Only displayed to differentiate Filtered Sweep from Sweep if the default tool is visible
		public static LocString TOOLBAR_TITLE_FILTERED = "Filtered Sweep";
	}
}
