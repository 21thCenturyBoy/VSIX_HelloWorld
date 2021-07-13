namespace FirstToolWin
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using System.ComponentModel.Design;
    using System.Windows.Forms;
    using Microsoft.VisualStudio.Shell.Interop;
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("9f156029-8b19-4cf4-b37f-6f41cb317956")]
    public class FirstToolWindow : ToolWindowPane
    {
        public FirstToolWindowControl control;
        /// <summary>
        /// Initializes a new instance of the <see cref="FirstToolWindow"/> class.
        /// </summary>
        public FirstToolWindow() : base(null)
        {
            this.Caption = "FirstToolWindow";

            control = new FirstToolWindowControl();
            this.Content = control;

            this.ToolBar = new CommandID(FirstToolWindowCommand.CommandSet, FirstToolWindowCommand.ToolbarID);
            this.ToolBarLocation = (int)VSTWT_LOCATION.VSTWT_TOP;
        }
    }
}
