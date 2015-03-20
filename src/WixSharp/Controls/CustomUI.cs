using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Xml.Linq;

namespace WixSharp
{
    /// <summary>
    /// Defines the association between an MSI dialog control and the <see cref="T:WixSharp.PublishingInfo" />.
    /// </summary>
    public partial class PublishingInfo : WixEntity
    {
        /// <summary>
        /// Gets or sets the dialog Id.
        /// </summary>
        /// <value>
        /// The dialog Id.
        /// </value>
        public string Dialog { get; set; }

        /// <summary>
        /// Gets or sets the control Id.
        /// </summary>
        /// <value>
        /// The control Id.
        /// </value>
        public string Control { get; set; }

        /// <summary>
        /// The actions associated with the dialog control.
        /// </summary>
        public List<DialogAction> Actions = new List<DialogAction>();
    }

    /// <summary>
    /// Defines <see cref="T:WixSharp.DialogAction"/> for showing MSI dialog.
    /// </summary>
    public partial class ShowDialog : DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShowDialog"/> class.
        /// </summary>
        /// <param name="dialogName">Name of the dialog.</param>
        /// <param name="condition">The condition.</param>
        public ShowDialog(string dialogName, string condition = "1")
        {
            this.Name = ControlAction.NewDialog.ToString();
            this.Condition = condition;
            this.Value = dialogName;
        }
    }


    /// <summary>
    /// Defines <see cref="T:WixSharp.DialogAction"/> for executing MSI CustomAction ("DoAction").
    /// </summary>
    public partial class ExecueteCustomAction : DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExecueteCustomAction"/> class.
        /// </summary>
        /// <param name="actionName">Name of the action.</param>
        /// <param name="condition">The condition.</param>
        public ExecueteCustomAction(string actionName, string condition = "1")
        {
            this.Name = "DoAction";
            this.Condition = condition;
            this.Value = actionName;
        }
    }

    /// <summary>
    /// Defines "SetTargetPath" <see cref="T:WixSharp.DialogAction"/>.
    /// </summary>
    public partial class SetTargetPath : DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetTargetPath"/> class.
        /// </summary>
        /// <param name="propertyValue">The property value. Default value is "[WIXUI_INSTALLDIR]".</param>
        /// <param name="condition">The condition.</param>
        public SetTargetPath(string propertyValue = "[WIXUI_INSTALLDIR]", string condition = "1")
        {
            this.Name = ControlAction.SetTargetPath.ToString();
            this.Value = propertyValue;
            this.Condition = condition;
        }
    }

    /// <summary>
    /// Defines <see cref="T:WixSharp.DialogAction"/> for setting property.
    /// </summary>
    public partial class SetProperty : DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetProperty"/> class.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="propertyValue">The property value.</param>
        /// <param name="condition">The condition.</param>
        public SetProperty(string propertyName, string propertyValue, string condition = "1")
        {
            this.Property = propertyName;
            this.Value = propertyValue;
            this.Condition = condition;
        }
    }

    /// <summary>
    /// Defines custom UI control "Close Dialog" action.
    /// </summary>
    public partial class CloseDialog : DialogAction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:WixSharp.CloseDialog"/> class.
        /// </summary>
        /// <param name="returnValue">The return value.</param>
        /// <param name="condition">The condition.</param>
        public CloseDialog(string returnValue = "Return", string condition = "1")
        {
            this.Value = returnValue;
            this.Name = ControlAction.EndDialog.ToString();
            this.Condition = condition;
        }
    }

    /// <summary>
    /// Defines custom UI control generic action. 
    /// </summary>
    public partial class DialogAction : WixEntity
    {
#pragma warning disable 1591
        new public string Name;

        public string Property;

        public string Value;

        public string Condition = "1";

        public int? Order;
#pragma warning restore 1591
    }

    /// <summary>
    /// Simple class that defines custom UI (WiX <c>UI</c> element). This is a specialized version of <see cref="T:WixSharp.CustomUI"/> class
    /// designed to allow simple customization of the dialogs sequence without the introduction of any custom dialogs. 
    /// <example>The following is an example demonstrates how to skip <c>License Agreement</c> dialog, 
    /// which is otherwise displayed between <c>Welcome</c> and <c>InstallDir</c> dialogs.
    /// <code>
    /// project.CustomUI = new DialogSequence()
    ///                            .On(Dialogs.WelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.InstallDirDlg))
    ///                            .On(Dialogs.InstallDirDlg, Buttons.Back, new ShowDialog(Dialogs.WelcomeDlg)); 
    /// </code>
    /// </example>
    /// </summary>
    public class DialogSequence : CustomUI
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DialogSequence"/> class.
        /// </summary>
        public DialogSequence()
        {
            TextStyles.Clear();
            DialogRefs.Clear();
            Properties.Clear();
        }

        /// <summary>
        /// Defines the WiX Dialog UI control Action (event handler).
        /// <para>Note that all handlers will have <c>Order</c> field automatically assigned '5' 
        /// to ensure the overriding the default WiX event handlers</para>
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="control">The control.</param>
        /// <param name="handlers">The handlers.</param>
        /// <returns></returns>
        public DialogSequence On(string dialog, string control, params DialogAction[] handlers)
        {
            handlers.Where(h => !h.Order.HasValue)
                    .ForEach(h => h.Order = 5); //something high enough to have the highest priority at runtime

            base.On(dialog, control, handlers);
            return this;
        }
    }

    /// <summary>
    /// Defines custom UI (WiX <c>UI</c> element). This class is to be used to define the customization of the standard MSI UI. 
    /// The usual scenario of the customization is the injection of the custom <see cref="T:WixSharp.Dialog"/> 
    /// into the sequence of the standard dialogs. This can be accomplished by defining the custom dialog as  
    /// <see cref="T:WixSharp.WixForm"/>. Such a <see cref="T:WixSharp.WixForm"/> can be edited with the Visual Studio form designer. 
    /// <para>
    /// When <see cref="T:WixSharp.WixForm"/> is complete it can be converted into <see cref="T:WixSharp.Dialog"/> 
    /// with the <see cref="T:WixSharp.Dialog.ToWDialog"/>() call. And at compile time Wix# compiler converts 
    /// <see cref="T:WixSharp.Dialog"/> into the final WiX <c>UI</c> element XML definition.
    /// </para>
    /// <para>
    /// While it is possible to construct <see cref="T:WixSharp.CustomUI"/> instance manually it is preferred to use 
    /// Factory methods of  <see cref="T:WixSharp.CustomUIBuilder"/> (e.g. BuildPostLicenseDialogUI) for this.
    /// </para>
    /// <code>
    ///  project.UI = WUI.WixUI_Common;
    ///  project.CustomUI = CustomUIBuilder.BuildPostLicenseDialogUI(productActivationDialog,
    ///                                                              onNextActions: new DialogAction[]{
    ///                                                                                 new ExecueteCustomAction ("ValidateLicenceKey"),
    ///                                                                                 new ShowDialog(Dialogs.InstallDirDlg, "SERIALNUMBER_VALIDATED = \"TRUE\"")});
    /// </code>
    /// </summary>
    public partial class CustomUI : WixEntity
    {
        /// <summary>
        /// The collection of PropertyId vs. PropertyValue pairs to be defined as the <c>Property</c> WiX elements inside of the <c>UI</c> element.
        /// </summary>
        public Dictionary<string, string> Properties = new Dictionary<string, string>()
        {
            {"DefaultUIFont", "WixUI_Font_Normal"},
            {"WIXUI_INSTALLDIR", "TARGETDIR"},
            {"PIDTemplate", "####-####-####-####"},
            {"ARPNOMODIFY", "1"}
        };

        /// <summary>
        /// The collection of StyleId vs. Font pairs to be defined as the <c>TextStyle</c> WiX elements inside of the <c>UI</c> element.
        /// </summary>
        public Dictionary<string, Font> TextStyles = new Dictionary<string, Font>()
        {
            {"WixUI_Font_Normal", new Font("Tahoma", 8)},
            {"WixUI_Font_Bigger", new Font("Tahoma", 12)},
            {"WixUI_Font_Title", new Font("Tahoma", 9, FontStyle.Bold)}
        };

        /// <summary>
        /// The collection of dialog IDs to be defined as the <c>DialogRef</c> WiX elements inside of the <c>UI</c> element.
        /// By default it contains references to all MSI predefined dialogs.
        /// </summary>
        public List<string> DialogRefs = new List<string>(CommonDialogs.AllValues());

        /// <summary>
        /// The collection of <see cref="T:WixSharp.Dialog"/>s to be defined as the custom <c>Dialog</c> WiX elements inside of the <c>UI</c> element.
        /// </summary>
        public List<Dialog> CustomDialogs = new List<Dialog>();

        /// <summary>
        /// Defines UI sequence via collection of <see cref="T:WixSharp.PublishingInfo"/> items containing association between Dialog controls and MSI Dialog actions. 
        /// </summary>
        public List<PublishingInfo> UISequence = new List<PublishingInfo>();

        /// <summary>
        /// Defines the WiX Dialog UI control Action (event handler).
        /// <code>
        /// customUI.On(Dialogs.WelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.LicenseAgreementDlg));
        /// </code>
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="control">The control.</param>
        /// <param name="handlers">The handlers.</param>
        public void On(string dialog, string control, params DialogAction[] handlers)
        {
            var actionInfo = UISequence.Where(x => x.Dialog == dialog && x.Control == control)
                                       .Select(x => x)
                                       .FirstOrDefault();
            if (actionInfo == null)
            {
                UISequence.Add(actionInfo = new PublishingInfo
                    {
                        Dialog = dialog,
                        Control = control
                    });
            }

            actionInfo.Actions.AddRange(handlers);
        }

        /// <summary>
        /// Defines the <see cref="T:WixSharp.Dialog"/> UI control Action (event handler).
        /// <code>
        /// customUI.On(activationDialog, Buttons.Cancel, new CloseDialog("Exit"));
        /// </code>
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="control">The control.</param>
        /// <param name="handlers">The handlers.</param>
        public void On(Dialog dialog, string control, params DialogAction[] handlers)
        {
            On(dialog.Id, control, handlers);
        }

        /// <summary>
        /// Converts the <see cref="T:WixSharp.CustomUI"/> instance into WiX <see cref="T:System.Xml.Linq.XElement"/>.
        /// </summary>
        /// <returns><see cref="T:System.Xml.Linq.XElement"/> instance.</returns>
        public virtual XElement ToXElement()
        {
            var ui = new XElement("UI")
                         .AddAttributes(this.Attributes);

            foreach (string key in TextStyles.Keys)
            {
                var font = TextStyles[key];
                var style = ui.AddElement(new XElement("TextStyle",
                                              new XAttribute("Id", key),
                                              new XAttribute("FaceName", font.FontFamily.Name),
                                              new XAttribute("Size", font.Size),
                                              new XAttribute("Bold", font.Bold.ToYesNo()),
                                              new XAttribute("Italic", font.Italic.ToYesNo()),
                                              new XAttribute("Strike", font.Strikeout.ToYesNo()),
                                              new XAttribute("Underline", font.Underline.ToYesNo())));
            }

            foreach (string key in Properties.Keys)
                ui.Add(new XElement("Property",
                           new XAttribute("Id", key),
                           new XAttribute("Value", Properties[key])));

            foreach (string dialogId in DialogRefs)
                ui.Add(new XElement("DialogRef",
                           new XAttribute("Id", dialogId)));

            foreach (PublishingInfo info in this.UISequence)
            {
                int index = 0;
                foreach (DialogAction action in info.Actions)
                {
                    index++;
                    var element = ui.AddElement(new XElement("Publish",
                                                    new XAttribute("Dialog", info.Dialog),
                                                    new XAttribute("Control", info.Control),
                                                    action.Condition));

                    Action<string, string> AddAttribute = (name, value) =>
                    {
                        if (!value.IsEmpty())
                            element.Add(new XAttribute(name, value));
                    };

                    AddAttribute("Event", action.Name);
                    AddAttribute("Value", action.Value);
                    AddAttribute("Property", action.Property);

                    if (action.Order.HasValue)
                    {
                        element.Add(new XAttribute("Order", action.Order.Value));
                    }
                    else if (info.Actions.Count > 1)
                    {
                        element.Add(new XAttribute("Order", index));
                    }
                }
            }

            foreach (Dialog dialog in this.CustomDialogs)
            {
                ui.Add(dialog.ToXElement());
            }

            return ui;
        }
    }


    /// <summary>
    /// Defines values (names) for the WiX custom UI predefined dialog types. 
    /// </summary>
    public class CommonDialogs
    {
#pragma warning disable 1591
        public const string BrowseDlg = "BrowseDlg";
        public const string DiskCostDlg = "DiskCostDlg";
        public const string ErrorDlg = "ErrorDlg";
        public const string FatalError = "FatalError";
        public const string FilesInUse = "FilesInUse";
        public const string MsiRMFilesInUse = "MsiRMFilesInUse";
        public const string PrepareDlg = "PrepareDlg";
        public const string ProgressDlg = "ProgressDlg";
        public const string ResumeDlg = "ResumeDlg";
        public const string UserExit = "UserExit";
#pragma warning restore 1591
        /// <summary>
        /// Returns all WiX custom UI predefined dialog types.
        /// </summary>
        /// <returns>Array of dialog types.</returns>
        static public string[] AllValues()
        {
            return Utils.AllConstStringValues<CommonDialogs>();
        }
    }

    /// <summary>
    /// Defines values (names) for the standard WiX dialogs. 
    /// </summary>
    public class Dialogs
    {
#pragma warning disable 1591
        public const string ExitDialog = "ExitDialog";
        public const string WelcomeDlg = "WelcomeDlg";
        public const string LicenseAgreementDlg = "LicenseAgreementDlg";
        public const string InstallDirDlg = "InstallDirDlg";
        public const string VerifyReadyDlg = "VerifyReadyDlg";
        public const string MaintenanceWelcomeDlg = "MaintenanceWelcomeDlg";
        public const string MaintenanceTypeDlg = "MaintenanceTypeDlg";
#pragma warning restore 1591
    }


    /// <summary>
    /// Defines values (names) for the WiX custom UI predefined buttons. 
    /// </summary>
    public class Buttons
    {
#pragma warning disable 1591
        public const string Next = "Next";
        public const string Back = "Back";
        public const string Cancel = "Cancel";
        public const string Finish = "Finish";
        public const string ChangeFolder = "ChangeFolder";
        public const string Repair = "RepairButton";
        public const string Remove = "RemoveButton";
#pragma warning restore 1591
    }

    /// <summary>
    /// The Factory class for building <see cref="T:WixSharp.CustomUI"/>. 
    /// <para>
    /// While it is possible to construct  instance manually it is preferred to use 
    /// Factory methods of  <see cref="T:WixSharp.CustomUIBuilder"/> for this.
    /// </para>
    /// <code>
    ///  project.UI = WUI.WixUI_Common;
    ///  project.CustomUI = CustomUIBuilder.BuildPostLicenseDialogUI(productActivationDialog,
    ///                                                              onNextActions: new DialogAction[]{
    ///                                                                                 new ExecueteCustomAction ("ValidateLicenceKey"),
    ///                                                                                 new ShowDialog(Dialogs.InstallDirDlg, "SERIALNUMBER_VALIDATED = \"TRUE\"")});
    /// </code>
    /// </summary>
    public class CustomUIBuilder
    {

        /// <summary>
        /// Builds <see cref="T:WixSharp.CustomUI"/> instance and injects <see cref="T:WixSharp.Dialog"/> into the standard UI sequence 
        /// just after <c>LicenceDialog</c> step. 
        /// </summary>
        /// <param name="customDialog">The <see cref="T:WixSharp.Dialog"/> dialog to be injected.</param>
        /// <param name="onNextActions">The on next actions.</param>
        /// <param name="onBackActions">The on back actions.</param>
        /// <param name="onCancelActions">The on cancel actions.</param>
        /// <returns><see cref="T:WixSharp.CustomUI"/> instance.</returns>
        public static CustomUI BuildPostLicenseDialogUI(Dialog customDialog,
            DialogAction[] onNextActions = null,
            DialogAction[] onBackActions = null,
            DialogAction[] onCancelActions = null)
        {
            var customUI = new CustomUI();

            customUI.CustomDialogs.Add(customDialog);

            customUI.On(Dialogs.ExitDialog, Buttons.Finish, new CloseDialog() { Order = 9999 });

            customUI.On(Dialogs.WelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.LicenseAgreementDlg));

            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Back, new ShowDialog(Dialogs.WelcomeDlg));
            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Next, new ShowDialog(customDialog, "LicenseAccepted = \"1\""));

            customUI.On(customDialog, Buttons.Back, onBackActions ?? new DialogAction[] { new ShowDialog(Dialogs.LicenseAgreementDlg) });
            customUI.On(customDialog, Buttons.Next, onNextActions ?? new DialogAction[] { new ShowDialog(Dialogs.InstallDirDlg) });
            customUI.On(customDialog, Buttons.Cancel, onCancelActions ?? new DialogAction[] { new CloseDialog("Exit") });

            customUI.On(Dialogs.InstallDirDlg, Buttons.Back, new ShowDialog(customDialog));
            customUI.On(Dialogs.InstallDirDlg, Buttons.Next, new SetTargetPath(),
                                                             new ShowDialog(Dialogs.VerifyReadyDlg));

            customUI.On(Dialogs.InstallDirDlg, Buttons.ChangeFolder,
                                                             new SetProperty("_BrowseProperty", "[WIXUI_INSTALLDIR]"),
                                                             new ShowDialog(CommonDialogs.BrowseDlg));

            customUI.On(Dialogs.VerifyReadyDlg, Buttons.Back, new ShowDialog(Dialogs.InstallDirDlg, Condition.NOT_Installed),
                                                              new ShowDialog(Dialogs.MaintenanceTypeDlg, Condition.Installed));

            customUI.On(Dialogs.MaintenanceWelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.MaintenanceTypeDlg));

            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Back, new ShowDialog(Dialogs.MaintenanceWelcomeDlg));
            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Repair, new ShowDialog(Dialogs.VerifyReadyDlg));
            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Remove, new ShowDialog(Dialogs.VerifyReadyDlg));

            return customUI;
        }


        /// <summary>
        /// Injects the post license CLR dialog.
        /// Builds <see cref="T:WixSharp.CustomUI"/> instance and injects the ID of the <see cref="T:WixSharp.ManagedAction"/> for displaying 
        /// the custom CLD dialog into the standard UI sequence 
        /// just after <c>LicenceDialog</c> step. 
        /// <code>
        /// project.UI = WUI.WixUI_Common;
        /// project.CustomUI = CustomUIBuilder.InjectPostLicenseClrDialog(customDialog.Id, " LicenseAccepted = \"1\"");
        /// </code>
        /// </summary>
        /// <param name="showClrDialogAction">The show CLR dialog action.</param>
        /// <param name="clrDialogGoNextCondition">The CLR dialog go next condition.</param>
        /// <returns></returns>
        public static CustomUI InjectPostLicenseClrDialog(string showClrDialogAction, string clrDialogGoNextCondition = null)
        {
            var customUI = new CustomUI();

            customUI.On(Dialogs.ExitDialog, Buttons.Finish, new CloseDialog() { Order = 9999 });

            customUI.On(Dialogs.WelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.LicenseAgreementDlg));

            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Back, new ShowDialog(Dialogs.WelcomeDlg));
            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Next, new ExecueteCustomAction(showClrDialogAction));
            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Next, new ShowDialog(Dialogs.InstallDirDlg, "Custom_UI_Command = \"next\" AND  " + (clrDialogGoNextCondition ?? "\"1\"")));
            customUI.On(Dialogs.LicenseAgreementDlg, Buttons.Next, new CloseDialog("Exit", "Custom_UI_Command = \"abort\"") {Order=2});

            customUI.On(Dialogs.InstallDirDlg, Buttons.Back, new ExecueteCustomAction(showClrDialogAction));
            customUI.On(Dialogs.InstallDirDlg, Buttons.Back, new ShowDialog(Dialogs.LicenseAgreementDlg, "Custom_UI_Command = \"back\""));
            customUI.On(Dialogs.InstallDirDlg, Buttons.Next, new SetTargetPath(),
                                                             new ShowDialog(Dialogs.VerifyReadyDlg));

            customUI.On(Dialogs.InstallDirDlg, Buttons.ChangeFolder,
                                                             new SetProperty("_BrowseProperty", "[WIXUI_INSTALLDIR]"),
                                                             new ShowDialog(CommonDialogs.BrowseDlg));

            customUI.On(Dialogs.VerifyReadyDlg, Buttons.Back, new ShowDialog(Dialogs.InstallDirDlg, Condition.NOT_Installed),
                                                              new ShowDialog(Dialogs.MaintenanceTypeDlg, Condition.Installed));

            customUI.On(Dialogs.MaintenanceWelcomeDlg, Buttons.Next, new ShowDialog(Dialogs.MaintenanceTypeDlg));

            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Back, new ShowDialog(Dialogs.MaintenanceWelcomeDlg));
            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Repair, new ShowDialog(Dialogs.VerifyReadyDlg));
            customUI.On(Dialogs.MaintenanceTypeDlg, Buttons.Remove, new ShowDialog(Dialogs.VerifyReadyDlg));

            return customUI;
        }
    }
}
