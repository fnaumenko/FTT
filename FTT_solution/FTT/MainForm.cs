using System;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Plot;

namespace FTT
{
	public class MainForm : System.Windows.Forms.Form
    {
        #region Constants
        const string FILE_FA_FILTER = "fa files (*.fa)|*.fa|All files (*.*)|*.*";
        const string FILE_TXT_FILTER = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
        const string READING = "Reading...";
        const string SCANNING = "Scanning...";
        const string SORTING = "Sorting...";
        const string OUTPUTTING = "Printing...";
        const string READ_CANCEL = "Data read canceled";
        const string USER_CANCEL = "Canceled by user";
        const string OPEN = "Open ";
        const string VIEW = "View";
        const string DLL_FAULT = "Missing dll";
        const string INP_FAULT = "Input fault";
        const string INPUT = "Input: ";
        const string ERROR = "Error";
        const string DONE = "Done";
        const string NON = "Non";
        const string HEAD_SEP = ", ";
        const string YEAR = "©2012";
        const string OUTPUTDATA = " output data";
        static string SAVE_REQUEST = "You will lose unsaved current result.\n" + new string(' ', 22) + "Save it?";

        const string FORMAT_ZERO_VISIBLE = "{0:0.##}";
        //const string FORMAT_ZERO_HIDE = "{0:.##}";      // not in use now

        /// <summary>Different between max and min height of tabTask control.</summary>
        static int TabTASK_HEIGHT_SHIFT;
        const string PnlUP = ">";
        const string PnlDOWN = "<";

        #endregion
        #region Task Settings
        /// <summary>Represents common labels and settings for the task.</summary>
        struct TaskSetting
        {
            /// <summary>Name of Task</summary>
            public string TaskName;
            /// <summary>Label of output richTextBox</summary>
            public string OutputLabel;
            /// <summary>Label of Plot control</summary>
            public string GraphicLabel;
            /// <summary>Capture of X axis on the plot</summary>
            public string AxisXCapture;
            /// <summary>Capture of Y axis on the plot</summary>
            public string AxisYCapture;
            /// <summary>Plot mode</summary>
            public Plot.Modes PlotMode;
            /// <summary>The name of input file.</summary>
            public string FileName;
            /// <summary>The result of calculation: collection TrimmedPatterns | ScanWindows </summary>
            public object Issue;
            ///// <summary>True if result of calculation is saved.</summary>
            //public bool IsObjectSaved;
        }
        TaskSetting[] _taskSettings = new TaskSetting[2];

        #endregion
        #region Members

		/// <summary>The sign of full size of the MainForm.</summary>
		bool	_isGraphVisible = true;
        /// <summary>The current stopwatch.</summary>
        Stopwatch _stopWatch = new Stopwatch();
        /// <summary>The pen to drawing unshaked histogramm.</summary>
        Pen _firstPen = new Pen(Color.Red, 2f);
        /// <summary>The pen to drawing shaked histogramm.</summary>
        Pen _secondPen = new Pen(Color.Blue, 2f);
        /// <summary>Current Width of Graph Control.</summary>
        int _graphWidth;
        /// <summary>The capture of the MainForm.</summary>
        string _mainCapture;

        /// <summary>The initial height of tabTask control for supply of let up-down;.</summary>
        int _tabTaskHeightMax;
        /// <summary>The brush of link-button (Up-Down(.</summary>
        Brush _linkBrush;

		private System.Windows.Forms.Button btnStart;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.StatusBar stsBar;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private Plot.Plot Graph;
        private System.Windows.Forms.StatusBarPanel stsBarPnlTime;
        private System.Windows.Forms.StatusBarPanel stsBarPnlInfo;
		private System.Windows.Forms.RichTextBox rTxtBox;
        private System.Windows.Forms.Timer timerMain;
		private System.Windows.Forms.StatusBarPanel stsBarPnlStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
		private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnAbout;
		private System.Windows.Forms.Label lblOutputText;
        private System.Windows.Forms.Label lblGraphic;
        private System.Windows.Forms.Button btnRead;
        private ToolTip toolTip;
        private BackgroundWorker backgroundWorker;
        private CheckBox chkBoxSaveRequest;
        private NeatTabControl tabTask;
        private TabPage tabPgGlobal;
        private TabPage tabPgLocal;
        private GroupBox grpBoxFrequncy;
        private RadioButton rBtnIndividual;
        private RadioButton rBtnMax;
        private NumericUpDown nmrShakesCnt;
        private Label lblShake;
        private Label lblLength;
        private Label lblInexactCnt;
        private NumericUpDown nmrPattLength;
        private Label lblMismatches;
        private NumericUpDown nmrMismatchCnt;
        private GroupBox grpBoxPattern;
        private Label lblMotif;
        private CheckBox chkBoxSimilars;
        private NumericUpDown nmrVisibleCnt;
        private CheckBox chkBoxShake;
        private GroupBox grpBoxFilter;
        private TextBox txtBoxMinFF;
        private Label lblCritFF;
        private TextBox txtBoxMaxFF;
        private TextBox txtBoxMaxCV;
        private Label lblCritCV;
        private TextBox txtBoxMinCV;
        private CheckBox chkBoxCritCV;
        private CheckBox chkBoxCritFF;
        private GroupBox grpBoxWindow;
        private Label lblWinShift;
        private Label lblWinIncr;
        private Label lblWinStopLenth;
        private Label lblWinStartLength;
        private NumericUpDown nmrWinShift;
        private NumericUpDown nmrWinIncr;
        private NumericUpDown nmrWinStopLength;
        private NumericUpDown nmrWinStartLength;
        private CheckBox chkBoxTurbo;
        private GroupBox grpBoxWorkPatterns;
        private RadioButton rBtnSeqPatterns;
        private RadioButton rBtnAllPatterns;
        private Button btnHelp;
        private SplitContainer splitContainer;
        private LinkLabel linkLbl;
        private Label lblPuttCount;
        private HelpProvider helpProvider;
        private ProgressBar prgBar;
        private Label lblOutputCaption;
        private CheckBox chkBoxDraw;

        #endregion
        #region Constructor and Destructor
        public MainForm()
		{
			InitializeComponent();

            #region debug settings
            //nmrPattLength.Maximum = 25;
            //nmrPattLength.Value = 7;
            //nmrMismatchCnt.Value = 0;
            //nmrShakesCnt.Minimum = 1;
            //nmrShakesCnt.Value = 5;
            //chkBoxShake.Checked = false;
            //rBtnSeqPatterns.Checked = true;     // from input only
            //nmrWinIncr.Value = 100;
            //nmrWinShift.Value = 100;
            //chkBoxSaveRequest.Checked = false;
            //rBtnMax.Checked = true;
            #endregion
            #region fill _taskSettings
            // local
            TaskSetting tsk = _taskSettings[0];
            tsk.AxisXCapture = Abbr.Frequency;
            tsk.AxisYCapture = "Quantity";
            tsk.GraphicLabel = "HISTOGRAM";
            tsk.OutputLabel = "PATTERNS STATISTIC";
            tsk.PlotMode = Plot.Modes.BarChart;
            tsk.TaskName = Abbr.Local;
            // global
            _taskSettings[0] = tsk;
            tsk = _taskSettings[1];
            tsk.AxisXCapture = "Window's start position";
            tsk.AxisYCapture = Abbr.F;
            tsk.GraphicLabel = "GRAPHIC";
            tsk.OutputLabel = "WINDOWS STATISTIC";
            tsk.PlotMode = Plot.Modes.Graph;
            tsk.TaskName = Abbr.Global;
            _taskSettings[1] = tsk;
            #endregion
            #region Main Form
            this.MinimumSize = this.Size;
            _mainCapture = this.Text;
            lblOutputText.Text = _taskSettings[0].OutputLabel;
            #endregion
            #region tabTask

            tabTask.Initialize();
            grpBoxPattern.BackColor = lblShake.BackColor = tabTask.TabPages[1].BackColor = tabTask.TabPages[0].BackColor;

            // set & save tabTask max height for correct AutoScaling;
            _tabTaskHeightMax = grpBoxFilter.Bottom + tabTask.ItemSize.Height + 10;
            // set height shift;
            TabTASK_HEIGHT_SHIFT = grpBoxFilter.Height - txtBoxMinFF.Top + 5;
            tabTask.Height = _tabTaskHeightMax - TabTASK_HEIGHT_SHIFT;
            linkLbl.Text = "  ";

            // set visual order
            tabTask.SendToBack();   // visualizes bottom Shuffles controls
            // Hide Check Boxes under tabTask
            chkBoxDraw.SendToBack();
            chkBoxTurbo.SendToBack();
            chkBoxSaveRequest.SendToBack();
            // set visibility of dependent controls
            chkBoxTurbo.Visible = grpBoxFrequncy.Visible = nmrShakesCnt.Enabled = chkBoxShake.Checked;
            // set link brush
            _linkBrush = new SolidBrush(grpBoxFilter.ForeColor);

            #endregion
            #region set ProgressBar
            const int BORDER = 4;
            Point pt = stsBar.Location;
            pt.Y += BORDER;
            pt.X += stsBarPnlInfo.Width + BORDER;
            //pt.X += BORDER;
            prgBar.Location = pt;
            prgBar.Width = stsBarPnlStatus.Width - BORDER;
			prgBar.Height = stsBar.Height - 6;
            prgBar.Anchor = AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right;
            #endregion
            #region Graph
            Graph.Init(_taskSettings[0].AxisXCapture, _taskSettings[0].AxisYCapture);
            Graph.Mode = _taskSettings[0].PlotMode;
            Graph.PenWidth = 1.5f;
            Graph.SetLimits(Plot.Limits.AutoLimit, Plot.Limits.AutoLimit, 0, Plot.Limits.AutoLimit);
            #endregion
            #region HelpProvider settings

            string helpName = "FTT_help.chm";

            if (File.Exists(helpName))
                helpProvider.HelpNamespace = helpName;
            else
            {
                helpName = @"Help\" + helpName;
                if (File.Exists(helpName))
                    helpProvider.HelpNamespace = helpName;
                else
                {   // execution from Visual Studio
                    helpName = @"..\..\..\" + helpName;
                    if (File.Exists(helpName))
                        helpProvider.HelpNamespace = helpName;
                }
            }

            // Work Patterns mode
            helpProvider.SetHelpNavigator(rBtnAllPatterns, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(rBtnAllPatterns, "Work Patterns.htm");
            helpProvider.SetHelpNavigator(rBtnSeqPatterns, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(rBtnSeqPatterns, "Work Patterns.htm");
            // Pattern
            helpProvider.SetHelpNavigator(nmrPattLength, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrPattLength, "Pattern.htm");
            helpProvider.SetHelpNavigator(nmrMismatchCnt, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrMismatchCnt, "Pattern.htm");
            // Shuffles
            helpProvider.SetHelpNavigator(chkBoxShake, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxShake, "Shuffles.htm");
            helpProvider.SetHelpNavigator(nmrShakesCnt, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrShakesCnt, "Shuffles.htm");
            // Frequency calc
            helpProvider.SetHelpNavigator(rBtnIndividual, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(rBtnIndividual, "FrequencyCalc.htm");
            helpProvider.SetHelpNavigator(rBtnMax, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(rBtnMax, "FrequencyCalc.htm");
            // Show similars
            helpProvider.SetHelpNavigator(chkBoxSimilars, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxSimilars, "Show similars.htm");
            helpProvider.SetHelpNavigator(nmrVisibleCnt, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrVisibleCnt, "Show similars.htm");
            // Window
            helpProvider.SetHelpNavigator(nmrWinStartLength, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrWinStartLength, "Window.htm");
            helpProvider.SetHelpNavigator(nmrWinStopLength, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrWinStopLength, "Window.htm");
            helpProvider.SetHelpNavigator(nmrWinIncr, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrWinIncr, "Window.htm");
            helpProvider.SetHelpNavigator(nmrWinShift, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(nmrWinShift, "Window.htm");
            // Filter
            helpProvider.SetHelpNavigator(chkBoxCritFF, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxCritFF, "Filter.htm");
            helpProvider.SetHelpNavigator(txtBoxMinFF, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(txtBoxMinFF, "Filter.htm");
            helpProvider.SetHelpNavigator(txtBoxMaxFF, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(txtBoxMaxFF, "Filter.htm");
            helpProvider.SetHelpNavigator(chkBoxCritCV, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxCritCV, "Filter.htm");
            helpProvider.SetHelpNavigator(txtBoxMinCV, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(txtBoxMinCV, "Filter.htm");
            helpProvider.SetHelpNavigator(txtBoxMaxCV, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(txtBoxMaxCV, "Filter.htm");
            // common
            helpProvider.SetHelpNavigator(chkBoxTurbo, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxTurbo, "Turbo.htm");
            helpProvider.SetHelpNavigator(chkBoxDraw, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxDraw, "Show graphic.htm");
            helpProvider.SetHelpNavigator(chkBoxSaveRequest, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(chkBoxSaveRequest, "Saving request.htm");
            // buttons
            helpProvider.SetHelpNavigator(btnStart, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(btnStart, "Buttons.htm");
            helpProvider.SetHelpNavigator(btnCancel, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(btnCancel, "Buttons.htm");
            helpProvider.SetHelpNavigator(btnRead, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(btnRead, "Buttons.htm");
            helpProvider.SetHelpNavigator(btnSave, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(btnSave, "Buttons.htm");
            // output
            helpProvider.SetHelpNavigator(rTxtBox, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(rTxtBox, "Output text.htm");
            helpProvider.SetHelpNavigator(Graph, HelpNavigator.Topic);
            helpProvider.SetHelpKeyword(Graph, "Output graphic.htm");

            #endregion            
            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");
            btnStart.Focus();
		}

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
		{
            if (disposing)
			{
                _firstPen.Dispose();
                _secondPen.Dispose();
                _linkBrush.Dispose();
                if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}
        #endregion
        #region Windows Form Designer generated code
        /// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.btnStart = new System.Windows.Forms.Button();
            this.rTxtBox = new System.Windows.Forms.RichTextBox();
            this.lblOutputText = new System.Windows.Forms.Label();
            this.stsBar = new System.Windows.Forms.StatusBar();
            this.stsBarPnlInfo = new System.Windows.Forms.StatusBarPanel();
            this.stsBarPnlStatus = new System.Windows.Forms.StatusBarPanel();
            this.stsBarPnlTime = new System.Windows.Forms.StatusBarPanel();
            this.btnCancel = new System.Windows.Forms.Button();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.timerMain = new System.Windows.Forms.Timer(this.components);
            this.lblGraphic = new System.Windows.Forms.Label();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.Graph = new Plot.Plot();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.chkBoxSaveRequest = new System.Windows.Forms.CheckBox();
            this.nmrShakesCnt = new System.Windows.Forms.NumericUpDown();
            this.lblLength = new System.Windows.Forms.Label();
            this.nmrPattLength = new System.Windows.Forms.NumericUpDown();
            this.lblMismatches = new System.Windows.Forms.Label();
            this.nmrMismatchCnt = new System.Windows.Forms.NumericUpDown();
            this.chkBoxTurbo = new System.Windows.Forms.CheckBox();
            this.chkBoxDraw = new System.Windows.Forms.CheckBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.lblShake = new System.Windows.Forms.Label();
            this.grpBoxPattern = new System.Windows.Forms.GroupBox();
            this.lblInexactCnt = new System.Windows.Forms.Label();
            this.rBtnSeqPatterns = new System.Windows.Forms.RadioButton();
            this.rBtnAllPatterns = new System.Windows.Forms.RadioButton();
            this.grpBoxWorkPatterns = new System.Windows.Forms.GroupBox();
            this.lblPuttCount = new System.Windows.Forms.Label();
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.lblOutputCaption = new System.Windows.Forms.Label();
            this.backgroundWorker = new System.ComponentModel.BackgroundWorker();
            this.helpProvider = new System.Windows.Forms.HelpProvider();
            this.prgBar = new System.Windows.Forms.ProgressBar();
            this.tabTask = new FTT.NeatTabControl();
            this.tabPgLocal = new System.Windows.Forms.TabPage();
            this.lblMotif = new System.Windows.Forms.Label();
            this.chkBoxSimilars = new System.Windows.Forms.CheckBox();
            this.nmrVisibleCnt = new System.Windows.Forms.NumericUpDown();
            this.chkBoxShake = new System.Windows.Forms.CheckBox();
            this.grpBoxFrequncy = new System.Windows.Forms.GroupBox();
            this.rBtnIndividual = new System.Windows.Forms.RadioButton();
            this.rBtnMax = new System.Windows.Forms.RadioButton();
            this.tabPgGlobal = new System.Windows.Forms.TabPage();
            this.grpBoxFilter = new System.Windows.Forms.GroupBox();
            this.linkLbl = new System.Windows.Forms.LinkLabel();
            this.txtBoxMinFF = new System.Windows.Forms.TextBox();
            this.lblCritFF = new System.Windows.Forms.Label();
            this.txtBoxMaxFF = new System.Windows.Forms.TextBox();
            this.txtBoxMaxCV = new System.Windows.Forms.TextBox();
            this.lblCritCV = new System.Windows.Forms.Label();
            this.txtBoxMinCV = new System.Windows.Forms.TextBox();
            this.chkBoxCritCV = new System.Windows.Forms.CheckBox();
            this.chkBoxCritFF = new System.Windows.Forms.CheckBox();
            this.grpBoxWindow = new System.Windows.Forms.GroupBox();
            this.lblWinShift = new System.Windows.Forms.Label();
            this.lblWinIncr = new System.Windows.Forms.Label();
            this.lblWinStopLenth = new System.Windows.Forms.Label();
            this.lblWinStartLength = new System.Windows.Forms.Label();
            this.nmrWinShift = new System.Windows.Forms.NumericUpDown();
            this.nmrWinIncr = new System.Windows.Forms.NumericUpDown();
            this.nmrWinStopLength = new System.Windows.Forms.NumericUpDown();
            this.nmrWinStartLength = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlTime)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrShakesCnt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrPattLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrMismatchCnt)).BeginInit();
            this.grpBoxPattern.SuspendLayout();
            this.grpBoxWorkPatterns.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.tabTask.SuspendLayout();
            this.tabPgLocal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmrVisibleCnt)).BeginInit();
            this.grpBoxFrequncy.SuspendLayout();
            this.tabPgGlobal.SuspendLayout();
            this.grpBoxFilter.SuspendLayout();
            this.grpBoxWindow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinShift)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinIncr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinStopLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinStartLength)).BeginInit();
            this.SuspendLayout();
            // 
            // btnStart
            // 
            resources.ApplyResources(this.btnStart, "btnStart");
            this.btnStart.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnStart.Name = "btnStart";
            this.helpProvider.SetShowHelp(this.btnStart, ((bool)(resources.GetObject("btnStart.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnStart, resources.GetString("btnStart.ToolTip"));
            this.btnStart.UseVisualStyleBackColor = false;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            this.btnStart.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.esc_KeyPress);
            // 
            // rTxtBox
            // 
            resources.ApplyResources(this.rTxtBox, "rTxtBox");
            this.rTxtBox.BackColor = System.Drawing.SystemColors.ButtonHighlight;
            this.rTxtBox.Name = "rTxtBox";
            this.rTxtBox.ReadOnly = true;
            this.helpProvider.SetShowHelp(this.rTxtBox, ((bool)(resources.GetObject("rTxtBox.ShowHelp"))));
            this.rTxtBox.TabStop = false;
            this.toolTip.SetToolTip(this.rTxtBox, resources.GetString("rTxtBox.ToolTip"));
            // 
            // lblOutputText
            // 
            resources.ApplyResources(this.lblOutputText, "lblOutputText");
            this.lblOutputText.Name = "lblOutputText";
            this.helpProvider.SetShowHelp(this.lblOutputText, ((bool)(resources.GetObject("lblOutputText.ShowHelp"))));
            // 
            // stsBar
            // 
            resources.ApplyResources(this.stsBar, "stsBar");
            this.stsBar.CausesValidation = false;
            this.stsBar.Name = "stsBar";
            this.stsBar.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.stsBarPnlInfo,
            this.stsBarPnlStatus,
            this.stsBarPnlTime});
            this.helpProvider.SetShowHelp(this.stsBar, ((bool)(resources.GetObject("stsBar.ShowHelp"))));
            this.stsBar.ShowPanels = true;
            this.stsBar.SizingGrip = false;
            // 
            // stsBarPnlInfo
            // 
            resources.ApplyResources(this.stsBarPnlInfo, "stsBarPnlInfo");
            // 
            // stsBarPnlStatus
            // 
            this.stsBarPnlStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            resources.ApplyResources(this.stsBarPnlStatus, "stsBarPnlStatus");
            // 
            // stsBarPnlTime
            // 
            resources.ApplyResources(this.stsBarPnlTime, "stsBarPnlTime");
            // 
            // btnCancel
            // 
            resources.ApplyResources(this.btnCancel, "btnCancel");
            this.btnCancel.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Name = "btnCancel";
            this.helpProvider.SetShowHelp(this.btnCancel, ((bool)(resources.GetObject("btnCancel.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnCancel, resources.GetString("btnCancel.ToolTip"));
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // timerMain
            // 
            this.timerMain.Enabled = true;
            this.timerMain.Interval = 50;
            this.timerMain.Tick += new System.EventHandler(this.timerMain_Tick);
            // 
            // lblGraphic
            // 
            resources.ApplyResources(this.lblGraphic, "lblGraphic");
            this.lblGraphic.Name = "lblGraphic";
            this.helpProvider.SetShowHelp(this.lblGraphic, ((bool)(resources.GetObject("lblGraphic.ShowHelp"))));
            // 
            // btnSave
            // 
            resources.ApplyResources(this.btnSave, "btnSave");
            this.btnSave.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnSave.Name = "btnSave";
            this.helpProvider.SetShowHelp(this.btnSave, ((bool)(resources.GetObject("btnSave.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnSave, resources.GetString("btnSave.ToolTip"));
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnAbout
            // 
            this.btnAbout.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.btnAbout, "btnAbout");
            this.btnAbout.Name = "btnAbout";
            this.helpProvider.SetShowHelp(this.btnAbout, ((bool)(resources.GetObject("btnAbout.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnAbout, resources.GetString("btnAbout.ToolTip"));
            this.btnAbout.UseVisualStyleBackColor = false;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnRead
            // 
            resources.ApplyResources(this.btnRead, "btnRead");
            this.btnRead.BackColor = System.Drawing.SystemColors.ControlLight;
            this.btnRead.Name = "btnRead";
            this.helpProvider.SetShowHelp(this.btnRead, ((bool)(resources.GetObject("btnRead.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnRead, resources.GetString("btnRead.ToolTip"));
            this.btnRead.UseVisualStyleBackColor = false;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // Graph
            // 
            resources.ApplyResources(this.Graph, "Graph");
            this.Graph.AutoValidate = System.Windows.Forms.AutoValidate.Disable;
            this.Graph.AxisMargin = ((byte)(3));
            this.Graph.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.Graph.CausesValidation = false;
            this.Graph.ExtMargin = ((byte)(5));
            this.Graph.IsSpline = false;
            this.Graph.IsXInteger = true;
            this.Graph.IsYInteger = true;
            this.Graph.LegendMargin = ((byte)(3));
            this.Graph.LowPowerX = ((short)(-2));
            this.Graph.LowPowerY = ((short)(-2));
            this.Graph.Mode = Plot.Modes.Graph;
            this.Graph.Name = "Graph";
            this.Graph.PenWidth = 1F;
            this.Graph.ShowBorder = true;
            this.helpProvider.SetShowHelp(this.Graph, ((bool)(resources.GetObject("Graph.ShowHelp"))));
            this.Graph.ShowLegend = false;
            this.Graph.TitleMargin = ((byte)(1));
            this.toolTip.SetToolTip(this.Graph, resources.GetString("Graph.ToolTip"));
            this.Graph.TopPowerX = ((short)(3));
            this.Graph.TopPowerY = ((short)(3));
            this.Graph.ValueMargin = ((byte)(5));
            // 
            // chkBoxSaveRequest
            // 
            resources.ApplyResources(this.chkBoxSaveRequest, "chkBoxSaveRequest");
            this.chkBoxSaveRequest.Checked = true;
            this.chkBoxSaveRequest.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxSaveRequest.Name = "chkBoxSaveRequest";
            this.helpProvider.SetShowHelp(this.chkBoxSaveRequest, ((bool)(resources.GetObject("chkBoxSaveRequest.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxSaveRequest, resources.GetString("chkBoxSaveRequest.ToolTip"));
            this.chkBoxSaveRequest.UseVisualStyleBackColor = true;
            // 
            // nmrShakesCnt
            // 
            resources.ApplyResources(this.nmrShakesCnt, "nmrShakesCnt");
            this.nmrShakesCnt.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nmrShakesCnt.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nmrShakesCnt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmrShakesCnt.Name = "nmrShakesCnt";
            this.helpProvider.SetShowHelp(this.nmrShakesCnt, ((bool)(resources.GetObject("nmrShakesCnt.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrShakesCnt, resources.GetString("nmrShakesCnt.ToolTip"));
            this.nmrShakesCnt.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nmrShakesCnt.ValueChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // lblLength
            // 
            this.lblLength.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblLength, "lblLength");
            this.lblLength.Name = "lblLength";
            this.helpProvider.SetShowHelp(this.lblLength, ((bool)(resources.GetObject("lblLength.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblLength, resources.GetString("lblLength.ToolTip"));
            // 
            // nmrPattLength
            // 
            resources.ApplyResources(this.nmrPattLength, "nmrPattLength");
            this.nmrPattLength.Maximum = new decimal(new int[] {
            13,
            0,
            0,
            0});
            this.nmrPattLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nmrPattLength.Name = "nmrPattLength";
            this.helpProvider.SetShowHelp(this.nmrPattLength, ((bool)(resources.GetObject("nmrPattLength.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrPattLength, resources.GetString("nmrPattLength.ToolTip"));
            this.nmrPattLength.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nmrPattLength.ValueChanged += new System.EventHandler(this.Pattern_ValueChanged);
            // 
            // lblMismatches
            // 
            this.lblMismatches.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblMismatches, "lblMismatches");
            this.lblMismatches.Name = "lblMismatches";
            this.helpProvider.SetShowHelp(this.lblMismatches, ((bool)(resources.GetObject("lblMismatches.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblMismatches, resources.GetString("lblMismatches.ToolTip"));
            // 
            // nmrMismatchCnt
            // 
            resources.ApplyResources(this.nmrMismatchCnt, "nmrMismatchCnt");
            this.nmrMismatchCnt.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nmrMismatchCnt.Name = "nmrMismatchCnt";
            this.helpProvider.SetShowHelp(this.nmrMismatchCnt, ((bool)(resources.GetObject("nmrMismatchCnt.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrMismatchCnt, resources.GetString("nmrMismatchCnt.ToolTip"));
            this.nmrMismatchCnt.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmrMismatchCnt.ValueChanged += new System.EventHandler(this.Pattern_ValueChanged);
            // 
            // chkBoxTurbo
            // 
            resources.ApplyResources(this.chkBoxTurbo, "chkBoxTurbo");
            this.chkBoxTurbo.Name = "chkBoxTurbo";
            this.helpProvider.SetShowHelp(this.chkBoxTurbo, ((bool)(resources.GetObject("chkBoxTurbo.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxTurbo, resources.GetString("chkBoxTurbo.ToolTip"));
            this.chkBoxTurbo.UseVisualStyleBackColor = true;
            // 
            // chkBoxDraw
            // 
            resources.ApplyResources(this.chkBoxDraw, "chkBoxDraw");
            this.chkBoxDraw.Checked = true;
            this.chkBoxDraw.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkBoxDraw.Name = "chkBoxDraw";
            this.helpProvider.SetShowHelp(this.chkBoxDraw, ((bool)(resources.GetObject("chkBoxDraw.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxDraw, resources.GetString("chkBoxDraw.ToolTip"));
            this.chkBoxDraw.CheckedChanged += new System.EventHandler(this.chkBoxDraw_CheckedChanged);
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.btnHelp, "btnHelp");
            this.btnHelp.Name = "btnHelp";
            this.helpProvider.SetShowHelp(this.btnHelp, ((bool)(resources.GetObject("btnHelp.ShowHelp"))));
            this.toolTip.SetToolTip(this.btnHelp, resources.GetString("btnHelp.ToolTip"));
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // lblShake
            // 
            resources.ApplyResources(this.lblShake, "lblShake");
            this.lblShake.BackColor = System.Drawing.SystemColors.Control;
            this.lblShake.Name = "lblShake";
            this.helpProvider.SetShowHelp(this.lblShake, ((bool)(resources.GetObject("lblShake.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblShake, resources.GetString("lblShake.ToolTip"));
            // 
            // grpBoxPattern
            // 
            this.grpBoxPattern.BackColor = System.Drawing.SystemColors.Control;
            this.grpBoxPattern.Controls.Add(this.nmrMismatchCnt);
            this.grpBoxPattern.Controls.Add(this.lblMismatches);
            this.grpBoxPattern.Controls.Add(this.nmrPattLength);
            this.grpBoxPattern.Controls.Add(this.lblInexactCnt);
            this.grpBoxPattern.Controls.Add(this.lblLength);
            this.grpBoxPattern.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.grpBoxPattern, "grpBoxPattern");
            this.grpBoxPattern.Name = "grpBoxPattern";
            this.helpProvider.SetShowHelp(this.grpBoxPattern, ((bool)(resources.GetObject("grpBoxPattern.ShowHelp"))));
            this.grpBoxPattern.TabStop = false;
            this.toolTip.SetToolTip(this.grpBoxPattern, resources.GetString("grpBoxPattern.ToolTip"));
            // 
            // lblInexactCnt
            // 
            resources.ApplyResources(this.lblInexactCnt, "lblInexactCnt");
            this.lblInexactCnt.Name = "lblInexactCnt";
            this.helpProvider.SetShowHelp(this.lblInexactCnt, ((bool)(resources.GetObject("lblInexactCnt.ShowHelp"))));
            // 
            // rBtnSeqPatterns
            // 
            resources.ApplyResources(this.rBtnSeqPatterns, "rBtnSeqPatterns");
            this.rBtnSeqPatterns.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rBtnSeqPatterns.Name = "rBtnSeqPatterns";
            this.helpProvider.SetShowHelp(this.rBtnSeqPatterns, ((bool)(resources.GetObject("rBtnSeqPatterns.ShowHelp"))));
            this.toolTip.SetToolTip(this.rBtnSeqPatterns, resources.GetString("rBtnSeqPatterns.ToolTip"));
            this.rBtnSeqPatterns.UseVisualStyleBackColor = true;
            // 
            // rBtnAllPatterns
            // 
            resources.ApplyResources(this.rBtnAllPatterns, "rBtnAllPatterns");
            this.rBtnAllPatterns.Checked = true;
            this.rBtnAllPatterns.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rBtnAllPatterns.Name = "rBtnAllPatterns";
            this.helpProvider.SetShowHelp(this.rBtnAllPatterns, ((bool)(resources.GetObject("rBtnAllPatterns.ShowHelp"))));
            this.rBtnAllPatterns.TabStop = true;
            this.toolTip.SetToolTip(this.rBtnAllPatterns, resources.GetString("rBtnAllPatterns.ToolTip"));
            this.rBtnAllPatterns.UseVisualStyleBackColor = true;
            this.rBtnAllPatterns.CheckedChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // grpBoxWorkPatterns
            // 
            this.grpBoxWorkPatterns.Controls.Add(this.rBtnSeqPatterns);
            this.grpBoxWorkPatterns.Controls.Add(this.rBtnAllPatterns);
            this.grpBoxWorkPatterns.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.grpBoxWorkPatterns, "grpBoxWorkPatterns");
            this.grpBoxWorkPatterns.Name = "grpBoxWorkPatterns";
            this.helpProvider.SetShowHelp(this.grpBoxWorkPatterns, ((bool)(resources.GetObject("grpBoxWorkPatterns.ShowHelp"))));
            this.grpBoxWorkPatterns.TabStop = false;
            this.toolTip.SetToolTip(this.grpBoxWorkPatterns, resources.GetString("grpBoxWorkPatterns.ToolTip"));
            // 
            // lblPuttCount
            // 
            resources.ApplyResources(this.lblPuttCount, "lblPuttCount");
            this.lblPuttCount.Name = "lblPuttCount";
            this.helpProvider.SetShowHelp(this.lblPuttCount, ((bool)(resources.GetObject("lblPuttCount.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblPuttCount, resources.GetString("lblPuttCount.ToolTip"));
            // 
            // splitContainer
            // 
            resources.ApplyResources(this.splitContainer, "splitContainer");
            this.splitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer.Name = "splitContainer";
            // 
            // splitContainer.Panel1
            // 
            this.splitContainer.Panel1.Controls.Add(this.lblOutputCaption);
            this.splitContainer.Panel1.Controls.Add(this.rTxtBox);
            this.splitContainer.Panel1.Controls.Add(this.lblOutputText);
            this.helpProvider.SetShowHelp(this.splitContainer.Panel1, ((bool)(resources.GetObject("splitContainer.Panel1.ShowHelp"))));
            // 
            // splitContainer.Panel2
            // 
            this.splitContainer.Panel2.Controls.Add(this.Graph);
            this.splitContainer.Panel2.Controls.Add(this.lblGraphic);
            this.helpProvider.SetShowHelp(this.splitContainer.Panel2, ((bool)(resources.GetObject("splitContainer.Panel2.ShowHelp"))));
            this.helpProvider.SetShowHelp(this.splitContainer, ((bool)(resources.GetObject("splitContainer.ShowHelp"))));
            this.toolTip.SetToolTip(this.splitContainer, resources.GetString("splitContainer.ToolTip"));
            // 
            // lblOutputCaption
            // 
            resources.ApplyResources(this.lblOutputCaption, "lblOutputCaption");
            this.lblOutputCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(252)))), ((int)(((byte)(252)))), ((int)(((byte)(252)))));
            this.lblOutputCaption.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblOutputCaption.ForeColor = System.Drawing.SystemColors.HotTrack;
            this.lblOutputCaption.Name = "lblOutputCaption";
            this.helpProvider.SetShowHelp(this.lblOutputCaption, ((bool)(resources.GetObject("lblOutputCaption.ShowHelp"))));
            // 
            // backgroundWorker
            // 
            this.backgroundWorker.WorkerReportsProgress = true;
            this.backgroundWorker.WorkerSupportsCancellation = true;
            this.backgroundWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // helpProvider
            // 
            resources.ApplyResources(this.helpProvider, "helpProvider");
            // 
            // prgBar
            // 
            resources.ApplyResources(this.prgBar, "prgBar");
            this.prgBar.Name = "prgBar";
            this.helpProvider.SetShowHelp(this.prgBar, ((bool)(resources.GetObject("prgBar.ShowHelp"))));
            this.prgBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // tabTask
            // 
            this.tabTask.Controls.Add(this.tabPgLocal);
            this.tabTask.Controls.Add(this.tabPgGlobal);
            this.tabTask.DrawMode = System.Windows.Forms.TabDrawMode.OwnerDrawFixed;
            resources.ApplyResources(this.tabTask, "tabTask");
            this.tabTask.Name = "tabTask";
            this.tabTask.SelectedIndex = 0;
            this.helpProvider.SetShowHelp(this.tabTask, ((bool)(resources.GetObject("tabTask.ShowHelp"))));
            this.tabTask.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.toolTip.SetToolTip(this.tabTask, resources.GetString("tabTask.ToolTip"));
            this.tabTask.SelectedIndexChanged += new System.EventHandler(this.tabTask_SelectedIndexChanged);
            // 
            // tabPgLocal
            // 
            this.tabPgLocal.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.tabPgLocal, "tabPgLocal");
            this.tabPgLocal.Controls.Add(this.lblMotif);
            this.tabPgLocal.Controls.Add(this.chkBoxSimilars);
            this.tabPgLocal.Controls.Add(this.nmrVisibleCnt);
            this.tabPgLocal.Controls.Add(this.chkBoxShake);
            this.tabPgLocal.Controls.Add(this.grpBoxFrequncy);
            this.tabPgLocal.Name = "tabPgLocal";
            this.helpProvider.SetShowHelp(this.tabPgLocal, ((bool)(resources.GetObject("tabPgLocal.ShowHelp"))));
            // 
            // lblMotif
            // 
            resources.ApplyResources(this.lblMotif, "lblMotif");
            this.lblMotif.Name = "lblMotif";
            this.helpProvider.SetShowHelp(this.lblMotif, ((bool)(resources.GetObject("lblMotif.ShowHelp"))));
            // 
            // chkBoxSimilars
            // 
            resources.ApplyResources(this.chkBoxSimilars, "chkBoxSimilars");
            this.chkBoxSimilars.Name = "chkBoxSimilars";
            this.helpProvider.SetShowHelp(this.chkBoxSimilars, ((bool)(resources.GetObject("chkBoxSimilars.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxSimilars, resources.GetString("chkBoxSimilars.ToolTip"));
            this.chkBoxSimilars.CheckedChanged += new System.EventHandler(this.chkBoxSimilars_CheckedChanged);
            // 
            // nmrVisibleCnt
            // 
            resources.ApplyResources(this.nmrVisibleCnt, "nmrVisibleCnt");
            this.nmrVisibleCnt.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.nmrVisibleCnt.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.nmrVisibleCnt.Name = "nmrVisibleCnt";
            this.helpProvider.SetShowHelp(this.nmrVisibleCnt, ((bool)(resources.GetObject("nmrVisibleCnt.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrVisibleCnt, resources.GetString("nmrVisibleCnt.ToolTip"));
            this.nmrVisibleCnt.Value = new decimal(new int[] {
            5,
            0,
            0,
            0});
            // 
            // chkBoxShake
            // 
            this.chkBoxShake.Checked = true;
            this.chkBoxShake.CheckState = System.Windows.Forms.CheckState.Checked;
            resources.ApplyResources(this.chkBoxShake, "chkBoxShake");
            this.chkBoxShake.Name = "chkBoxShake";
            this.helpProvider.SetShowHelp(this.chkBoxShake, ((bool)(resources.GetObject("chkBoxShake.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxShake, resources.GetString("chkBoxShake.ToolTip"));
            this.chkBoxShake.CheckedChanged += new System.EventHandler(this.chkBoxShake_CheckedChanged);
            // 
            // grpBoxFrequncy
            // 
            this.grpBoxFrequncy.Controls.Add(this.rBtnIndividual);
            this.grpBoxFrequncy.Controls.Add(this.rBtnMax);
            this.grpBoxFrequncy.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.grpBoxFrequncy, "grpBoxFrequncy");
            this.grpBoxFrequncy.Name = "grpBoxFrequncy";
            this.helpProvider.SetShowHelp(this.grpBoxFrequncy, ((bool)(resources.GetObject("grpBoxFrequncy.ShowHelp"))));
            this.grpBoxFrequncy.TabStop = false;
            this.toolTip.SetToolTip(this.grpBoxFrequncy, resources.GetString("grpBoxFrequncy.ToolTip"));
            // 
            // rBtnIndividual
            // 
            resources.ApplyResources(this.rBtnIndividual, "rBtnIndividual");
            this.rBtnIndividual.Checked = true;
            this.rBtnIndividual.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rBtnIndividual.Name = "rBtnIndividual";
            this.helpProvider.SetShowHelp(this.rBtnIndividual, ((bool)(resources.GetObject("rBtnIndividual.ShowHelp"))));
            this.rBtnIndividual.TabStop = true;
            this.toolTip.SetToolTip(this.rBtnIndividual, resources.GetString("rBtnIndividual.ToolTip"));
            this.rBtnIndividual.UseVisualStyleBackColor = true;
            this.rBtnIndividual.CheckedChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // rBtnMax
            // 
            resources.ApplyResources(this.rBtnMax, "rBtnMax");
            this.rBtnMax.ForeColor = System.Drawing.SystemColors.ControlText;
            this.rBtnMax.Name = "rBtnMax";
            this.helpProvider.SetShowHelp(this.rBtnMax, ((bool)(resources.GetObject("rBtnMax.ShowHelp"))));
            this.toolTip.SetToolTip(this.rBtnMax, resources.GetString("rBtnMax.ToolTip"));
            this.rBtnMax.UseVisualStyleBackColor = true;
            // 
            // tabPgGlobal
            // 
            this.tabPgGlobal.BackColor = System.Drawing.SystemColors.ControlLight;
            resources.ApplyResources(this.tabPgGlobal, "tabPgGlobal");
            this.tabPgGlobal.Controls.Add(this.grpBoxFilter);
            this.tabPgGlobal.Controls.Add(this.grpBoxWindow);
            this.tabPgGlobal.Cursor = System.Windows.Forms.Cursors.Default;
            this.tabPgGlobal.Name = "tabPgGlobal";
            this.helpProvider.SetShowHelp(this.tabPgGlobal, ((bool)(resources.GetObject("tabPgGlobal.ShowHelp"))));
            // 
            // grpBoxFilter
            // 
            this.grpBoxFilter.Controls.Add(this.linkLbl);
            this.grpBoxFilter.Controls.Add(this.txtBoxMinFF);
            this.grpBoxFilter.Controls.Add(this.lblCritFF);
            this.grpBoxFilter.Controls.Add(this.txtBoxMaxFF);
            this.grpBoxFilter.Controls.Add(this.txtBoxMaxCV);
            this.grpBoxFilter.Controls.Add(this.lblCritCV);
            this.grpBoxFilter.Controls.Add(this.txtBoxMinCV);
            this.grpBoxFilter.Controls.Add(this.chkBoxCritCV);
            this.grpBoxFilter.Controls.Add(this.chkBoxCritFF);
            this.grpBoxFilter.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.grpBoxFilter, "grpBoxFilter");
            this.grpBoxFilter.Name = "grpBoxFilter";
            this.helpProvider.SetShowHelp(this.grpBoxFilter, ((bool)(resources.GetObject("grpBoxFilter.ShowHelp"))));
            this.grpBoxFilter.TabStop = false;
            this.toolTip.SetToolTip(this.grpBoxFilter, resources.GetString("grpBoxFilter.ToolTip"));
            // 
            // linkLbl
            // 
            this.linkLbl.AutoEllipsis = true;
            resources.ApplyResources(this.linkLbl, "linkLbl");
            this.linkLbl.LinkBehavior = System.Windows.Forms.LinkBehavior.NeverUnderline;
            this.linkLbl.LinkColor = System.Drawing.SystemColors.Highlight;
            this.linkLbl.Name = "linkLbl";
            this.helpProvider.SetShowHelp(this.linkLbl, ((bool)(resources.GetObject("linkLbl.ShowHelp"))));
            this.linkLbl.TabStop = true;
            this.linkLbl.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLbl_LinkClicked);
            this.linkLbl.Paint += new System.Windows.Forms.PaintEventHandler(this.linkLbl_Paint);
            // 
            // txtBoxMinFF
            // 
            resources.ApplyResources(this.txtBoxMinFF, "txtBoxMinFF");
            this.txtBoxMinFF.Name = "txtBoxMinFF";
            this.helpProvider.SetShowHelp(this.txtBoxMinFF, ((bool)(resources.GetObject("txtBoxMinFF.ShowHelp"))));
            this.toolTip.SetToolTip(this.txtBoxMinFF, resources.GetString("txtBoxMinFF.ToolTip"));
            this.txtBoxMinFF.TextChanged += new System.EventHandler(this.txtBoxFF_TextChanged);
            // 
            // lblCritFF
            // 
            resources.ApplyResources(this.lblCritFF, "lblCritFF");
            this.lblCritFF.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCritFF.Name = "lblCritFF";
            this.helpProvider.SetShowHelp(this.lblCritFF, ((bool)(resources.GetObject("lblCritFF.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblCritFF, resources.GetString("lblCritFF.ToolTip"));
            // 
            // txtBoxMaxFF
            // 
            resources.ApplyResources(this.txtBoxMaxFF, "txtBoxMaxFF");
            this.txtBoxMaxFF.Name = "txtBoxMaxFF";
            this.helpProvider.SetShowHelp(this.txtBoxMaxFF, ((bool)(resources.GetObject("txtBoxMaxFF.ShowHelp"))));
            this.toolTip.SetToolTip(this.txtBoxMaxFF, resources.GetString("txtBoxMaxFF.ToolTip"));
            this.txtBoxMaxFF.TextChanged += new System.EventHandler(this.txtBoxFF_TextChanged);
            // 
            // txtBoxMaxCV
            // 
            resources.ApplyResources(this.txtBoxMaxCV, "txtBoxMaxCV");
            this.txtBoxMaxCV.Name = "txtBoxMaxCV";
            this.helpProvider.SetShowHelp(this.txtBoxMaxCV, ((bool)(resources.GetObject("txtBoxMaxCV.ShowHelp"))));
            this.toolTip.SetToolTip(this.txtBoxMaxCV, resources.GetString("txtBoxMaxCV.ToolTip"));
            this.txtBoxMaxCV.TextChanged += new System.EventHandler(this.txtBoxCV_TextChanged);
            // 
            // lblCritCV
            // 
            resources.ApplyResources(this.lblCritCV, "lblCritCV");
            this.lblCritCV.ForeColor = System.Drawing.SystemColors.ControlText;
            this.lblCritCV.Name = "lblCritCV";
            this.helpProvider.SetShowHelp(this.lblCritCV, ((bool)(resources.GetObject("lblCritCV.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblCritCV, resources.GetString("lblCritCV.ToolTip"));
            // 
            // txtBoxMinCV
            // 
            resources.ApplyResources(this.txtBoxMinCV, "txtBoxMinCV");
            this.txtBoxMinCV.Name = "txtBoxMinCV";
            this.helpProvider.SetShowHelp(this.txtBoxMinCV, ((bool)(resources.GetObject("txtBoxMinCV.ShowHelp"))));
            this.toolTip.SetToolTip(this.txtBoxMinCV, resources.GetString("txtBoxMinCV.ToolTip"));
            this.txtBoxMinCV.TextChanged += new System.EventHandler(this.txtBoxCV_TextChanged);
            // 
            // chkBoxCritCV
            // 
            resources.ApplyResources(this.chkBoxCritCV, "chkBoxCritCV");
            this.chkBoxCritCV.Name = "chkBoxCritCV";
            this.helpProvider.SetShowHelp(this.chkBoxCritCV, ((bool)(resources.GetObject("chkBoxCritCV.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxCritCV, resources.GetString("chkBoxCritCV.ToolTip"));
            this.chkBoxCritCV.CheckedChanged += new System.EventHandler(this.chkBoxCritCV_CheckedChanged);
            // 
            // chkBoxCritFF
            // 
            resources.ApplyResources(this.chkBoxCritFF, "chkBoxCritFF");
            this.chkBoxCritFF.Name = "chkBoxCritFF";
            this.helpProvider.SetShowHelp(this.chkBoxCritFF, ((bool)(resources.GetObject("chkBoxCritFF.ShowHelp"))));
            this.toolTip.SetToolTip(this.chkBoxCritFF, resources.GetString("chkBoxCritFF.ToolTip"));
            this.chkBoxCritFF.CheckedChanged += new System.EventHandler(this.chkBoxCritFF_CheckedChanged);
            // 
            // grpBoxWindow
            // 
            this.grpBoxWindow.Controls.Add(this.lblWinShift);
            this.grpBoxWindow.Controls.Add(this.lblWinIncr);
            this.grpBoxWindow.Controls.Add(this.lblWinStopLenth);
            this.grpBoxWindow.Controls.Add(this.lblWinStartLength);
            this.grpBoxWindow.Controls.Add(this.nmrWinShift);
            this.grpBoxWindow.Controls.Add(this.nmrWinIncr);
            this.grpBoxWindow.Controls.Add(this.nmrWinStopLength);
            this.grpBoxWindow.Controls.Add(this.nmrWinStartLength);
            this.grpBoxWindow.ForeColor = System.Drawing.SystemColors.Highlight;
            resources.ApplyResources(this.grpBoxWindow, "grpBoxWindow");
            this.grpBoxWindow.Name = "grpBoxWindow";
            this.helpProvider.SetShowHelp(this.grpBoxWindow, ((bool)(resources.GetObject("grpBoxWindow.ShowHelp"))));
            this.grpBoxWindow.TabStop = false;
            this.toolTip.SetToolTip(this.grpBoxWindow, resources.GetString("grpBoxWindow.ToolTip"));
            // 
            // lblWinShift
            // 
            this.lblWinShift.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblWinShift, "lblWinShift");
            this.lblWinShift.Name = "lblWinShift";
            this.helpProvider.SetShowHelp(this.lblWinShift, ((bool)(resources.GetObject("lblWinShift.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblWinShift, resources.GetString("lblWinShift.ToolTip"));
            // 
            // lblWinIncr
            // 
            this.lblWinIncr.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblWinIncr, "lblWinIncr");
            this.lblWinIncr.Name = "lblWinIncr";
            this.helpProvider.SetShowHelp(this.lblWinIncr, ((bool)(resources.GetObject("lblWinIncr.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblWinIncr, resources.GetString("lblWinIncr.ToolTip"));
            // 
            // lblWinStopLenth
            // 
            this.lblWinStopLenth.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblWinStopLenth, "lblWinStopLenth");
            this.lblWinStopLenth.Name = "lblWinStopLenth";
            this.helpProvider.SetShowHelp(this.lblWinStopLenth, ((bool)(resources.GetObject("lblWinStopLenth.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblWinStopLenth, resources.GetString("lblWinStopLenth.ToolTip"));
            // 
            // lblWinStartLength
            // 
            this.lblWinStartLength.ForeColor = System.Drawing.SystemColors.ControlText;
            resources.ApplyResources(this.lblWinStartLength, "lblWinStartLength");
            this.lblWinStartLength.Name = "lblWinStartLength";
            this.helpProvider.SetShowHelp(this.lblWinStartLength, ((bool)(resources.GetObject("lblWinStartLength.ShowHelp"))));
            this.toolTip.SetToolTip(this.lblWinStartLength, resources.GetString("lblWinStartLength.ToolTip"));
            // 
            // nmrWinShift
            // 
            this.nmrWinShift.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            resources.ApplyResources(this.nmrWinShift, "nmrWinShift");
            this.nmrWinShift.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nmrWinShift.Name = "nmrWinShift";
            this.helpProvider.SetShowHelp(this.nmrWinShift, ((bool)(resources.GetObject("nmrWinShift.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrWinShift, resources.GetString("nmrWinShift.ToolTip"));
            this.nmrWinShift.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.nmrWinShift.ValueChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // nmrWinIncr
            // 
            this.nmrWinIncr.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            resources.ApplyResources(this.nmrWinIncr, "nmrWinIncr");
            this.nmrWinIncr.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.nmrWinIncr.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nmrWinIncr.Name = "nmrWinIncr";
            this.helpProvider.SetShowHelp(this.nmrWinIncr, ((bool)(resources.GetObject("nmrWinIncr.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrWinIncr, resources.GetString("nmrWinIncr.ToolTip"));
            this.nmrWinIncr.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            this.nmrWinIncr.ValueChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // nmrWinStopLength
            // 
            this.nmrWinStopLength.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            resources.ApplyResources(this.nmrWinStopLength, "nmrWinStopLength");
            this.nmrWinStopLength.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nmrWinStopLength.Minimum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nmrWinStopLength.Name = "nmrWinStopLength";
            this.helpProvider.SetShowHelp(this.nmrWinStopLength, ((bool)(resources.GetObject("nmrWinStopLength.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrWinStopLength, resources.GetString("nmrWinStopLength.ToolTip"));
            this.nmrWinStopLength.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            this.nmrWinStopLength.ValueChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // nmrWinStartLength
            // 
            this.nmrWinStartLength.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            resources.ApplyResources(this.nmrWinStartLength, "nmrWinStartLength");
            this.nmrWinStartLength.Maximum = new decimal(new int[] {
            500,
            0,
            0,
            0});
            this.nmrWinStartLength.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nmrWinStartLength.Name = "nmrWinStartLength";
            this.helpProvider.SetShowHelp(this.nmrWinStartLength, ((bool)(resources.GetObject("nmrWinStartLength.ShowHelp"))));
            this.toolTip.SetToolTip(this.nmrWinStartLength, resources.GetString("nmrWinStartLength.ToolTip"));
            this.nmrWinStartLength.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.nmrWinStartLength.ValueChanged += new System.EventHandler(this.checkToSave_ValueChanged);
            // 
            // MainForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.prgBar);
            this.Controls.Add(this.lblPuttCount);
            this.Controls.Add(this.splitContainer);
            this.Controls.Add(this.tabTask);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.grpBoxWorkPatterns);
            this.Controls.Add(this.chkBoxTurbo);
            this.Controls.Add(this.chkBoxDraw);
            this.Controls.Add(this.chkBoxSaveRequest);
            this.Controls.Add(this.lblShake);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.stsBar);
            this.Controls.Add(this.grpBoxPattern);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.nmrShakesCnt);
            this.HelpButton = true;
            this.Name = "MainForm";
            this.helpProvider.SetShowHelp(this, ((bool)(resources.GetObject("$this.ShowHelp"))));
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stsBarPnlTime)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrShakesCnt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrPattLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrMismatchCnt)).EndInit();
            this.grpBoxPattern.ResumeLayout(false);
            this.grpBoxWorkPatterns.ResumeLayout(false);
            this.grpBoxWorkPatterns.PerformLayout();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.ResumeLayout(false);
            this.tabTask.ResumeLayout(false);
            this.tabPgLocal.ResumeLayout(false);
            this.tabPgLocal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nmrVisibleCnt)).EndInit();
            this.grpBoxFrequncy.ResumeLayout(false);
            this.grpBoxFrequncy.PerformLayout();
            this.tabPgGlobal.ResumeLayout(false);
            this.grpBoxFilter.ResumeLayout(false);
            this.grpBoxFilter.PerformLayout();
            this.grpBoxWindow.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinShift)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinIncr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinStopLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nmrWinStartLength)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion
        #region Main
        /// <summary>The main entry point for the application.</summary>
		[STAThread]
		static void Main() 
		{
            try
            {
                Application.Run(new MainForm());
            }
            catch (FileNotFoundException e)   // not found Plot.dll 
            {
                MsgBoxError(e.Message, DLL_FAULT);
            }
            catch (Exception e)                         // for any case
            {
                MsgBoxError(e.Message, "?");
            }
        }

        public static void MsgBoxError(string msg, string capt)
        {
            MessageBox.Show(msg,
                String.Format("{0} - {1}", Abbr.FTT, capt),
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #endregion
        #region File input

        /// <summary>Gets the current input file name.</summary>
        string FileName
        {
            get { return _taskSettings[TaskIndex].FileName; }
            set { _taskSettings[TaskIndex].FileName = value; }
        }

		/// <summary>Gets the full caption text of this instance.</summary>
		/// <param name="isMaximized">True if this instance window is maximized.</param>
		string	CaptionText	(bool isMaximized)
		{
            string capt = isMaximized ? _mainCapture : Abbr.FTT;
            return FileName == null ? capt :
				String.Format("{0} - {1}", ShortFileName, capt );
		}

        /// <summary>Gets the short file name.</summary>
        string ShortFileName
        {
            get
            {
                return FileName == null ? string.Empty :
                    FileName.Substring(FileName.LastIndexOf('\\') + 1);
            }
        }

        /// <summary>Restores the string represented the value by the name of field in a file.</summary>
		/// <param name="head">The string of the head with all the writing values.</param>
		/// <param name="fieldName">The name of field.</param>
		/// <returns></returns>
		string		RestoreValImage	(string head, string fieldName)
		{
			int ind, ind0, ind1;
			ind0 = head.IndexOf(fieldName) + fieldName.Length;
			ind1 = head.IndexOf(HEAD_SEP, ind0-2);
            ind = head.IndexOf(Abbr.EOL, ind0 - 2);
			if( ind1 < 0 || ind1 > ind )
				ind1 = ind;
			return head.Substring(ind0, ind1-ind0);
		}

		/// <summary>Restores the integer value by the name of field.</summary>
		/// <param name="head">The string of the head with all the writing values.</param>
		/// <param name="fieldName">The name of field.</param>
		/// <returns></returns>
		int		RestoreIntVal	(string head, string fieldName)
		{
			return int.Parse(RestoreValImage(head, fieldName));
		}

		/// <summary>Restores the boolean value by the name of field.</summary>
		/// <param name="head">The string of the head with all the writing values.</param>
		/// <param name="fieldName">The name of field.</param>
		/// <returns></returns>
		bool	RestoreBoolVal	(string head, string fieldName)
		{
            return RestoreValImage(head, fieldName) != NON;
		}

		/// <summary>Restores the controls values for the output head.</summary>
		/// <param name="head">The string of the head with all the writing values.</param>
		void	RestoreControlsState	(string head)
		{
            // Work Patterns mode
            if (rBtnAllPatterns.Text == RestoreValImage(head, grpBoxPattern.Text))
                rBtnAllPatterns.Checked = true;
            else
                rBtnSeqPatterns.Checked = true;

			nmrPattLength.Value = RestoreIntVal(head, lblLength.Text);
			nmrMismatchCnt.Value = RestoreIntVal(head, lblMismatches.Text);
            if (chkBoxShake.Checked = RestoreBoolVal(head, lblShake.Text) )
                nmrShakesCnt.Value = RestoreIntVal(head, lblShake.Text);
			nmrWinStartLength.Value = RestoreIntVal(head, lblWinStartLength.Text);
			nmrWinStopLength.Value = RestoreIntVal(head, lblWinStopLenth.Text);
			nmrWinIncr.Value = RestoreIntVal(head, lblWinIncr.Text);
			nmrWinShift.Value = RestoreIntVal(head, lblWinShift.Text);
		}

		/// <summary>Opens the file by OpenFileDialog and reads the output ScanWindows data.</summary>
		/// <returns>The input data as a ScanWindows.</returns>
		ScanWindows ReadScanWindows	()
		{
			ScanWindows res = null;
			openFileDialog.Filter = FILE_TXT_FILTER;
            openFileDialog.Title = OPEN + Abbr.Global + OUTPUTDATA;

			if( openFileDialog.ShowDialog() == DialogResult.OK )
			{
                string header = string.Empty;
                FileName = openFileDialog.FileName;
                try
                {
                    res = ScanWindows.Read(FileName, ref header);
                    RestoreControlsState(header);
                    //Words.Init((byte)nmrPattLength.Value, (byte)nmrMismatchCnt.Value);
                }
                catch (Exception e)
                {
                    MsgBoxError(e.Message, INP_FAULT);
                }
                // define the short file name
                this.Text = this.CaptionText(true);
			}
            return res;
		}

		/// <summary>Opens the file by OpenFileDialog and reads the input sequance.</summary>
        /// <returns>True if input sequence is not empty.</returns>
        bool ReadSequance()
        {
			bool res = false;
            openFileDialog.Filter = FILE_FA_FILTER;
            openFileDialog.Title = OPEN + Abbr.SavedData;

			if( openFileDialog.ShowDialog() == DialogResult.OK )
			{
                FileName = openFileDialog.FileName;
                ClearOutputs();
                prgBar.Value = 0;
                try { res = Sequence.Read(FileName); }
                catch (Exception e) { MsgBoxError(e.Message, INP_FAULT); }
				// define the short file name
				this.Text = this.CaptionText(true);
			}
			return res;
		}
        #endregion
        #region File output

        /// <summary>Gets ths string represented NumericUpDown label and value.</summary>
        /// <param name="label">Output label of NumericUpDown control.</param>
        /// <param name="numeric">Output value of NumericUpDown control.</param>
        /// <returns></returns>
        string NumericToString (Label label, NumericUpDown numeric)
        {
            //return label.Text.Substring(0, label.Text.Length - 1) + "="     // cut ':' and set '='
            return label.Text + " "
                + (numeric.Enabled ? numeric.Value.ToString() : NON);       // add control's value
        }

        /// <summary>Writes the head of output in a stream.</summary>
		/// <param name="sWriter">The output StreamWriter.</param>
        void SetOutputHeader(StreamWriter sWriter)
		{
            // programme name, version, input file name
            sWriter.WriteLine("{0} v{1}.{2}  {3} output for {4}:  {5:### ### ###} {6}s",
                Abbr.FTT,
                System.Reflection.Assembly.GetAssembly(this.GetType()).GetName().Version.ToString(3),
                YEAR,
                _taskSettings[TaskIndex].TaskName,
                ShortFileName,
                stsBarPnlInfo.Text,
                Abbr.Pattern
                );
            // Work Patterns mode
            sWriter.WriteLine("{0}: {1}",
                grpBoxWorkPatterns.Text,
                rBtnAllPatterns.Checked ? rBtnAllPatterns.Text : rBtnSeqPatterns.Text
                );
            // Pattern: Length, Mismatches, Shakes
            sWriter.WriteLine( "{0}> {1}, {2}, {3}" ,
				grpBoxPattern.Text,
				NumericToString(lblLength, nmrPattLength),
				NumericToString(lblMismatches, nmrMismatchCnt),
                NumericToString(lblShake, nmrShakesCnt)
			    );
            if (IsGlobalTask)    // global windows settings
               sWriter.WriteLine("{0}> {1}, {2}, {3}, {4}",
                   grpBoxWindow.Text,
                   NumericToString(lblWinStartLength, nmrWinStartLength),
                   NumericToString(lblWinStopLenth, nmrWinStopLength),
                   NumericToString(lblWinIncr, nmrWinIncr),
                   NumericToString(lblWinShift, nmrWinShift)
                   );
            else
            {
                // Frequency mode
                if (chkBoxShake.Checked)
                   sWriter.WriteLine("{0}: {1}",
                       grpBoxFrequncy.Text,
                       rBtnMax.Checked ? rBtnMax.Text : rBtnIndividual.Text);
                // Similars
                sWriter.WriteLine("{0}{1}", chkBoxSimilars.Text,
                   nmrVisibleCnt.Enabled ?
                       " " + lblMotif.Text + " " + nmrVisibleCnt.Value.ToString() :
                       ": " + NON);
            }
            if( chkBoxTurbo.Visible )
                sWriter.WriteLine("Turbo: {0}",
                    chkBoxTurbo.Enabled && chkBoxTurbo.Checked ? "On" : "Off");
            sWriter.WriteLine();

            string procInfo = string.Empty;
            System.Management.ManagementObjectSearcher searcher =
                new System.Management.ManagementObjectSearcher("Select * from Win32_processor");
            foreach (System.Management.ManagementObject share in searcher.Get())
                procInfo = share["Name"] +
                    ((Convert.ToSingle(share["CurrentClockSpeed"])) / 1000).ToString(" 0.00 GHz");
            sWriter.WriteLine("Processor: {0}", procInfo);
            sWriter.WriteLine("Total runtime: {0}", stsBarPnlTime.Text);
            sWriter.WriteLine();
		}

		/// <summary>Writes data to file.</summary>
        /// <returns>True if writing was successfull.</returns>
		bool	WriteData	()
		{
            Stream stream;
            saveFileDialog.Filter = FILE_TXT_FILTER;
            saveFileDialog.FileName = Path.GetFileNameWithoutExtension(FileName) + "_out";
			saveFileDialog.Title = "Save texts result";

			if(saveFileDialog.ShowDialog() == DialogResult.OK)
                if ( (stream = saveFileDialog.OpenFile()) != null )
				{
//					rTxtBox.SaveFile(stream, RichTextBoxStreamType.PlainText);

					StreamWriter sWriter = new StreamWriter(stream);
                    SetOutputHeader(sWriter);
                    foreach (string str in rTxtBox.Lines)
                        sWriter.WriteLine(str);
					sWriter.Close();
					stream.Close();
                    return true;
				}
			return false;
		}
#if DEBUG
//        void	TestShake	()
//        {
//            string output;
//            string outfilename = _fileName.Replace(".", "_shake.");
////			saveFileDialog.FileName = _fileName.Replace(".", "_shake.");
////			saveFileDialog.Filter = FILE_NAME_FILTER;
////			saveFileDialog.Title = "Save shake test result";
//            for(int i=1; i<=50; i++)
//            {
//                output = Processing.Shake(_input);
//                StreamWriter sWriter = new StreamWriter(outfilename.Replace(".", i.ToString()+"."));
//                sWriter.WriteLine(ShortFileName + " after shaking");
//                sWriter.WriteLine(output);
//                sWriter.Close();

////				if(saveFileDialog.ShowDialog() == DialogResult.OK)
////				{
////					Stream stream = saveFileDialog.OpenFile();
////					if( stream != null)
////					{
////						StreamWriter sWriter = new StreamWriter(stream);
////						sWriter.WriteLine(ShortFileName + " after shaking");
////						sWriter.WriteLine(output);
////						sWriter.Close();
////						stream.Close();
////					}
////				}
//            }
//        }
#endif
		#endregion
		#region Output in controls

        /// <summary>Clear all the output controls.</summary>
        void ClearOutputs()
        {
            lblOutputCaption.Text = string.Empty;
            rTxtBox.Clear();
            Graph.Clear();
            stsBarPnlStatus.Text = stsBarPnlInfo.Text = string.Empty;
            _stopWatch.Reset();
        }

        /// <summary>Draws the graphic if it is permitted.</summary>
        /// <param name="obj">Drawing object.</param>
        /// <param name="isFirstCall">True if method is calling first time in cycle; otherwise, false.</param>
        void DrawGraph(object obj, bool isFirstPlot)
        {
            if (_isGraphVisible)
                if (obj.GetType() == typeof(Patterns))
                    Graph.DrawPlot(
                        ((Patterns)obj).Points, string.Empty,
                        isFirstPlot ? _firstPen : _secondPen,
                        isFirstPlot,
                        //Convert.ToByte(isFirstPlot)   // to see both the initial and the rest 
                        0
                        );
                else
                    Graph.DrawPlot((obj as ScanWindows).Points);
        }

        /// <summary>Draws the graphic if it is permitted.</summary>
        /// <param name="obj">Drawing object.</param>
        void Draw    (object obj)
        {
            if (_isGraphVisible && obj != null)
                if (obj.GetType() == typeof(TrimmedPatterns))
                    Graph.DrawPlot((obj as TrimmedPatterns).Patterns.Points, string.Empty, _firstPen);
                else
                    Graph.DrawPlot((obj as ScanWindows).Points);
            else
                Graph.Refresh();
        }

        /// <summary>Draws the current result if it is permitted.</summary>
        void Draw()
        {
            Draw(_taskSettings[TaskIndex].Issue);
        }

        /// <summary>Printed output array and selected it by default.</summary>
		/// <param name="output">The printed string array.</param>
        /// <param name="motives">True if motives are printed.</param>
        void OutputData(string[] output, bool motives)
		{
            if (output == null) return;
			
            //rTxtBox.SelectAll();
			rTxtBox.Lines = output;
            if (motives)
                // select motifs by red
                for (int pos=0, i=0; i < output.Length-1; i++)
				{
                    pos += output[i++].Length + 1;
                    rTxtBox.Select(pos, output[i].Length);
					rTxtBox.SelectionColor = Color.Red;
					pos += output[i].Length + 1;
				}
		}

        /// <summary>Outputs the TrimmedPatterns or ScanWindows in the RichTextBox.</summary>
        /// <param name="obj">The ouput object.</param>
        void Output(object obj)
        {
            if (obj == null) return;
            int count;
            stsBarPnlInfo.Text = OUTPUTTING;
            if (obj.GetType() == typeof(TrimmedPatterns))
            {
                TrimmedPatterns tp = obj as TrimmedPatterns;
                byte showMotifs = (byte)(chkBoxSimilars.Checked ? nmrVisibleCnt.Value : 0);
                lblOutputCaption.Text = tp.GetCaption(
                        chkBoxShake.Checked, showMotifs, rBtnIndividual.Checked);
                // Outputs the TrimmedPatterns in the RichTextBox.
                OutputData(
                    tp.ToStrings(chkBoxShake.Checked, showMotifs, rBtnIndividual.Checked),
                    chkBoxSimilars.Checked);
                count = (obj as TrimmedPatterns).Count;
            }
            else
            {
                ScanWindows sw = obj as ScanWindows;
                lblOutputCaption.Text = sw.Caption;
                OutputData(chkBoxCritFF.Checked || chkBoxCritCV.Checked ?
                    // Outputs the ScanWindows in the RichTextBox according to the filter setting.
                    sw.ToStrings(
                        CurrFloatOutputFormat,
                        GetSingle(txtBoxMinFF.Text, chkBoxCritFF.Checked, false),
                        GetSingle(txtBoxMaxFF.Text, chkBoxCritFF.Checked, true),
                        GetSingle(txtBoxMinCV.Text, chkBoxCritCV.Checked, false),
                        GetSingle(txtBoxMaxCV.Text, chkBoxCritCV.Checked, true)
                    ) :
                    // Outputs the ScanWindows in the RichTextBox without any filter.
                    sw.ToStrings(), false);
                count = (obj as ScanWindows).Count;
            }
            stsBarPnlInfo.Text = count.ToString();

            //Size sz = rTxtBox.Size;
            //using (Graphics g = CreateGraphics())
            //{
            //    sz.Width = (int)g.MeasureString(lblOutputCaption.Text, Font).Width + 5;
            //}
            ////sz.Width = lblOutputCaption.Text.Length * 8;
            //rTxtBox.Size = sz;
            //rTxtBox.MinimumSize = sz;
            //Refresh();
        }

        /// <summary>Outputs the current result in the RichTextBox.</summary>
        void Output()
        {
            Output(_taskSettings[TaskIndex].Issue);
        }

        /// <summary>Gets setted by user composite format string for float value.</summary>
        public string CurrFloatOutputFormat
        {
            get
            {
                //return chkBoxZeroVal.Checked ? FORMAT_ZERO_VISIBLE : FORMAT_ZERO_HIDE;
                return FORMAT_ZERO_VISIBLE;
            }
        }

#if DEBUG
		public	void	PrintTime	(string title)
		{
			if (_stopWatch.IsRunning)
			{
				TimeSpan ts = _stopWatch.Elapsed;
				stsBarPnlStatus.Text += String.Format("{0} {1:00}:{2:00}:{3:000}  ",
					title, ts.Minutes, ts.Seconds, ts.Milliseconds);
			}
		}
#endif
		#endregion
        #region Support controls methods

        /// <summary>Gets a value indicating whether the global task is setted.</summary>
        bool IsGlobalTask
        {
            get { return tabTask.SelectedIndex == 1; }
        }
        
        /// <summary>Gets 0 if current task is Single or 1 if current task is Multi.</summary>
        int TaskIndex
        {
            get { return tabTask.SelectedIndex; }
        }

        /// <summary>Raises the request for saving and save it if the current issue has been not saved.</summary>
        void AutoSave()
        {
            if (_taskSettings[TaskIndex].Issue == null) 
                return;    // no result to save
            if (chkBoxSaveRequest.Checked && btnSave.Enabled
            && MessageBox.Show(SAVE_REQUEST, Abbr.FTT, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                btnSave_Click(this, new EventArgs());
            //if( !btnSave.Enabled )
                ClearOutputs();
            _taskSettings[TaskIndex].Issue = null;
        }

        /// <summary>
		/// Sets the value indicating wether input controls can respond to user interaction.
		/// </summary>
        bool InputControlsEnabled
		{
			set
			{
                grpBoxWorkPatterns.Enabled =
                tabTask.Enabled =
                grpBoxPattern.Enabled =
                lblShake.Enabled = chkBoxShake.Enabled =
                chkBoxTurbo.Enabled = chkBoxDraw.Enabled =
                btnStart.Enabled = btnRead.Enabled 
                = value;
                btnSave.Enabled = value ? (rTxtBox.Text.Length > 0) : false;
                nmrShakesCnt.Enabled = value ? (IsGlobalTask || chkBoxShake.Checked) : false;
                btnCancel.Enabled = prgBar.Visible = !value;
            }
		}

		/// <summary>Set the size of MainForm to full or small width.</summary>
        private bool IsGraphVisible
		{
			set
			{
				if( _isGraphVisible != value )
				{
                    int shift;
                    if (_isGraphVisible = value)    // with graph
					{
                        shift = _graphWidth + splitContainer.SplitterWidth;
                        // Width first, MinimumSize second
                        Width += shift;
                        MinimumSize = new Size(MinimumSize.Width + shift, MinimumSize.Height);
					}
					else                            // without graph
					{
                        _graphWidth = Graph.Width;  // remember width of hidden control
                        shift = _graphWidth + splitContainer.SplitterWidth;
                        // MinimumSize first, Width second
                        MinimumSize = new Size(MinimumSize.Width - shift, MinimumSize.Height);
                        Width -= shift;
					}
                    splitContainer.Panel2Collapsed = !value;
                }
			}
		}

        #endregion
        #region Buttons

        /// <summary>Performes the calculation process.</summary>
        /// <param name="worker">The BackgroundWorker in wich Processing is run.</param>
        private object Make(BackgroundWorker worker)
        {
			string status = DONE;
            _stopWatch.Start();
            try
            {
                Processing prc = new Processing(
                    worker,
                    prgBar,
                    (byte)nmrPattLength.Value,
                    (byte)nmrMismatchCnt.Value,
                    rBtnAllPatterns.Checked,
                    chkBoxTurbo.Visible && chkBoxTurbo.Enabled ? chkBoxTurbo.Checked : false,
                    chkBoxDraw.Checked
                    );
                InputControlsEnabled = false;
                if (IsGlobalTask)
                    _taskSettings[1].Issue = prc.ScanGlobal(
                        (short)nmrShakesCnt.Value,
                        (short)nmrWinStartLength.Value,
                        (short)nmrWinStopLength.Value,
                        (short)nmrWinIncr.Value,
                        (short)nmrWinShift.Value
                        );
                else
                    _taskSettings[0].Issue = prc.ScanLocal(
                        (short)(chkBoxShake.Checked ? nmrShakesCnt.Value : 0),
                        rBtnIndividual.Checked
                        );
                //nmrPattLength
            }
            //catch (ThreadAbortException) { }
            catch (InvalidOperationException e)
            {
                // don't put message when the ThreadAbortException raised throw the InvalidOperationException
                if (e.InnerException == null || e.InnerException.GetType() != typeof(ThreadAbortException))
                    MsgBoxError(e.ToString(), status = ERROR);
            }
            catch (SystemException e)
            {
                MsgBoxError(e.Message, status = ERROR);
            }
            catch (ApplicationException e)
            {
                // Empty string if exception was raised during cancelling by user
                if (e.Message != string.Empty)
                {
                    status = INP_FAULT;
                    MsgBoxError(e.Message, ERROR);
                }
            }
			_stopWatch.Stop();
            stsBarPnlInfo.Text = string.Empty;
            return status;
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            // Do not access the form's BackgroundWorker reference directly.
            // Instead, use the reference provided by the sender parameter.
            BackgroundWorker bw = sender as BackgroundWorker;

            e.Result = Make(bw);

            // If the operation was canceled by the user, set the DoWorkEventArgs.Cancel property to true.
            if (bw.CancellationPending)
                e.Cancel = true;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)            // The user canceled the operation.
            {
                stsBarPnlInfo.Text = string.Empty;
                stsBarPnlStatus.Text = USER_CANCEL;
            }
            else
            {
                if (e.Error == null)    // The operation completed normally.
                {
                    Output();
                    Draw();
                }
                // if the process was canceled by the user, e.Result has not a string value
                stsBarPnlStatus.Text = e.Result as string;
            }
            InputControlsEnabled = true;
        }

        private void btnStart_Click(object sender, System.EventArgs e)
		{
            stsBarPnlInfo.Text = READING;
            if (ReadSequance())
            {
                //Graph.DrawCoverString("IN PROCESS...");
                backgroundWorker.RunWorkerAsync();
            }
            else
                stsBarPnlStatus.Text = READ_CANCEL;
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
            this.backgroundWorker.CancelAsync();
            Graph.Clear();
		}

        private void esc_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == 27)    // 'escape' key
            {
                this.backgroundWorker.CancelAsync();
                Graph.Clear();
            }

        }
        
        private void btnRead_Click(object sender, System.EventArgs e)
		{
            stsBarPnlInfo.Text = READING;
            if ( ( _taskSettings[1].Issue = ReadScanWindows() ) != null)
			{
                Graph.Clear();
                Output(_taskSettings[1].Issue);
                Graph.Mode = Plot.Modes.Graph;
                Draw(_taskSettings[1].Issue);
                //chkBoxZeroVal.Checked = true;       // because zero values are always visible in a file
                stsBarPnlStatus.Text = VIEW + OUTPUTDATA;
                stsBarPnlTime.Text = string.Empty;
			}
			else
				stsBarPnlStatus.Text = READ_CANCEL;
		}

		private void btnSave_Click  (object sender, System.EventArgs e)
		{
            if (rTxtBox.Text.Length > 0)
                btnSave.Enabled = !WriteData();
		}

		private void btnAbout_Click (object sender, System.EventArgs e)
		{
			new AboutForm().ShowDialog();
		}

        private void btnHelp_Click(object sender, EventArgs e)
        {
            try
            {
                Process SysInfo = new Process();
                SysInfo.StartInfo.ErrorDialog = true;
                SysInfo.StartInfo.FileName = helpProvider.HelpNamespace;
                SysInfo.Start();
            }
            catch (Exception)
            {
                // exception is treated by Make() method
            }
        }

        #endregion
        #region tabTask Control

        /// <summary>Manages controls visibility depend on selected tab.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabTask_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (IsGlobalTask)
                chkBoxTurbo.Visible = nmrShakesCnt.Enabled = true;
            else
                chkBoxTurbo.Visible = nmrShakesCnt.Enabled = grpBoxFrequncy.Visible = chkBoxShake.Checked;
            int i = TaskIndex;
            // restore initial tabTask Height for Local
            if (i == 0 && tabTask.Height == _tabTaskHeightMax)
                tabTask.Height -= TabTASK_HEIGHT_SHIFT;
            // set labels and output
            lblOutputText.Text = _taskSettings[i].OutputLabel;
            lblGraphic.Text = _taskSettings[i].GraphicLabel;
            ClearOutputs();
            Graph.Init(_taskSettings[i].AxisXCapture, _taskSettings[i].AxisYCapture);
            Graph.Mode = _taskSettings[i].PlotMode;
            Output(_taskSettings[i].Issue);
            Draw(_taskSettings[i].Issue);
            Text = CaptionText(true);
            btnRead.Visible = IsGlobalTask;
            btnStart.Focus();
        }

        #endregion
        #region Input controls reactions

        private void chkBoxShake_CheckedChanged(object sender, EventArgs e)
        {
            AutoSave();
            chkBoxTurbo.Visible = grpBoxFrequncy.Visible = nmrShakesCnt.Enabled = chkBoxShake.Checked;
        }

        /// <summary>Common method for check Pattern's new parameters.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Pattern_ValueChanged(object sender, System.EventArgs e)
        {
            AutoSave();
            // check if mismatches number greater then length of word
            byte wordLength = (byte)nmrPattLength.Value;
            byte mismatchCount = (byte)nmrMismatchCnt.Value;
            if (wordLength < mismatchCount)
                nmrMismatchCnt.Value = wordLength;
            // check free memory - for Local task only
            long oneMem = Similars.OneSize(wordLength, mismatchCount) + Patterns.OneSize();
            if (!IsGlobalTask)
                oneMem += TrimmedPatterns.OneSize();
            oneMem *= Words.GetCount(wordLength);
            chkBoxTurbo.Enabled = Environment.WorkingSet > oneMem;
            stsBarPnlStatus.Text = string.Format("Mb: {0} / {1}", Environment.WorkingSet / (1024 * 1024), oneMem / (1024 * 1024));
        }

        /// <summary>Common controls method for check if the result should be save.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkToSave_ValueChanged(object sender, EventArgs e)
        {
            AutoSave();
        }

        private void linkLbl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // change label text and tabTask Height
            if (tabTask.Height == _tabTaskHeightMax)
                tabTask.Height -= TabTASK_HEIGHT_SHIFT;
            else
                tabTask.Height += TabTASK_HEIGHT_SHIFT;
        }

        private void linkLbl_Paint(object sender, PaintEventArgs e)
        {
            // rotate dublicated text on 90 degree
            string label = tabTask.Height == _tabTaskHeightMax ? PnlDOWN : PnlUP;
            StringFormat sformat = new StringFormat(StringFormatFlags.DirectionVertical);
            Brush brush = grpBoxFilter.Enabled ? _linkBrush : Brushes.Gray;
            e.Graphics.DrawString(label, linkLbl.Font, brush, new PointF(-1, -1), sformat);
            e.Graphics.DrawString(label, linkLbl.Font, brush, new PointF(-1, 3), sformat);
        }

        #endregion
        #region Output controls reactions

        /// <summary>Forces output issue and unblock 'Save' button.
        /// Auxiliary base method.</summary>
        private void OutputWithUnblockButton()
        {
            Output();
            btnSave.Enabled = true;
        }
        
        private void chkBoxSimilars_CheckedChanged(object sender, System.EventArgs e)
        {
            OutputWithUnblockButton();
            //nmrVisibleCnt.Enabled = chkBoxSimilars.Checked;
        }

        private void nmrVisibleCnt_ValueChanged(object sender, EventArgs e)
        {
            OutputWithUnblockButton();
        }

        private void chkBoxDraw_CheckedChanged(object sender, System.EventArgs e)
		{
			IsGraphVisible = chkBoxDraw.Checked;
		}

        private void chkBoxCritFF_CheckedChanged(object sender, System.EventArgs e)
		{
            OutputWithUnblockButton();
            txtBoxMinFF.Enabled = txtBoxMaxFF.Enabled = chkBoxCritFF.Checked;
        }

		private void chkBoxCritCV_CheckedChanged(object sender, System.EventArgs e)
		{
            OutputWithUnblockButton();
            //txtBoxMinCV.Enabled = txtBoxMaxCV.Enabled = chkBoxCritCV.Checked;
            txtBoxMinCV.Enabled = txtBoxMaxCV.Enabled = ((CheckBox)sender).Checked;
        }

        private void txtBoxFF_TextChanged(object sender, EventArgs e)
        {
            OutputWithUnblockButtonChecked(chkBoxCritFF);
        }

        private void txtBoxCV_TextChanged(object sender, EventArgs e)
        {
            OutputWithUnblockButtonChecked(chkBoxCritCV);
        }

        /// <summary>Forces output second issue depent on Checked status of CheckBox. 
        /// Auxiliary base method.</summary>
        /// <param name="chBox">Controlled CheckBox.</param>
        private void OutputWithUnblockButtonChecked(CheckBox chBox)
        {
            if (chBox.Checked)
                Output(_taskSettings[1].Issue);
            btnSave.Enabled = true;
        }

        /// <summary>Gets the single value from the text represented TextBox control. 
        /// Auxiliary base method.</summary>
		/// <param name="text">The text of the TextBox control.</param>
		/// <param name="isCheck">True if the text is enabled.</param>
		/// <param name="isPositive">True if it may return the positive infinity value.</param>
		/// <returns></returns>
		float	GetSingle	(string text, bool isCheck, bool isPositive)
		{
			return ( isCheck && text != string.Empty && text != "." ) ?
				float.Parse(text) :
				( isPositive ? float.PositiveInfinity : float.NegativeInfinity );
		}

        /// <summary>Gets True if input char is incorrect for the Single image.</summary>
        /// <param name="e">KeyPressEventArgs enclosed input char.</param>
        /// <param name="txtBox">The TextBox control.</param>
        /// <returns></returns>
        bool IsIncorrectSingle(KeyPressEventArgs e, TextBox txtBox)
        {
            char inpchar = e.KeyChar;
            return (!char.IsDigit(inpchar) && inpchar != 8 && inpchar != '.')
                || (inpchar == '.' && txtBox.Text.IndexOf('.') >= 0);
        }

        private void txtBoxFilter_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            e.Handled = IsIncorrectSingle(e, sender as TextBox);
        }
        #endregion
        #region Other controls reactions
        private void MainForm_Resize(object sender, System.EventArgs e)
		{
            Text = CaptionText(this.WindowState != FormWindowState.Minimized);
        }

        //private void sprgBar_Fulfilment(object sender, FTT.FulfilmentEventArgs e)
        //{
        //    if( this.WindowState == FormWindowState.Minimized )
        //        this.Text = (e.Percent >= 0 ? e.Percent.ToString("0% ") : string.Empty) + CaptionText(false);
        //}

        //static int counter = 0;

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // ProgressChanged event is used to move progress bar and to draw a plot at the time,
            // because it is only one event owned to BackgroundWorker class, 
            // but two different events raised by process.
            // In purpose of progress bar moving none of 'e' properties are used.
            // In purpose of drawing e.UserState is initialized by drawed object, 
            // and e.ProgressPercentage is initialized by IsFirstCall booling value for Patterns as drawed object.
            if (e.UserState == null)
            {

                if (e.ProgressPercentage >= 0)
                {
                    if (prgBar.Value != prgBar.Maximum)
                        prgBar.Value++;
                    //else
                    //    Console.Beep(700, 200);
                }
                else
                    stsBarPnlInfo.Text =
                        e.ProgressPercentage == Patterns.TreatSCANNING ? SCANNING : SORTING;
            }
            else
                DrawGraph(e.UserState, Convert.ToBoolean(e.ProgressPercentage));
        }

        private void timerMain_Tick(System.Object sender, System.EventArgs e)
        {
            if (_stopWatch.IsRunning)
            {
                // Get the elapsed time as a TimeSpan value.
                TimeSpan ts = _stopWatch.Elapsed;
                stsBarPnlTime.Text = String.Format("{0:00}:{1:00}:{2:00}",
                    ts.Hours, ts.Minutes, ts.Seconds);
            }
        }
        #endregion
    }
 }
