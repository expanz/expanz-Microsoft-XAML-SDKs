using System;
using Microsoft.Windows.Design.Metadata;
using Expanz.ThinRIA.Core.Silverlight.Design.Types;
using Expanz.ThinRIA.Core.Common.VisualStudio.Design;

namespace Expanz.ThinRIA.Core.Silverlight.VisualStudio.Design
{
    internal class SilverlightTypeResolver : IRegistrationTypeResolver
    {
        /// <summary>
        /// Takes a platform neutral TypeIdentifier and returns back a platform specific Type.
        ///  For each of the types you are providing a design experience for, you must add an entry to this method.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Type GetPlatformType(TypeIdentifier id)
        {
            //System.IO.File.AppendAllText(@"C:\Temp\DesignDebug.txt", "GetPlatformType: " + id.Name);

            switch (id.Name)
            {
                case ExpanzControlsEnum.ActivityPanelEx:
                    return SilverlightTypes.ActivityPanelExControlType;
                case ExpanzControlsEnum.ApplicationNavigationMenu:
                    return SilverlightTypes.ApplicationNavigationMenuControlType;;
                case ExpanzControlsEnum.BusyIndicatorEx:
                    return SilverlightTypes.BusyIndicatorExControlType;
                case ExpanzControlsEnum.ButtonEx:
                    return SilverlightTypes.ButtonExControlType;
                case ExpanzControlsEnum.CheckBoxEx:
                    return SilverlightTypes.CheckBoxExControlType;
                case ExpanzControlsEnum.ChildWindowEx:
                    return SilverlightTypes.ChildWindowExControlType;
                case ExpanzControlsEnum.ClientMessageWindow:
                    return SilverlightTypes.ClientMessageWindowControlType;
                case ExpanzControlsEnum.CloseButton:
                    return SilverlightTypes.CloseButtonControlType;
                case ExpanzControlsEnum.ComboBoxEx:
                    return SilverlightTypes.ComboBoxExControlType;
                case ExpanzControlsEnum.ComboBoxItemEx:
                    return SilverlightTypes.ComboBoxItemExControlType;
                case ExpanzControlsEnum.ContentContainer:
                    return SilverlightTypes.ContentContainerControlType;
                case ExpanzControlsEnum.ContextMenuButton:
                    return SilverlightTypes.ContextMenuButtonControlType;
                case ExpanzControlsEnum.DataGridEx:
                    return SilverlightTypes.DataGridExControlType;
                case ExpanzControlsEnum.DatePickerEx:
                    return SilverlightTypes.DatePickerExControlType;
                case ExpanzControlsEnum.HelperButton:
                    return SilverlightTypes.HelperButtonControlType;
                case ExpanzControlsEnum.HyperlinkButtonEx:
                    return SilverlightTypes.HyperlinkButtonExControlType;
                case ExpanzControlsEnum.LabelEx:
                    return SilverlightTypes.LabelExControlType;
                case ExpanzControlsEnum.LaunchActivityButton:
                    return SilverlightTypes.LaunchActivityButtonControlType;
                case ExpanzControlsEnum.LaunchActivityHyperlinkButton:
                    return SilverlightTypes.LaunchActivityHyperlinkButtonControlType;
                case ExpanzControlsEnum.ListBoxEx:
                    return SilverlightTypes.ListBoxExControlType;
                case ExpanzControlsEnum.ListBoxItemEx:
                    return SilverlightTypes.ListBoxItemExControlType;
                case ExpanzControlsEnum.LoginForm:
                    return SilverlightTypes.LoginFormControlType;
                case ExpanzControlsEnum.MessagePanel:
                    return SilverlightTypes.MessagePanelControlType;
                case ExpanzControlsEnum.PageEx:
                    return SilverlightTypes.PageExControlType;
                case ExpanzControlsEnum.PropertiesButton:
                    return SilverlightTypes.PropertiesButtonControlType;
                case ExpanzControlsEnum.RadioButtonColumn:
                    return SilverlightTypes.RadioButtonColumnControlType;
                //case ExpanzControlsEnum.TextBlockEx:
                //    return SilverlightTypes.TextBlockExControlType;
                case ExpanzControlsEnum.TextBoxEx:
                    return SilverlightTypes.TextBoxExControlType;
                //case ExpanzControlsEnum.TrafficDebugButton:
                //    return SilverlightTypes.TrafficDebugButtonwControlType;
                case ExpanzControlsEnum.TrafficDebuggingWindow:
                    return SilverlightTypes.TrafficDebuggingWindowControlType;
                case ExpanzControlsEnum.UserControlEx:
                    return SilverlightTypes.UserControlExControlType;
                //case ExpanzControlsEnum.ValidationSummaryEx:
                    //return SilverlightTypes.ValidationSummaryExControlType;
            }

            throw new ArgumentOutOfRangeException("id.Name", id.Name, "Control has no corresponding enumeration");
        }
    }
}
