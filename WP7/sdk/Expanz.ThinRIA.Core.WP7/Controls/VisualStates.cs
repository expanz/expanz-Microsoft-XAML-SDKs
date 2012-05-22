﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Diagnostics;
using System.Windows;

namespace Expanz.ThinRIA.Controls
{
    internal static class VisualStates
    {
        #region GroupValidation

        /// <summary>
        /// VSM Group for validation, containing focus details.  These details are not orthogonal and thus were combined.   See TextBox validation for precedence.
        /// </summary>
        public const string GroupValidation = "ValidationStates";

        /// <summary>
        /// VSM State for Valid
        /// </summary>
        public const string StateValid = "Valid";

        /// <summary>
        /// VSM STate for Invalid
        /// </summary>
        public const string StateInvalid = "Invalid";

        /// <summary>
        /// VSM State for Valid and Focused (DescriptionViewer specific)
        /// </summary>
        public const string StateValidFocused = "ValidFocused";

        /// <summary>
        /// VSM State for Valid and Unfocused (DescriptionViewer specific)
        /// </summary>
        public const string StateValidUnfocused = "ValidUnfocused";

        /// <summary>
        /// VSM State for Invalid and Focused (DescriptionViewer specific)
        /// </summary>
        public const string StateInvalidFocused = "InvalidFocused";

        /// <summary>
        /// VSM State for Invalid and Focused (DescriptionViewer specific)
        /// </summary>
        public const string StateInvalidUnfocused = "InvalidUnfocused";

        /// <summary>
        /// VSM State for no errors (ValidationSummary specific)
        /// </summary>
        public const string StateEmpty = "Empty";

        /// <summary>
        /// VSM State for containing errors (ValidationSummary specific)
        /// </summary>
        public const string StateHasErrors = "HasErrors";

        /// <summary>
        /// RowInvalid state
        /// </summary>
        public const string StateRowInvalid = "RowInvalid";

        /// <summary>
        /// RowValid state
        /// </summary>
        public const string StateRowValid = "RowValid";

        #endregion GroupValidation

        #region GroupCommon

        /// <summary>
        /// VSM Group for common states, such as Normal or Disabled
        /// </summary>
        public const string GroupCommon = "CommonStates";

        /// <summary>
        /// VSM state for Normal (enabled)
        /// </summary>
        public const string StateNormal = "Normal";

        /// <summary>
        /// VSM state for Disabled
        /// </summary>
        public const string StateDisabled = "Disabled";

        /// <summary>
        /// MouseOver state
        /// </summary>
        public const string StateMouseOver = "MouseOver";

        /// <summary>
        /// Pressed state
        /// </summary>
        public const string StatePressed = "Pressed";

        #endregion GroupCommon

        #region GroupRequired

        /// <summary>
        /// VSM group for required states
        /// </summary>
        public const string GroupRequired = "RequiredStates";

        /// <summary>
        /// VSM state for not required
        /// </summary>
        public const string StateNotRequired = "NotRequired";

        /// <summary>
        /// VSM state for required
        /// </summary>
        public const string StateRequired = "Required";

        #endregion GroupRequired

        #region GroupDescription

        /// <summary>
        /// VSM group for description states
        /// </summary>
        public const string GroupDescription = "DescriptionStates";

        /// <summary>
        /// VSM state for no description defined
        /// </summary>
        public const string StateNoDescription = "NoDescription";

        /// <summary>
        /// VSM state for having a description defined
        /// </summary>
        public const string StateHasDescription = "HasDescription";

        #endregion GroupDescription

        #region GroupExpanded
        /// <summary>
        /// Expanded state
        /// </summary>
        public const string StateExpanded = "Expanded";

        /// <summary>
        /// Collapsed state
        /// </summary>
        public const string StateCollapsed = "Collapsed";
        #endregion GroupExpanded

        #region GroupFocus
        /// <summary>
        /// Unfocused state
        /// </summary>
        public const string StateUnfocused = "Unfocused";

        /// <summary>
        /// Focused state
        /// </summary>
        public const string StateFocused = "Focused";

        /// <summary>
        /// Focus state group
        /// </summary>
        public const string GroupFocus = "FocusStates";
        #endregion GroupFocus

        #region GroupSelection
        /// <summary>
        /// Selected state
        /// </summary>
        public const string StateSelected = "Selected";

        /// <summary>
        /// Unselected state
        /// </summary>
        public const string StateUnselected = "Unselected";

        /// <summary>
        /// Selection state group
        /// </summary>
        public const string GroupSelection = "SelectionStates";


        #endregion GroupSelection

        #region GroupActive
        /// <summary>
        /// Active state
        /// </summary>
        public const string StateActive = "Active";

        /// <summary>
        /// Inactive state
        /// </summary>
        public const string StateInactive = "Inactive";

        /// <summary>
        /// Active state group
        /// </summary>
        public const string GroupActive = "ActiveStates";
        #endregion GroupActive

        #region GroupCurrent
        /// <summary>
        /// Regular state
        /// </summary>
        public const string StateRegular = "Regular";

        /// <summary>
        /// Current state
        /// </summary>
        public const string StateCurrent = "Current";

        /// <summary>
        /// Current state group
        /// </summary>
        public const string GroupCurrent = "CurrentStates";
        #endregion GroupCurrent

        #region GroupInteraction
        /// <summary>
        /// Display state
        /// </summary>
        public const string StateDisplay = "Display";

        /// <summary>
        /// Editing state
        /// </summary>
        public const string StateEditing = "Editing";

        /// <summary>
        /// Interaction state group
        /// </summary>
        public const string GroupInteraction = "InteractionStates";
        #endregion GroupInteraction

        #region GroupAlternatingRow
        /// <summary>
        /// Regular Row state
        /// </summary>
        public const string StateRegularRow = "RegularRow";

        /// <summary>
        /// Alternating Row state
        /// </summary>
        public const string StateAlternatingRow = "AlternatingRow";

        /// <summary>
        /// Alternating Row state group
        /// </summary>
        public const string GroupAlternatingRow = "AlternatingRowStates";
        #endregion GroupAlternatingRow

        #region GroupSort
        /// <summary>
        /// Unsorted state
        /// </summary>
        public const string StateUnsorted = "Unsorted";

        /// <summary>
        /// Sort Ascending state
        /// </summary>
        public const string StateSortAscending = "SortAscending";

        /// <summary>
        /// Sort Descending state
        /// </summary>
        public const string StateSortDescending = "SortDescending";

        /// <summary>
        /// Sort state group
        /// </summary>
        public const string GroupSort = "SortStates";
        #endregion GroupSort


        /// <summary>
        /// Use VisualStateManager to change the visual state of the control.
        /// </summary>
        /// <param name="control">
        /// Control whose visual state is being changed.
        /// </param>
        /// <param name="useTransitions">
        /// true to use transitions when updating the visual state, false to
        /// snap directly to the new visual state.
        /// </param>
        /// <param name="stateNames">
        /// Ordered list of state names and fallback states to transition into.
        /// Only the first state to be found will be used.
        /// </param>
        public static void GoToState(System.Windows.Controls.Control control, bool useTransitions, params string[] stateNames)
        {
            Debug.Assert(control != null);

            if (stateNames == null)
            {
                return;
            }

            foreach (string name in stateNames)
            {
                if (VisualStateManager.GoToState(control, name, useTransitions))
                {
                    break;
                }
            }
        }
    
    }
}
