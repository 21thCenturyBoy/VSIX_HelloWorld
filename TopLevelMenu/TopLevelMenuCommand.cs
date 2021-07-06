using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;
using System.Collections;

namespace TopLevelMenu
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class TopLevelMenuCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;
        /// <summary>
        /// Sub Command ID
        /// </summary>
        public const int cmdidTestSubCmd = 0x0105;

        /// <summary>
        /// Command ID
        /// </summary>
        public const uint cmdidMRUList = 0x200;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("4af50214-e270-40a5-9176-a6b36af31cf0");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="TopLevelMenuCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private TopLevelMenuCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            CommandID subCommandID = new CommandID(CommandSet, cmdidTestSubCmd);
            MenuCommand subItem = new MenuCommand(new EventHandler(SubItemCallback), subCommandID);
            commandService.AddCommand(subItem);

            this.InitMRUMenu(commandService);
        }

        #region InitializeMRU

        private int numMRUItems = 4;//生成项数
        private int baseMRUID = (int)cmdidMRUList;//初始占位符ID
        private ArrayList mruList;//命令名称字符串

        /// <summary>
        /// 初始化在 MRU 列表中显示的项的字符串列表
        /// </summary>
        private void InitializeMRUList()
        {
            if (null != mruList) return;

            mruList = new ArrayList();

            for (int i = 0; i < this.numMRUItems; i++) mruList.Add(string.Format(CultureInfo.CurrentCulture, "C# Script {0}.cs", i + 1));
        }
        /// <summary>
        /// 这会初始化 MRU list 菜单命令
        /// </summary>
        /// <param name="mcs"></param>
        private void InitMRUMenu(OleMenuCommandService mcs)
        {
            InitializeMRUList();

            for (int i = 0; i < this.numMRUItems; i++)
            {
                var cmdID = new CommandID(CommandSet, this.baseMRUID + i);//赋值ID
                var mc = new OleMenuCommand(OnMRUExec, cmdID);
                mc.BeforeQueryStatus += OnMRUQueryStatus;//当客户端请求命令的状态时调用,准备打卡
                mcs.AddCommand(mc);
            }
        }
        /// <summary>
        /// 显示回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMRUQueryStatus(object sender, EventArgs e)
        {
            OleMenuCommand menuCommand = sender as OleMenuCommand;
            if (null == menuCommand) return;

            int MRUItemIndex = menuCommand.CommandID.ID - this.baseMRUID;
            if (MRUItemIndex >= 0 && MRUItemIndex < this.mruList.Count)
            {
                menuCommand.Text = this.mruList[MRUItemIndex] as string;
            }

        }
        /// <summary>
        /// 触发处理命令回调
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMRUExec(object sender, EventArgs e)
        {
            var menuCommand = sender as OleMenuCommand;
            if (null == menuCommand) return;

            int MRUItemIndex = menuCommand.CommandID.ID - this.baseMRUID;

            if (MRUItemIndex >= 0 && MRUItemIndex < this.mruList.Count)
            {
                string selection = this.mruList[MRUItemIndex] as string;
                //向后排序
                for (int i = MRUItemIndex; i > 0; i--)
                {
                    this.mruList[i] = this.mruList[i - 1];
                }
                this.mruList[0] = selection;

                System.Windows.Forms.MessageBox.Show(string.Format(CultureInfo.CurrentCulture, "打开脚本 {0}", selection));
            }

        }
        #endregion

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static TopLevelMenuCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in TopLevelMenuCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new TopLevelMenuCommand(package, commandService);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.MenuItemCallback()", this.GetType().FullName);
            string title = "TopLevelMenuCommand";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
        private void SubItemCallback(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string message = string.Format(CultureInfo.CurrentCulture, "Inside {0}.SubItemCallback()", this.GetType().FullName);
            string title = "CmdIdTestSubCmd";

            // Show a message box to prove we were here
            VsShellUtilities.ShowMessageBox(
                this.package,
                message,
                title,
                OLEMSGICON.OLEMSGICON_INFO,
                OLEMSGBUTTON.OLEMSGBUTTON_OK,
                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST);
        }
    }
}
