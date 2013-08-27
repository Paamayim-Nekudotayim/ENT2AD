/*
 * Created by SharpDevelop.
 * User: administrateur
 * Date: 18/06/2013
 * Time: 17:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
namespace ENT2AD
{
	partial class MainForm
	{
		/// <summary>
		/// Designer variable used to keep track of non-visual components.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		
		/// <summary>
		/// Disposes resources used by the form.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				if (components != null) {
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
		
		/// <summary>
		/// This method is required for Windows Forms designer support.
		/// Do not change the method contents inside the source code editor. The Forms designer might
		/// not be able to load this method if it was changed manually.
		/// </summary>
		private void InitializeComponent()
		{
			this.ADTreeView = new ENT2AD.ADTreeView();
			this.openCsv = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.accountsCreator = new System.ComponentModel.BackgroundWorker();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.txtDomain = new System.Windows.Forms.TextBox();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.txtLogonserver = new System.Windows.Forms.TextBox();
			this.cbxLocalDrives = new ENT2AD.UI.DrivesComboBox();
			this.label5 = new System.Windows.Forms.Label();
			this.txtProfilePath = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.txtFullProfilePath = new System.Windows.Forms.TextBox();
			this.label7 = new System.Windows.Forms.Label();
			this.label8 = new System.Windows.Forms.Label();
			this.txtElevesScript = new System.Windows.Forms.TextBox();
			this.label9 = new System.Windows.Forms.Label();
			this.txtProfsScript = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ADTreeView
			// 
			this.ADTreeView.Location = new System.Drawing.Point(12, 42);
			this.ADTreeView.Name = "ADTreeView";
			this.ADTreeView.Size = new System.Drawing.Size(346, 319);
			this.ADTreeView.TabIndex = 0;
			// 
			// openCsv
			// 
			this.openCsv.Location = new System.Drawing.Point(13, 13);
			this.openCsv.Name = "openCsv";
			this.openCsv.Size = new System.Drawing.Size(117, 23);
			this.openCsv.TabIndex = 1;
			this.openCsv.Text = "Ouvrir CSV";
			this.openCsv.UseVisualStyleBackColor = true;
			this.openCsv.Click += new System.EventHandler(this.Button1Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(374, 251);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(138, 23);
			this.button2.TabIndex = 2;
			this.button2.Text = "Créer dans AD";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.Button2Click);
			// 
			// accountsCreator
			// 
			this.accountsCreator.WorkerReportsProgress = true;
			this.accountsCreator.DoWork += new System.ComponentModel.DoWorkEventHandler(this.AccountsCreatorDoWork);
			this.accountsCreator.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.AccountsCreatorProgressChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(12, 376);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(35, 13);
			this.label1.TabIndex = 3;
			this.label1.Text = "label1";
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(374, 307);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(35, 13);
			this.label2.TabIndex = 4;
			this.label2.Text = "label2";
			// 
			// txtDomain
			// 
			this.txtDomain.Location = new System.Drawing.Point(492, 68);
			this.txtDomain.Name = "txtDomain";
			this.txtDomain.Size = new System.Drawing.Size(269, 20);
			this.txtDomain.TabIndex = 5;
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(374, 71);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(49, 13);
			this.label3.TabIndex = 6;
			this.label3.Text = "Domaine";
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(374, 97);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(44, 13);
			this.label4.TabIndex = 7;
			this.label4.Text = "Serveur";
			// 
			// txtLogonserver
			// 
			this.txtLogonserver.Location = new System.Drawing.Point(492, 94);
			this.txtLogonserver.Name = "txtLogonserver";
			this.txtLogonserver.Size = new System.Drawing.Size(269, 20);
			this.txtLogonserver.TabIndex = 8;
			// 
			// cbxLocalDrives
			// 
			this.cbxLocalDrives.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxLocalDrives.FormattingEnabled = true;
			this.cbxLocalDrives.Location = new System.Drawing.Point(492, 39);
			this.cbxLocalDrives.Name = "cbxLocalDrives";
			this.cbxLocalDrives.Size = new System.Drawing.Size(269, 21);
			this.cbxLocalDrives.TabIndex = 9;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(374, 42);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(102, 13);
			this.label5.TabIndex = 10;
			this.label5.Text = "Disque de stockage";
			// 
			// txtProfilePath
			// 
			this.txtProfilePath.Location = new System.Drawing.Point(492, 121);
			this.txtProfilePath.Name = "txtProfilePath";
			this.txtProfilePath.Size = new System.Drawing.Size(269, 20);
			this.txtProfilePath.TabIndex = 11;
			this.txtProfilePath.TextChanged += new System.EventHandler(this.TxtProfilePathTextChanged);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(374, 124);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(82, 13);
			this.label6.TabIndex = 13;
			this.label6.Text = "Chemin du profil";
			// 
			// txtFullProfilePath
			// 
			this.txtFullProfilePath.Location = new System.Drawing.Point(492, 147);
			this.txtFullProfilePath.Name = "txtFullProfilePath";
			this.txtFullProfilePath.ReadOnly = true;
			this.txtFullProfilePath.Size = new System.Drawing.Size(269, 20);
			this.txtFullProfilePath.TabIndex = 14;
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(374, 150);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(82, 13);
			this.label7.TabIndex = 15;
			this.label7.Text = "Chemin complet";
			// 
			// label8
			// 
			this.label8.AutoSize = true;
			this.label8.Location = new System.Drawing.Point(374, 180);
			this.label8.Name = "label8";
			this.label8.Size = new System.Drawing.Size(68, 13);
			this.label8.TabIndex = 16;
			this.label8.Text = "Script élèves";
			// 
			// txtElevesScript
			// 
			this.txtElevesScript.Location = new System.Drawing.Point(492, 177);
			this.txtElevesScript.Name = "txtElevesScript";
			this.txtElevesScript.Size = new System.Drawing.Size(269, 20);
			this.txtElevesScript.TabIndex = 17;
			// 
			// label9
			// 
			this.label9.AutoSize = true;
			this.label9.Location = new System.Drawing.Point(374, 205);
			this.label9.Name = "label9";
			this.label9.Size = new System.Drawing.Size(60, 13);
			this.label9.TabIndex = 19;
			this.label9.Text = "Script profs";
			// 
			// txtProfsScript
			// 
			this.txtProfsScript.Location = new System.Drawing.Point(492, 202);
			this.txtProfsScript.Name = "txtProfsScript";
			this.txtProfsScript.Size = new System.Drawing.Size(269, 20);
			this.txtProfsScript.TabIndex = 21;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(778, 403);
			this.Controls.Add(this.txtProfsScript);
			this.Controls.Add(this.label9);
			this.Controls.Add(this.txtElevesScript);
			this.Controls.Add(this.label8);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.txtFullProfilePath);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.txtProfilePath);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.cbxLocalDrives);
			this.Controls.Add(this.txtLogonserver);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.txtDomain);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.openCsv);
			this.Controls.Add(this.ADTreeView);
			this.Name = "MainForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "ENT2AD";
			this.Load += new System.EventHandler(this.MainFormLoad);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		private System.Windows.Forms.TextBox txtProfsScript;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.TextBox txtElevesScript;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox txtFullProfilePath;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox txtProfilePath;
		private System.Windows.Forms.Label label5;
		private ENT2AD.UI.DrivesComboBox cbxLocalDrives;
		private System.Windows.Forms.TextBox txtLogonserver;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.TextBox txtDomain;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.ComponentModel.BackgroundWorker accountsCreator;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button openCsv;
		private ENT2AD.ADTreeView ADTreeView;
	}
}
