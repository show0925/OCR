// Neural Network OCR
//
// Copyright © Andrew Kirillov, 2005
// andrew.kirillov@gmail.com
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;

using AForge.Math;
using AForge.NeuralNet;
using AForge.NeuralNet.Learning;

namespace NeuroOCR
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private static string[] fonts = new string[] {"Arial", "Courier", "Tahoma", "Times New Roman", "Verdana"};
		private bool[] regularFonts = new bool[fonts.Length];
		private bool[] italicFonts = new bool[fonts.Length];
		private int[,][] data;
		private Receptors receptors = new Receptors();
		private int initialReceptorsCount = 500;
		private int receptorsCount = 100;
		private Network neuralNet;

		private float	learningRate1 = 1.0f;
		private float	errorLimit1 = 1.0f;
		private float	learningRate2 = 0.2f;
		private float	errorLimit2 = 0.1f;
		private int		learningEpoch = 0;

		private float	error = 0.0f;
		private int		misclassified = 0;

		private DateTime startTime;
		private Thread workerThread;
		private ManualResetEvent stopEvent = null;
		private int workType;

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Button clearButton;
		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox initialReceptorsBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox receptorsBox;
		private System.Windows.Forms.Button generateReceptorsButton;
		private System.ComponentModel.IContainer components;

		private System.Windows.Forms.CheckBox showReceptorsCheck;
		private System.Windows.Forms.GroupBox trainingSetGroup;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox arialCheck;
		private System.Windows.Forms.CheckBox arialItalicCheck;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.CheckBox courierCheck;
		private System.Windows.Forms.CheckBox courierItalicCheck;
		private System.Windows.Forms.CheckBox tahomaCheck;
		private System.Windows.Forms.CheckBox tahomaItalicCheck;
		private System.Windows.Forms.CheckBox timesCheck;
		private System.Windows.Forms.CheckBox timesItalicCheck;
		private System.Windows.Forms.CheckBox verdanaCheck;
		private System.Windows.Forms.CheckBox verdanaItalicCheck;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.ComboBox lettersCombo;
		private System.Windows.Forms.Button drawButton;
		private System.Windows.Forms.ComboBox fontsCombo;
		private System.Windows.Forms.GroupBox groupBox3;
		private System.Windows.Forms.Button filterDataButton;
		private System.Windows.Forms.GroupBox groupBox4;
		private System.Windows.Forms.Button traintNetworkButton;
		private System.Windows.Forms.Button recognizeButton;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox currentReceptorsBox;
		private System.Windows.Forms.Label label12;
		private System.Windows.Forms.ComboBox layersCombo;
		private System.Windows.Forms.Label label13;
		private System.Windows.Forms.TextBox alfaBox;
		private System.Windows.Forms.Button createNetButton;
		private System.Windows.Forms.Label label14;
		private System.Windows.Forms.GroupBox groupBox5;
		private System.Windows.Forms.Label label15;
		private System.Windows.Forms.TextBox timeBox;
		private System.Windows.Forms.Label label16;
		private System.Windows.Forms.TextBox statusBox;
		private System.Windows.Forms.ProgressBar progressBar;
		private System.Windows.Forms.Timer timer;
		private System.Windows.Forms.Button stopButton;
		private System.Windows.Forms.Label label17;
		private System.Windows.Forms.Label label18;
		private System.Windows.Forms.TextBox rate1Box;
		private System.Windows.Forms.Label label19;
		private System.Windows.Forms.TextBox limit1Box;
		private System.Windows.Forms.Label label20;
		private System.Windows.Forms.TextBox limit2Box;
		private System.Windows.Forms.Label label21;
		private System.Windows.Forms.TextBox rate2Box;
		private System.Windows.Forms.Label label22;
		private System.Windows.Forms.Label label23;
		private NeuroOCR.PaintBoard paintBoard;
		private NeuroOCR.GridArray dataGrid;
		private System.Windows.Forms.CheckBox scaleCheck;
		private System.Windows.Forms.Label label24;
		private System.Windows.Forms.TextBox learningEpochBox;
		private System.Windows.Forms.Label label25;
		private System.Windows.Forms.TextBox errorBox;
		private System.Windows.Forms.Label label26;
		private System.Windows.Forms.TextBox outputBox;
		private System.Windows.Forms.TextBox misclassifiedBox;
		private System.Windows.Forms.Label label27;
		private System.Windows.Forms.Button generateDataButton;

		// Constructor
		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			initialReceptorsBox.Text = initialReceptorsCount.ToString();
			receptorsBox.Text = receptorsCount.ToString();
			showReceptorsCheck.Checked = paintBoard.ShowReceptors;
			scaleCheck.Checked = paintBoard.ScaleImage;

			// set receptors collection for the paint board
			paintBoard.Receptors = receptors;
			// generate default receptors collection
			GenerateReceptors();

			// add 26 letters
			for (int i = 0; i < 26; i++)
			{
				lettersCombo.Items.Add((char)((int)'A' + i));
			}
			lettersCombo.SelectedIndex = 0;
			// add all fonts
			for (int i = 0; i < fonts.Length; i++)
			{
				fontsCombo.Items.Add(fonts[i]);
				fontsCombo.Items.Add(string.Format("{0} - Italic", fonts[i]));
			}
			fontsCombo.SelectedIndex = 0;

			UpdateAvailableFonts();

			// set to one layer network
			layersCombo.SelectedIndex = 0;

			//
			alfaBox.Text	= "1.0";
			rate1Box.Text	= learningRate1.ToString();
			limit1Box.Text	= errorLimit1.ToString();
			rate2Box.Text	= learningRate2.ToString();
			limit2Box.Text	= errorLimit2.ToString();
			learningEpochBox.Text = learningEpoch.ToString();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.groupBox1 = new System.Windows.Forms.GroupBox();
			this.scaleCheck = new System.Windows.Forms.CheckBox();
			this.paintBoard = new NeuroOCR.PaintBoard();
			this.clearButton = new System.Windows.Forms.Button();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.showReceptorsCheck = new System.Windows.Forms.CheckBox();
			this.generateReceptorsButton = new System.Windows.Forms.Button();
			this.receptorsBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.initialReceptorsBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label11 = new System.Windows.Forms.Label();
			this.currentReceptorsBox = new System.Windows.Forms.TextBox();
			this.trainingSetGroup = new System.Windows.Forms.GroupBox();
			this.verdanaItalicCheck = new System.Windows.Forms.CheckBox();
			this.verdanaCheck = new System.Windows.Forms.CheckBox();
			this.timesItalicCheck = new System.Windows.Forms.CheckBox();
			this.timesCheck = new System.Windows.Forms.CheckBox();
			this.tahomaItalicCheck = new System.Windows.Forms.CheckBox();
			this.tahomaCheck = new System.Windows.Forms.CheckBox();
			this.courierItalicCheck = new System.Windows.Forms.CheckBox();
			this.courierCheck = new System.Windows.Forms.CheckBox();
			this.arialItalicCheck = new System.Windows.Forms.CheckBox();
			this.arialCheck = new System.Windows.Forms.CheckBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label6 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.label9 = new System.Windows.Forms.Label();
			this.label10 = new System.Windows.Forms.Label();
			this.lettersCombo = new System.Windows.Forms.ComboBox();
			this.drawButton = new System.Windows.Forms.Button();
			this.fontsCombo = new System.Windows.Forms.ComboBox();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.dataGrid = new NeuroOCR.GridArray();
			this.filterDataButton = new System.Windows.Forms.Button();
			this.generateDataButton = new System.Windows.Forms.Button();
			this.groupBox4 = new System.Windows.Forms.GroupBox();
			this.label27 = new System.Windows.Forms.Label();
			this.misclassifiedBox = new System.Windows.Forms.TextBox();
			this.outputBox = new System.Windows.Forms.TextBox();
			this.label26 = new System.Windows.Forms.Label();
			this.errorBox = new System.Windows.Forms.TextBox();
			this.label25 = new System.Windows.Forms.Label();
			this.learningEpochBox = new System.Windows.Forms.TextBox();
			this.label24 = new System.Windows.Forms.Label();
			this.label23 = new System.Windows.Forms.Label();
			this.limit2Box = new System.Windows.Forms.TextBox();
			this.label21 = new System.Windows.Forms.Label();
			this.rate2Box = new System.Windows.Forms.TextBox();
			this.label22 = new System.Windows.Forms.Label();
			this.label20 = new System.Windows.Forms.Label();
			this.limit1Box = new System.Windows.Forms.TextBox();
			this.label19 = new System.Windows.Forms.Label();
			this.rate1Box = new System.Windows.Forms.TextBox();
			this.label18 = new System.Windows.Forms.Label();
			this.label17 = new System.Windows.Forms.Label();
			this.createNetButton = new System.Windows.Forms.Button();
			this.alfaBox = new System.Windows.Forms.TextBox();
			this.label13 = new System.Windows.Forms.Label();
			this.layersCombo = new System.Windows.Forms.ComboBox();
			this.label12 = new System.Windows.Forms.Label();
			this.recognizeButton = new System.Windows.Forms.Button();
			this.traintNetworkButton = new System.Windows.Forms.Button();
			this.label14 = new System.Windows.Forms.Label();
			this.groupBox5 = new System.Windows.Forms.GroupBox();
			this.stopButton = new System.Windows.Forms.Button();
			this.progressBar = new System.Windows.Forms.ProgressBar();
			this.statusBox = new System.Windows.Forms.TextBox();
			this.label16 = new System.Windows.Forms.Label();
			this.timeBox = new System.Windows.Forms.TextBox();
			this.label15 = new System.Windows.Forms.Label();
			this.timer = new System.Windows.Forms.Timer(this.components);
			this.groupBox1.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.trainingSetGroup.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.groupBox4.SuspendLayout();
			this.groupBox5.SuspendLayout();
			this.SuspendLayout();
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.scaleCheck,
																					this.paintBoard,
																					this.clearButton});
			this.groupBox1.Location = new System.Drawing.Point(10, 5);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(195, 205);
			this.groupBox1.TabIndex = 1;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Drawing area";
			// 
			// scaleCheck
			// 
			this.scaleCheck.Location = new System.Drawing.Point(105, 175);
			this.scaleCheck.Name = "scaleCheck";
			this.scaleCheck.Size = new System.Drawing.Size(55, 24);
			this.scaleCheck.TabIndex = 4;
			this.scaleCheck.Text = "Scale";
			this.scaleCheck.CheckedChanged += new System.EventHandler(this.scaleCheck_CheckedChanged);
			// 
			// paintBoard
			// 
			this.paintBoard.Location = new System.Drawing.Point(10, 20);
			this.paintBoard.Name = "paintBoard";
			this.paintBoard.Receptors = null;
			this.paintBoard.Size = new System.Drawing.Size(150, 150);
			this.paintBoard.TabIndex = 3;
			// 
			// clearButton
			// 
			this.clearButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.clearButton.Location = new System.Drawing.Point(10, 175);
			this.clearButton.Name = "clearButton";
			this.clearButton.Size = new System.Drawing.Size(60, 23);
			this.clearButton.TabIndex = 2;
			this.clearButton.Text = "&Clear";
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.showReceptorsCheck,
																					this.generateReceptorsButton,
																					this.receptorsBox,
																					this.label2,
																					this.initialReceptorsBox,
																					this.label1,
																					this.label11,
																					this.currentReceptorsBox});
			this.groupBox2.Location = new System.Drawing.Point(10, 220);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(195, 135);
			this.groupBox2.TabIndex = 3;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Receptors";
			// 
			// showReceptorsCheck
			// 
			this.showReceptorsCheck.Location = new System.Drawing.Point(10, 100);
			this.showReceptorsCheck.Name = "showReceptorsCheck";
			this.showReceptorsCheck.Size = new System.Drawing.Size(57, 24);
			this.showReceptorsCheck.TabIndex = 5;
			this.showReceptorsCheck.Text = "&Show";
			this.showReceptorsCheck.CheckedChanged += new System.EventHandler(this.showReceptorsCheck_CheckedChanged);
			// 
			// generateReceptorsButton
			// 
			this.generateReceptorsButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.generateReceptorsButton.Location = new System.Drawing.Point(80, 100);
			this.generateReceptorsButton.Name = "generateReceptorsButton";
			this.generateReceptorsButton.Size = new System.Drawing.Size(60, 23);
			this.generateReceptorsButton.TabIndex = 4;
			this.generateReceptorsButton.Text = "&Generate";
			this.generateReceptorsButton.Click += new System.EventHandler(this.generateReceptorsButton_Click);
			// 
			// receptorsBox
			// 
			this.receptorsBox.Location = new System.Drawing.Point(80, 45);
			this.receptorsBox.Name = "receptorsBox";
			this.receptorsBox.Size = new System.Drawing.Size(60, 20);
			this.receptorsBox.TabIndex = 3;
			this.receptorsBox.Text = "";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(10, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(37, 16);
			this.label2.TabIndex = 2;
			this.label2.Text = "Filter:";
			// 
			// initialReceptorsBox
			// 
			this.initialReceptorsBox.Location = new System.Drawing.Point(80, 20);
			this.initialReceptorsBox.Name = "initialReceptorsBox";
			this.initialReceptorsBox.Size = new System.Drawing.Size(60, 20);
			this.initialReceptorsBox.TabIndex = 1;
			this.initialReceptorsBox.Text = "";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(10, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(42, 15);
			this.label1.TabIndex = 0;
			this.label1.Text = "Initial:";
			// 
			// label11
			// 
			this.label11.Location = new System.Drawing.Point(10, 73);
			this.label11.Name = "label11";
			this.label11.Size = new System.Drawing.Size(50, 14);
			this.label11.TabIndex = 7;
			this.label11.Text = "Current:";
			// 
			// currentReceptorsBox
			// 
			this.currentReceptorsBox.Location = new System.Drawing.Point(80, 70);
			this.currentReceptorsBox.Name = "currentReceptorsBox";
			this.currentReceptorsBox.ReadOnly = true;
			this.currentReceptorsBox.Size = new System.Drawing.Size(60, 20);
			this.currentReceptorsBox.TabIndex = 7;
			this.currentReceptorsBox.Text = "";
			// 
			// trainingSetGroup
			// 
			this.trainingSetGroup.BackColor = System.Drawing.SystemColors.Control;
			this.trainingSetGroup.Controls.AddRange(new System.Windows.Forms.Control[] {
																						   this.verdanaItalicCheck,
																						   this.verdanaCheck,
																						   this.timesItalicCheck,
																						   this.timesCheck,
																						   this.tahomaItalicCheck,
																						   this.tahomaCheck,
																						   this.courierItalicCheck,
																						   this.courierCheck,
																						   this.arialItalicCheck,
																						   this.arialCheck,
																						   this.label7,
																						   this.label6,
																						   this.label5,
																						   this.label4,
																						   this.label3,
																						   this.label8,
																						   this.label9,
																						   this.label10,
																						   this.lettersCombo,
																						   this.drawButton,
																						   this.fontsCombo});
			this.trainingSetGroup.Location = new System.Drawing.Point(10, 365);
			this.trainingSetGroup.Name = "trainingSetGroup";
			this.trainingSetGroup.Size = new System.Drawing.Size(195, 175);
			this.trainingSetGroup.TabIndex = 4;
			this.trainingSetGroup.TabStop = false;
			this.trainingSetGroup.Text = "Training set";
			// 
			// verdanaItalicCheck
			// 
			this.verdanaItalicCheck.Location = new System.Drawing.Point(162, 115);
			this.verdanaItalicCheck.Name = "verdanaItalicCheck";
			this.verdanaItalicCheck.Size = new System.Drawing.Size(16, 16);
			this.verdanaItalicCheck.TabIndex = 14;
			this.verdanaItalicCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// verdanaCheck
			// 
			this.verdanaCheck.Location = new System.Drawing.Point(120, 115);
			this.verdanaCheck.Name = "verdanaCheck";
			this.verdanaCheck.Size = new System.Drawing.Size(16, 16);
			this.verdanaCheck.TabIndex = 13;
			this.verdanaCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// timesItalicCheck
			// 
			this.timesItalicCheck.Location = new System.Drawing.Point(162, 95);
			this.timesItalicCheck.Name = "timesItalicCheck";
			this.timesItalicCheck.Size = new System.Drawing.Size(16, 16);
			this.timesItalicCheck.TabIndex = 12;
			this.timesItalicCheck.Text = "checkBox1";
			this.timesItalicCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// timesCheck
			// 
			this.timesCheck.Location = new System.Drawing.Point(120, 95);
			this.timesCheck.Name = "timesCheck";
			this.timesCheck.Size = new System.Drawing.Size(16, 16);
			this.timesCheck.TabIndex = 11;
			this.timesCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// tahomaItalicCheck
			// 
			this.tahomaItalicCheck.Location = new System.Drawing.Point(162, 75);
			this.tahomaItalicCheck.Name = "tahomaItalicCheck";
			this.tahomaItalicCheck.Size = new System.Drawing.Size(16, 16);
			this.tahomaItalicCheck.TabIndex = 10;
			this.tahomaItalicCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// tahomaCheck
			// 
			this.tahomaCheck.Location = new System.Drawing.Point(120, 75);
			this.tahomaCheck.Name = "tahomaCheck";
			this.tahomaCheck.Size = new System.Drawing.Size(16, 16);
			this.tahomaCheck.TabIndex = 9;
			this.tahomaCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// courierItalicCheck
			// 
			this.courierItalicCheck.Location = new System.Drawing.Point(162, 55);
			this.courierItalicCheck.Name = "courierItalicCheck";
			this.courierItalicCheck.Size = new System.Drawing.Size(16, 16);
			this.courierItalicCheck.TabIndex = 8;
			this.courierItalicCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// courierCheck
			// 
			this.courierCheck.Location = new System.Drawing.Point(120, 55);
			this.courierCheck.Name = "courierCheck";
			this.courierCheck.Size = new System.Drawing.Size(16, 16);
			this.courierCheck.TabIndex = 7;
			this.courierCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// arialItalicCheck
			// 
			this.arialItalicCheck.Location = new System.Drawing.Point(162, 35);
			this.arialItalicCheck.Name = "arialItalicCheck";
			this.arialItalicCheck.Size = new System.Drawing.Size(16, 16);
			this.arialItalicCheck.TabIndex = 6;
			this.arialItalicCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// arialCheck
			// 
			this.arialCheck.Checked = true;
			this.arialCheck.CheckState = System.Windows.Forms.CheckState.Checked;
			this.arialCheck.Location = new System.Drawing.Point(120, 35);
			this.arialCheck.Name = "arialCheck";
			this.arialCheck.Size = new System.Drawing.Size(16, 16);
			this.arialCheck.TabIndex = 5;
			this.arialCheck.CheckedChanged += new System.EventHandler(this.fontCheck_CheckedChanged);
			// 
			// label7
			// 
			this.label7.Location = new System.Drawing.Point(10, 115);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(51, 15);
			this.label7.TabIndex = 4;
			this.label7.Text = "Verdana";
			// 
			// label6
			// 
			this.label6.Location = new System.Drawing.Point(10, 75);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(55, 17);
			this.label6.TabIndex = 3;
			this.label6.Text = "Tahoma";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(10, 95);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(105, 13);
			this.label5.TabIndex = 2;
			this.label5.Text = "Times New Roman";
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(10, 55);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(50, 15);
			this.label4.TabIndex = 1;
			this.label4.Text = "Courier";
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(10, 35);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(27, 13);
			this.label3.TabIndex = 0;
			this.label3.Text = "Arial";
			// 
			// label8
			// 
			this.label8.BackColor = System.Drawing.SystemColors.Info;
			this.label8.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label8.Location = new System.Drawing.Point(105, 15);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(46, 16);
			this.label8.TabIndex = 5;
			this.label8.Text = "Regular";
			// 
			// label9
			// 
			this.label9.BackColor = System.Drawing.SystemColors.Info;
			this.label9.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label9.Location = new System.Drawing.Point(155, 15);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(30, 16);
			this.label9.TabIndex = 5;
			this.label9.Text = "Italic";
			// 
			// label10
			// 
			this.label10.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label10.Location = new System.Drawing.Point(10, 135);
			this.label10.Name = "label10";
			this.label10.Size = new System.Drawing.Size(175, 2);
			this.label10.TabIndex = 5;
			// 
			// lettersCombo
			// 
			this.lettersCombo.Location = new System.Drawing.Point(10, 145);
			this.lettersCombo.Name = "lettersCombo";
			this.lettersCombo.Size = new System.Drawing.Size(35, 21);
			this.lettersCombo.TabIndex = 5;
			// 
			// drawButton
			// 
			this.drawButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.drawButton.Location = new System.Drawing.Point(144, 145);
			this.drawButton.Name = "drawButton";
			this.drawButton.Size = new System.Drawing.Size(41, 21);
			this.drawButton.TabIndex = 5;
			this.drawButton.Text = "Draw";
			this.drawButton.Click += new System.EventHandler(this.drawButton_Click);
			// 
			// fontsCombo
			// 
			this.fontsCombo.Location = new System.Drawing.Point(50, 145);
			this.fontsCombo.Name = "fontsCombo";
			this.fontsCombo.Size = new System.Drawing.Size(90, 21);
			this.fontsCombo.TabIndex = 5;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.dataGrid,
																					this.filterDataButton,
																					this.generateDataButton});
			this.groupBox3.Location = new System.Drawing.Point(215, 5);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(325, 435);
			this.groupBox3.TabIndex = 5;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Learning data";
			// 
			// dataGrid
			// 
			this.dataGrid.AutoSizeMinHeight = 10;
			this.dataGrid.AutoSizeMinWidth = 10;
			this.dataGrid.AutoStretchColumnsToFitWidth = false;
			this.dataGrid.AutoStretchRowsToFitHeight = false;
			this.dataGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.dataGrid.ContextMenuStyle = SourceGrid2.ContextMenuStyle.None;
			this.dataGrid.GridToolTipActive = true;
			this.dataGrid.Location = new System.Drawing.Point(10, 20);
			this.dataGrid.Name = "dataGrid";
			this.dataGrid.Size = new System.Drawing.Size(305, 375);
			this.dataGrid.SpecialKeys = SourceGrid2.GridSpecialKeys.Default;
			this.dataGrid.TabIndex = 3;
			// 
			// filterDataButton
			// 
			this.filterDataButton.Enabled = false;
			this.filterDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.filterDataButton.Location = new System.Drawing.Point(240, 405);
			this.filterDataButton.Name = "filterDataButton";
			this.filterDataButton.TabIndex = 2;
			this.filterDataButton.Text = "Filter Data";
			this.filterDataButton.Click += new System.EventHandler(this.filterDataButton_Click);
			// 
			// generateDataButton
			// 
			this.generateDataButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.generateDataButton.Location = new System.Drawing.Point(130, 405);
			this.generateDataButton.Name = "generateDataButton";
			this.generateDataButton.Size = new System.Drawing.Size(97, 23);
			this.generateDataButton.TabIndex = 1;
			this.generateDataButton.Text = "Generate Data";
			this.generateDataButton.Click += new System.EventHandler(this.generateDataButton_Click);
			// 
			// groupBox4
			// 
			this.groupBox4.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.label27,
																					this.misclassifiedBox,
																					this.outputBox,
																					this.label26,
																					this.errorBox,
																					this.label25,
																					this.learningEpochBox,
																					this.label24,
																					this.label23,
																					this.limit2Box,
																					this.label21,
																					this.rate2Box,
																					this.label22,
																					this.label20,
																					this.limit1Box,
																					this.label19,
																					this.rate1Box,
																					this.label18,
																					this.label17,
																					this.createNetButton,
																					this.alfaBox,
																					this.label13,
																					this.layersCombo,
																					this.label12,
																					this.recognizeButton,
																					this.traintNetworkButton,
																					this.label14});
			this.groupBox4.Location = new System.Drawing.Point(550, 5);
			this.groupBox4.Name = "groupBox4";
			this.groupBox4.Size = new System.Drawing.Size(195, 435);
			this.groupBox4.TabIndex = 6;
			this.groupBox4.TabStop = false;
			this.groupBox4.Text = "Neural network";
			// 
			// label27
			// 
			this.label27.Location = new System.Drawing.Point(10, 343);
			this.label27.Name = "label27";
			this.label27.Size = new System.Drawing.Size(75, 14);
			this.label27.TabIndex = 26;
			this.label27.Text = "Misclassified:";
			// 
			// misclassifiedBox
			// 
			this.misclassifiedBox.Location = new System.Drawing.Point(95, 340);
			this.misclassifiedBox.Name = "misclassifiedBox";
			this.misclassifiedBox.ReadOnly = true;
			this.misclassifiedBox.Size = new System.Drawing.Size(90, 20);
			this.misclassifiedBox.TabIndex = 25;
			this.misclassifiedBox.Text = "";
			// 
			// outputBox
			// 
			this.outputBox.Location = new System.Drawing.Point(135, 378);
			this.outputBox.Name = "outputBox";
			this.outputBox.ReadOnly = true;
			this.outputBox.Size = new System.Drawing.Size(50, 20);
			this.outputBox.TabIndex = 24;
			this.outputBox.Text = "";
			// 
			// label26
			// 
			this.label26.Location = new System.Drawing.Point(10, 380);
			this.label26.Name = "label26";
			this.label26.Size = new System.Drawing.Size(127, 14);
			this.label26.TabIndex = 23;
			this.label26.Text = "Network thinks that it is:";
			// 
			// errorBox
			// 
			this.errorBox.Location = new System.Drawing.Point(95, 315);
			this.errorBox.Name = "errorBox";
			this.errorBox.ReadOnly = true;
			this.errorBox.Size = new System.Drawing.Size(90, 20);
			this.errorBox.TabIndex = 22;
			this.errorBox.Text = "";
			this.errorBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label25
			// 
			this.label25.Location = new System.Drawing.Point(10, 318);
			this.label25.Name = "label25";
			this.label25.Size = new System.Drawing.Size(85, 14);
			this.label25.TabIndex = 21;
			this.label25.Text = "Summary error:";
			// 
			// learningEpochBox
			// 
			this.learningEpochBox.Location = new System.Drawing.Point(10, 160);
			this.learningEpochBox.Name = "learningEpochBox";
			this.learningEpochBox.TabIndex = 20;
			this.learningEpochBox.Text = "";
			// 
			// label24
			// 
			this.label24.Location = new System.Drawing.Point(10, 115);
			this.label24.Name = "label24";
			this.label24.Size = new System.Drawing.Size(173, 40);
			this.label24.TabIndex = 19;
			this.label24.Text = "Number of learning epoch (0 for learning until specified error limit is reached):" +
				"";
			// 
			// label23
			// 
			this.label23.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label23.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label23.Location = new System.Drawing.Point(10, 370);
			this.label23.Name = "label23";
			this.label23.Size = new System.Drawing.Size(175, 2);
			this.label23.TabIndex = 18;
			// 
			// limit2Box
			// 
			this.limit2Box.Location = new System.Drawing.Point(145, 250);
			this.limit2Box.Name = "limit2Box";
			this.limit2Box.Size = new System.Drawing.Size(40, 20);
			this.limit2Box.TabIndex = 17;
			this.limit2Box.Text = "";
			// 
			// label21
			// 
			this.label21.Location = new System.Drawing.Point(94, 253);
			this.label21.Name = "label21";
			this.label21.Size = new System.Drawing.Size(56, 14);
			this.label21.TabIndex = 16;
			this.label21.Text = "Error limit:";
			// 
			// rate2Box
			// 
			this.rate2Box.Location = new System.Drawing.Point(45, 250);
			this.rate2Box.Name = "rate2Box";
			this.rate2Box.Size = new System.Drawing.Size(40, 20);
			this.rate2Box.TabIndex = 15;
			this.rate2Box.Text = "";
			// 
			// label22
			// 
			this.label22.Location = new System.Drawing.Point(10, 253);
			this.label22.Name = "label22";
			this.label22.Size = new System.Drawing.Size(32, 14);
			this.label22.TabIndex = 14;
			this.label22.Text = "Rate:";
			// 
			// label20
			// 
			this.label20.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
			this.label20.Location = new System.Drawing.Point(10, 230);
			this.label20.Name = "label20";
			this.label20.Size = new System.Drawing.Size(100, 14);
			this.label20.TabIndex = 13;
			this.label20.Text = "Second pass:";
			// 
			// limit1Box
			// 
			this.limit1Box.Location = new System.Drawing.Point(145, 205);
			this.limit1Box.Name = "limit1Box";
			this.limit1Box.Size = new System.Drawing.Size(40, 20);
			this.limit1Box.TabIndex = 12;
			this.limit1Box.Text = "";
			// 
			// label19
			// 
			this.label19.Location = new System.Drawing.Point(94, 208);
			this.label19.Name = "label19";
			this.label19.Size = new System.Drawing.Size(56, 14);
			this.label19.TabIndex = 11;
			this.label19.Text = "Error limit:";
			// 
			// rate1Box
			// 
			this.rate1Box.Location = new System.Drawing.Point(45, 205);
			this.rate1Box.Name = "rate1Box";
			this.rate1Box.Size = new System.Drawing.Size(40, 20);
			this.rate1Box.TabIndex = 10;
			this.rate1Box.Text = "";
			// 
			// label18
			// 
			this.label18.Location = new System.Drawing.Point(10, 208);
			this.label18.Name = "label18";
			this.label18.Size = new System.Drawing.Size(32, 14);
			this.label18.TabIndex = 9;
			this.label18.Text = "Rate:";
			// 
			// label17
			// 
			this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
			this.label17.Location = new System.Drawing.Point(10, 185);
			this.label17.Name = "label17";
			this.label17.Size = new System.Drawing.Size(60, 14);
			this.label17.TabIndex = 8;
			this.label17.Text = "First pass:";
			// 
			// createNetButton
			// 
			this.createNetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.createNetButton.Location = new System.Drawing.Point(90, 75);
			this.createNetButton.Name = "createNetButton";
			this.createNetButton.Size = new System.Drawing.Size(95, 23);
			this.createNetButton.TabIndex = 4;
			this.createNetButton.Text = "Create Network";
			this.createNetButton.Click += new System.EventHandler(this.createNetButton_Click);
			// 
			// alfaBox
			// 
			this.alfaBox.Location = new System.Drawing.Point(95, 45);
			this.alfaBox.Name = "alfaBox";
			this.alfaBox.Size = new System.Drawing.Size(90, 20);
			this.alfaBox.TabIndex = 3;
			this.alfaBox.Text = "";
			// 
			// label13
			// 
			this.label13.Location = new System.Drawing.Point(10, 48);
			this.label13.Name = "label13";
			this.label13.Size = new System.Drawing.Size(80, 14);
			this.label13.TabIndex = 2;
			this.label13.Text = "Sigmoid`s Alfa:";
			// 
			// layersCombo
			// 
			this.layersCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.layersCombo.Items.AddRange(new object[] {
															 "One",
															 "Two"});
			this.layersCombo.Location = new System.Drawing.Point(95, 20);
			this.layersCombo.Name = "layersCombo";
			this.layersCombo.Size = new System.Drawing.Size(90, 21);
			this.layersCombo.TabIndex = 1;
			// 
			// label12
			// 
			this.label12.Location = new System.Drawing.Point(10, 23);
			this.label12.Name = "label12";
			this.label12.Size = new System.Drawing.Size(72, 16);
			this.label12.TabIndex = 0;
			this.label12.Text = "Layers count:";
			// 
			// recognizeButton
			// 
			this.recognizeButton.Enabled = false;
			this.recognizeButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.recognizeButton.Location = new System.Drawing.Point(90, 405);
			this.recognizeButton.Name = "recognizeButton";
			this.recognizeButton.Size = new System.Drawing.Size(95, 23);
			this.recognizeButton.TabIndex = 6;
			this.recognizeButton.Text = "Recognize";
			this.recognizeButton.Click += new System.EventHandler(this.recognizeButton_Click);
			// 
			// traintNetworkButton
			// 
			this.traintNetworkButton.Enabled = false;
			this.traintNetworkButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.traintNetworkButton.Location = new System.Drawing.Point(90, 280);
			this.traintNetworkButton.Name = "traintNetworkButton";
			this.traintNetworkButton.Size = new System.Drawing.Size(95, 23);
			this.traintNetworkButton.TabIndex = 5;
			this.traintNetworkButton.Text = "Train Network";
			this.traintNetworkButton.Click += new System.EventHandler(this.traintNetworkButton_Click);
			// 
			// label14
			// 
			this.label14.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.label14.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.label14.Location = new System.Drawing.Point(10, 105);
			this.label14.Name = "label14";
			this.label14.Size = new System.Drawing.Size(175, 2);
			this.label14.TabIndex = 7;
			// 
			// groupBox5
			// 
			this.groupBox5.Controls.AddRange(new System.Windows.Forms.Control[] {
																					this.stopButton,
																					this.progressBar,
																					this.statusBox,
																					this.label16,
																					this.timeBox,
																					this.label15});
			this.groupBox5.Location = new System.Drawing.Point(215, 445);
			this.groupBox5.Name = "groupBox5";
			this.groupBox5.Size = new System.Drawing.Size(530, 95);
			this.groupBox5.TabIndex = 7;
			this.groupBox5.TabStop = false;
			this.groupBox5.Text = "Progress";
			// 
			// stopButton
			// 
			this.stopButton.Enabled = false;
			this.stopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.stopButton.Location = new System.Drawing.Point(228, 63);
			this.stopButton.Name = "stopButton";
			this.stopButton.TabIndex = 5;
			this.stopButton.Text = "Stop";
			this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
			// 
			// progressBar
			// 
			this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.progressBar.Location = new System.Drawing.Point(10, 45);
			this.progressBar.Name = "progressBar";
			this.progressBar.Size = new System.Drawing.Size(510, 10);
			this.progressBar.TabIndex = 4;
			// 
			// statusBox
			// 
			this.statusBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right);
			this.statusBox.Location = new System.Drawing.Point(250, 20);
			this.statusBox.Name = "statusBox";
			this.statusBox.ReadOnly = true;
			this.statusBox.Size = new System.Drawing.Size(270, 20);
			this.statusBox.TabIndex = 3;
			this.statusBox.Text = "";
			// 
			// label16
			// 
			this.label16.Location = new System.Drawing.Point(200, 23);
			this.label16.Name = "label16";
			this.label16.Size = new System.Drawing.Size(43, 17);
			this.label16.TabIndex = 2;
			this.label16.Text = "Status:";
			// 
			// timeBox
			// 
			this.timeBox.Location = new System.Drawing.Point(90, 20);
			this.timeBox.Name = "timeBox";
			this.timeBox.ReadOnly = true;
			this.timeBox.Size = new System.Drawing.Size(80, 20);
			this.timeBox.TabIndex = 1;
			this.timeBox.Text = "";
			this.timeBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// label15
			// 
			this.label15.Location = new System.Drawing.Point(10, 23);
			this.label15.Name = "label15";
			this.label15.Size = new System.Drawing.Size(78, 19);
			this.label15.TabIndex = 0;
			this.label15.Text = "Time elapsed:";
			// 
			// timer
			// 
			this.timer.Tick += new System.EventHandler(this.timer_Tick);
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(754, 550);
			this.Controls.AddRange(new System.Windows.Forms.Control[] {
																		  this.groupBox5,
																		  this.groupBox4,
																		  this.groupBox3,
																		  this.trainingSetGroup,
																		  this.groupBox2,
																		  this.groupBox1});
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Location = new System.Drawing.Point(120, 0);
			this.MaximizeBox = false;
			this.Name = "MainForm";
			this.Text = "Neural Network OCR";
			this.groupBox1.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.trainingSetGroup.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox4.ResumeLayout(false);
			this.groupBox5.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new MainForm());
		}

		// On "Clear" button click - clear the drawing area
		private void clearButton_Click(object sender, System.EventArgs e)
		{
			paintBoard.ClearImage();
		}

		// On "Generate" button click - generate receptors
		private void generateReceptorsButton_Click(object sender, System.EventArgs e)
		{
			GetReceptorsCount();

			// generate
			GenerateReceptors();

			// disable train and recognize buttons
			traintNetworkButton.Enabled = false;
			recognizeButton.Enabled = false;
		}

		// Get recpetors count
		private void GetReceptorsCount()
		{
			try
			{
				initialReceptorsCount = Math.Max(25, Math.Min(5000, int.Parse(initialReceptorsBox.Text)));
				receptorsCount = Math.Max(10, Math.Min(initialReceptorsCount, int.Parse(receptorsBox.Text)));
			}
			catch (Exception)
			{
				initialReceptorsCount = 500;
				receptorsCount = 100;
			}
			initialReceptorsBox.Text = initialReceptorsCount.ToString();
			receptorsBox.Text = receptorsCount.ToString();
		}

		// Generate receptors
		private void GenerateReceptors()
		{
			// remove previous receptors
			receptors.Clear();
			// set reception area size
			receptors.AreaSize = paintBoard.AreaSize;
			// generate new receptors
			receptors.Generate(initialReceptorsCount);
			// set current receptors count
			currentReceptorsBox.Text = initialReceptorsCount.ToString();

			paintBoard.Invalidate();
		}

		// Show/Hide receptors
		private void showReceptorsCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			paintBoard.ShowReceptors = showReceptorsCheck.Checked;
		}

		// On fonts check box changed - enable/disable font
		private void fontCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			UpdateAvailableFonts();		
		}

		// Update available fonts
		private void UpdateAvailableFonts()
		{
			regularFonts[0]	= arialCheck.Checked;
			regularFonts[1]	= courierCheck.Checked;
			regularFonts[2]	= tahomaCheck.Checked;
			regularFonts[3]	= timesCheck.Checked;
			regularFonts[4]	= verdanaCheck.Checked;

			italicFonts[0]	= arialItalicCheck.Checked;
			italicFonts[1]	= courierItalicCheck.Checked;
			italicFonts[2]	= tahomaItalicCheck.Checked;
			italicFonts[3]	= timesItalicCheck.Checked;
			italicFonts[4]	= verdanaItalicCheck.Checked;

			generateDataButton.Enabled =
				regularFonts[0] | regularFonts[1] | regularFonts[2] | regularFonts[3] | regularFonts[4] |
				italicFonts[0] | italicFonts[1] | italicFonts[2] | italicFonts[3] | italicFonts[4];
		}

		// Draw letter
		private void drawButton_Click(object sender, System.EventArgs e)
		{
			paintBoard.DrawLetter((char)((int) 'A' + lettersCombo.SelectedIndex),
				fonts[fontsCombo.SelectedIndex >> 1], 90, ((fontsCombo.SelectedIndex & 1) != 0));
		}

		// Display work pregress
		private void ReportProgress(int step, string message)
		{
			// display progress
			if (progressBar.Visible)
				progressBar.Value = step;

			// display message
			if (message != null)
				statusBox.Text = message;

			// display elapsed time
			TimeSpan elapsed = DateTime.Now.Subtract(startTime);

			timeBox.Text = string.Format("{0}:{1}:{2}",
				elapsed.Hours.ToString("D2"),
				elapsed.Minutes.ToString("D2"),
				elapsed.Seconds.ToString("D2"));
			timeBox.Invalidate();
		}

		// Sart work
		private void StartWork(bool stopable)
		{
			// set busy cursor
			this.Cursor = Cursors.WaitCursor;

			//
			stopButton.Enabled = stopable;
			if (stopable)
			{
				// create events
				stopEvent = new ManualResetEvent(false);
				stopButton.Capture = true;
			}
			else
			{
				this.Capture = true;
			}

			// generate data in separate thread
			startTime = DateTime.Now;

			// start timer
			timer.Start();
		}

		// Stop work
		private void StopWork()
		{
			statusBox.Text = string.Empty;
			timeBox.Text = string.Empty;
			progressBar.Value = 0;

			// set default cursor
			this.Cursor = Cursors.Default;

			// stop timer
			timer.Stop();

			// release event
			if (stopEvent != null)
			{
				stopEvent.Close();
				stopEvent = null;
				stopButton.Capture = false;
			}
			else
			{
				this.Capture = false;
			}
		}

		// On timer
		private void timer_Tick(object sender, System.EventArgs e)
		{
			if (!workerThread.IsAlive)
			{
				switch (workType)
				{
					case 0:		// generate data
						// show learning data
						ShowLearningData();

						// enable data filtering
						filterDataButton.Enabled = true;

						// disable train and recognize buttons
						traintNetworkButton.Enabled = false;
						recognizeButton.Enabled = false;

						break;

					case 1:		// training network
						// last error
						if (data != null)
						{
							errorBox.Text = error.ToString();
							misclassifiedBox.Text = string.Format("{0} / {1}",
								misclassified, data.GetLength(0) * data[0, 0].Length);
						}
						break;
				}

				// stop work
				StopWork();
			}
		}

		// On "Generate Data" button click
		private void generateDataButton_Click(object sender, System.EventArgs e)
		{
			workType = 0;
			progressBar.Show();

			// start work
			StartWork(false);

			// set status message
			statusBox.Text = "Generating data ...";

			// create and start new thread
			workerThread = new Thread(new ThreadStart(GenerateLearningData));
			// start thread
			workerThread.Start();
		}

		// On "Filter Data" button click
		private void filterDataButton_Click(object sender, System.EventArgs e)
		{
			// set busy cursor
			this.Cursor = Cursors.WaitCursor;

			GetReceptorsCount();

			// filter data
			RemoveLearningDuplicates();
			FilterLearningData();
			ShowLearningData();

			// set receptors for paint board
			paintBoard.Receptors = receptors;
			paintBoard.Invalidate();

			// set current receptors count
			currentReceptorsBox.Text = receptors.Count.ToString();

			// disable train and recognize buttons
			traintNetworkButton.Enabled = false;
			recognizeButton.Enabled = false;

			// set default cursor
			this.Cursor = Cursors.Default;
		}

		// Generate data for learning
		private void GenerateLearningData()
		{
			int		objectsCount = 26;
			int		featuresCount = receptors.Count;
			int		variantsCount = 0;
			int		fontsCount = fonts.Length * 2;
			int		i, j, k, font, v = 0, step = 0;
			bool	italic;

			// count variants
			for (i = 0; i < fonts.Length; i++)
			{
				variantsCount += (regularFonts[i]) ? 1 : 0;
				variantsCount += (italicFonts[i]) ? 1 : 0;
			}

			if (variantsCount == 0)
				return;

			// set progress size
			progressBar.Maximum = objectsCount * variantsCount;

			// create data array
			data = new int [objectsCount, featuresCount][];
			// init each data element
			for (i = 0; i < objectsCount; i++)
			{
				for (j = 0; j < featuresCount; j++)
				{
					data[i, j] = new int[variantsCount];
				}
			}

			// fill data ...

			// for all fonts
			for (j = 0; j < fontsCount; j++)
			{
				font	= j >> 1;
				italic	= ((j & 1) != 0);

				// skip disabled fonts
				if (((italic) && (!italicFonts[font])) ||
					((!italic) && (!regularFonts[font])))
				{
					continue;
				}

				// for all objects
				for (i = 0; i < objectsCount; i++)
				{
					// draw letter
					paintBoard.DrawLetter((char)((int) 'A' + i), fonts[font], 90, italic, false);

					// get receptors state
					int[] state = receptors.GetReceptorsState(paintBoard.GetImage(false));

					// copy receptors state
					for (k = 0; k < featuresCount; k++)
					{
						data[i, k][v] = state[k];
					}

					// show progress
					ReportProgress(++step, null);
				}

				v++;
			}

			// clear paint area
			paintBoard.ClearImage();
		}

		// Display learning data in grid
		private void ShowLearningData()
		{
			if (data == null)
				return;

			int		objectsCount = data.GetLength(0);
			int		featuresCount = data.GetLength(1);
			int		variantsCount = data[0, 0].Length;
			int		i, j, k;
			int[]	item;

			// string data
			string[,]	strData = new string[objectsCount, featuresCount];
			char[]		ch = new char[variantsCount];

			for (i = 0; i < objectsCount; i++)
			{
				for (j = 0; j < featuresCount; j++)
				{
					item = data[i, j];

					for (k = 0; k < variantsCount; k++)
					{
						ch[k] = (char)((int)'0' + item[k]);
					}
					strData[i, j] = new string(ch);
				}
			}

			// show data
			dataGrid.LoadData(strData);
		}

		// Remove duplicates from learning data and receptors,
		// which produced these duplicates
		private void RemoveLearningDuplicates()
		{
			if (data == null)
				return;

			int		objectsCount = data.GetLength(0);
			int		featuresCount = data.GetLength(1);
			int		variantsCount = data[0, 0].Length;

			int		i, j, k, s;
			int[]	item;

			// calculate checksum of each object for each receptor
			int[,]	checkSum = new int[objectsCount, featuresCount];

			// for each object
			for (i = 0; i < objectsCount; i++)
			{
				// for each receptor
				for (j = 0; j < featuresCount; j++)
				{
					item = data[i, j];
					s = 0;

					// for each variant
					for (k = 0; k < variantsCount; k++)
					{
						s |= item[k] << k;
					}

					checkSum[i, j] = s;
				}
			}

			// find which receptors should be removed
			bool[]	remove = new bool[featuresCount];

			// walk through all receptors ...
			for (i = 0; i < featuresCount - 1; i++)
			{
				// skip receptors alredy marked as deleted
				if (remove[i] == true)
					continue;

				// ... and compare each receptor with others
				for (j = i + 1; j < featuresCount; j++)
				{
					// remove by default
					remove[j] = true;

					// compare cheksums of all objects
					for (k = 0; k < objectsCount; k++)
					{
						if (checkSum[k, i] != checkSum[k, j])
						{
							// ups, they are different, do not delete it
							remove[j] = false;
							break;
						}
					}
				}
			}

			// count receptors to save
			int receptorsToSave = 0;
			for (i = 0; i < featuresCount; i++)
				receptorsToSave += (remove[i]) ? 0 : 1;

			// filter data removing receptors with usability below acceptable
			int[,][] newData = new int [objectsCount, receptorsToSave][];
			Receptors newReceptors = new Receptors();

			k = 0;
			// for all receptors
			for (j = 0; j < featuresCount; j++)
			{
				if (remove[j])
					continue;

				// for all objects
				for (i = 0; i < objectsCount; i++)
				{
					newData[i, k] = data[i, j];
				}
				newReceptors.Add(receptors[j]);
				k++;
			}

			// set new data
			data = newData;
			receptors = newReceptors;
		}

		// Filter learning data
		private void FilterLearningData()
		{
			if (data == null)
				return;

			// data filtering is performed by removing bad receptors

			int		objectsCount = data.GetLength(0);
			int		featuresCount = data.GetLength(1);
			int		variantsCount = data[0, 0].Length;
			int		i, j, k, v;
			int[]	item;

			// maybe we already filtered ?
			// so check that new receptors count is not greater than we have
			if (receptorsCount >= featuresCount)
				return;

			int[]	outerCounters = new int[2];
			int[]	innerCounters = new int[2];
			double	ie, oe;

			double[] usabilities = new double[featuresCount];

			// for all receptors
			for (j = 0; j < featuresCount; j++)
			{
				// clear outer counters
				Array.Clear(outerCounters, 0, 2);

				ie = 0;
				// for all objects
				for (i = 0; i < objectsCount; i++)
				{
					// clear inner counters
					Array.Clear(innerCounters, 0, 2);
					// get variants item
					item = data[i, j];

					// for all variants
					for (k = 0; k < variantsCount; k++)
					{
						v = item[k];

						innerCounters[v]++;
						outerCounters[v]++;
					}

					// callculate inner entropy of receptor for current object
					ie += Statistics.Entropy(innerCounters, variantsCount);
				}

				// average inner entropy
				ie /= objectsCount;
				// outer entropy
				oe = Statistics.Entropy(outerCounters, objectsCount * variantsCount);
				// receptors usability
				usabilities[j] = (1.0 - ie) * oe;
			}

			// create usabilities copy and sort it
			double[] temp = (double[]) usabilities.Clone();
			Array.Sort(temp);
			// get acceptable usability for receptor
			double accaptableUsability = temp[featuresCount - receptorsCount];

			// filter data removing receptors with usability below acceptable
			int[,][] newData = new int [objectsCount, receptorsCount][];
			Receptors newReceptors = new Receptors();

			k = 0;
			// for all receptors
			for (j = 0; j < featuresCount; j++)
			{
				if (usabilities[j] < accaptableUsability)
					continue;

				// for all objects
				for (i = 0; i < objectsCount; i++)
				{
					newData[i, k] = data[i, j];
				}
				newReceptors.Add(receptors[j]);

				if (++k == receptorsCount)
					break;
			}

			// set new data
			data = newData;
			receptors = newReceptors;
		}

		// On "Create Network" button
		private void createNetButton_Click(object sender, System.EventArgs e)
		{
			CreateNetwork();
	
			// enable train and recognize buttons
			traintNetworkButton.Enabled = true;
			recognizeButton.Enabled = true;

			//
			errorBox.Text = string.Empty;
			misclassifiedBox.Text = string.Empty;
			outputBox.Text = string.Empty;
		}

		// Create Network
		private void CreateNetwork()
		{
			if (data == null)
				return;

			int		objectsCount = data.GetLength(0);
			int		featuresCount = data.GetLength(1);
			float	alfa;

			// get alfa value
			try
			{
				alfa = Math.Max(0.1f, Math.Min(10.0f, float.Parse(alfaBox.Text)));
			}
			catch (Exception)
			{
				alfa = 1.0f;
			}
			alfaBox.Text = alfa.ToString();

			// creare network
			if (layersCombo.SelectedIndex == 0)
			{
				neuralNet = new Network(new BipolarSigmoidFunction(alfa), featuresCount, objectsCount);
			}
			else
			{
				neuralNet = new Network(new BipolarSigmoidFunction(alfa), featuresCount, objectsCount, objectsCount);
			}

			// randomize network`s weights
			neuralNet.Randomize();
		}


		// On "Traing" button click
		private void traintNetworkButton_Click(object sender, System.EventArgs e)
		{
			outputBox.Text = string.Empty;

			// get parameters
			try
			{
				learningEpoch	= Math.Max(0, int.Parse(learningEpochBox.Text));
				learningRate1	= Math.Max(0.0001f, Math.Min(10.0f, float.Parse(rate1Box.Text)));
				errorLimit1		= Math.Max(0.0001f, Math.Min(1000.0f, float.Parse(limit1Box.Text)));
				learningRate2	= Math.Max(0.0001f, Math.Min(10.0f, float.Parse(rate2Box.Text)));
				errorLimit2		= Math.Max(0.0001f, Math.Min(1000.0f, float.Parse(limit2Box.Text)));
			}
			catch (Exception)
			{
				learningEpoch	= 0;
				learningRate1	= 1.0f;
				errorLimit1		= 1.0f;
				learningRate2	= 0.2f;
				errorLimit2		= 0.1f;
			}
			learningEpochBox.Text = learningEpoch.ToString();
			rate1Box.Text	= learningRate1.ToString();
			limit1Box.Text	= errorLimit1.ToString();
			rate2Box.Text	= learningRate2.ToString();
			limit2Box.Text	= errorLimit2.ToString();

			//
			workType = 1;
			progressBar.Hide();

			// start work
			StartWork(true);

			// set status message
			statusBox.Text = "Training network ...";

			// create and start new thread
			workerThread = new Thread(new ThreadStart(TrainNetwork));
			// start thread
			workerThread.Start();
		}

		// Traing neural network to recognize our training set
		private void TrainNetwork()
		{
			if (data == null)
				return;

			int		objectsCount = data.GetLength(0);
			int		featuresCount = data.GetLength(1);
			int		variantsCount = data[0, 0].Length;
			int		i, j, k, n;

			// generate possible outputs
			float[][]	possibleOutputs = new float[objectsCount][];

			for (i = 0; i < objectsCount; i++)
			{
				possibleOutputs[i] = new float[objectsCount];
				for (j = 0; j < objectsCount; j++)
				{
					possibleOutputs[i][j] = (i == j) ? 0.5f : -0.5f;
				}
			}

			// generate network training data
			float[][]	input = new float [objectsCount * variantsCount][];
			float[][]	output = new float [objectsCount * variantsCount][];
			float[]		ins;
			
			// for all varaints
			for (j = 0, n = 0; j < variantsCount; j++)
			{
				// for all objects
				for (i = 0; i < objectsCount; i++, n++)
				{
					// prepare input
					input[n] = ins = new float[featuresCount];

					// for each receptor
					for (k = 0; k < featuresCount; k++)
					{
						ins[k] = (float) data[i, k][j] - 0.5f;
					}

					// set output
					output[n] = possibleOutputs[i];
				}
			}

			System.Diagnostics.Debug.WriteLine("--- learning started");

			// create network teacher
			BackPropagationLearning	teacher = new BackPropagationLearning(neuralNet);

			// First pass
			teacher.LearningLimit	= errorLimit1;
			teacher.LearningRate	= learningRate1;

			i = 0;
			// learn
			do
			{
				error = teacher.LearnEpoch(input, output);
				i++;

				// report status
				if ((i % 100) == 0)
				{
					ReportProgress(0, string.Format("Learning, 1st pass ... (iterations: {0}, error: {1})",
						i, error));
				}

				// need to stop ?
				if (stopEvent.WaitOne(0, true))
					break;
			}
			while (((learningEpoch == 0) && (!teacher.IsConverged)) ||
				((learningEpoch != 0) && (i < learningEpoch)));

			System.Diagnostics.Debug.WriteLine("first pass: " + i + ", error = " + error);

			// skip second pass, if learning epoch number was specified
			if (learningEpoch == 0)
			{
				// Second pass
				teacher = new BackPropagationLearning(neuralNet);
			
				teacher.LearningLimit	= errorLimit2;
				teacher.LearningRate	= learningRate2;

				// learn
				do
				{
					error = teacher.LearnEpoch(input, output);
					i++;

					// report status
					if ((i % 100) == 0)
					{
						ReportProgress(0, string.Format("Learning, 2nd pass ... (iterations: {0}, error: {1})",
							i, error));
					}

					// need to stop ?
					if (stopEvent.WaitOne(0, true))
						break;
				}
				while (!teacher.IsConverged);

				System.Diagnostics.Debug.WriteLine("second pass: " + i + ", error = " + error);
			}

			// get the misclassified value
			misclassified = 0;
			// for all training patterns
			for (i = 0, n = input.Length; i < n; i++)
			{
				float[]	realOutput = neuralNet.Compute(input[i]);
				float[]	desiredOutput = output[i];
				int		maxIndex1 = 0;
				int		maxIndex2 = 0;
				float	max1 = realOutput[0];
				float	max2 = desiredOutput[0];

				for (j = 1, k = realOutput.Length; j < k; j++)
				{
					if (realOutput[j] > max1)
					{
						max1 = realOutput[j];
						maxIndex1 = j;
					}
					if (desiredOutput[j] > max2)
					{
						max2 = desiredOutput[j];
						maxIndex2 = j;
					}
				}

				if (maxIndex1 != maxIndex2)
					misclassified++;
			}
		}

		// On "Recognize" button click
		private void recognizeButton_Click(object sender, System.EventArgs e)
		{
			int		i, n, maxIndex = 0;

			// get current receptors state
			int[]	state = receptors.GetReceptorsState(paintBoard.GetImage());

			// for network input
			float[]	input = new float[state.Length];

			for (i = 0; i < state.Length; i++)
				input[i] = (float) state[i] - 0.5f;

			// compute network and get it's ouput
			float[]	output = neuralNet.Compute(input);

			// find the maximum from output
			float max = output[0];
			for (i = 1, n = output.Length; i < n; i++)
			{
				if (output[i] > max)
				{
					max = output[i];
					maxIndex = i;
				}
			}

			//
			outputBox.Text = string.Format("{0}", (char)((int) 'A' + maxIndex));
		}

		// On "Stop" button click - stop the work
		private void stopButton_Click(object sender, System.EventArgs e)
		{
			if (stopEvent != null)
				stopEvent.Set();
		}

		// On "Scale" checkbox changed
		private void scaleCheck_CheckedChanged(object sender, System.EventArgs e)
		{
			paintBoard.ScaleImage = scaleCheck.Checked;
		}
	}
}
