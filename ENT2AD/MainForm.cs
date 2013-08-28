/*
 * Created by SharpDevelop.
 * User: administrateur
 * Date: 18/06/2013
 * Time: 17:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CsvHelper.Configuration;

namespace ENT2AD
{
	/// <summary>
	/// Description of MainForm.
	/// </summary>
	public partial class MainForm : Form
	{
		public MainForm()
		{
			//
			// The InitializeComponent() call is required for Windows Forms designer support.
			//
			InitializeComponent();
			
			//
			// TODO: Add constructor code after the InitializeComponent() call.
			//
		}
		
		void MainFormLoad(object sender, EventArgs e)
		{
			txtDomain.Text = Domain.GetCurrentDomain().Name;
			txtLogonserver.Text = Environment.GetEnvironmentVariable("logonserver");
			txtProfilePath.Text = AD.GetOUProperty("LDAP://" + AD.GetLdapPathFromOUs(), "desktopProfile");
			txtElevesScript.Text = AD.GetOUProperty("LDAP://" + AD.GetLdapPathFromOUs(AD.ELEVES_ROOT_OU_NAME), "wWWHomePage");
			txtProfsScript.Text = AD.GetOUProperty("LDAP://" + AD.GetLdapPathFromOUs(AD.PROFS_ROOT_OU_NAME), "wWWHomePage");
		}
		
		void Button1Click(object sender, EventArgs e)
		{		
			string fullPath;
			
			using (OpenFileDialog browser = new OpenFileDialog()) {
				browser.ShowDialog();
				fullPath = browser.FileName;
			}
			
			if (!File.Exists(fullPath))
				return;
			
			try
			{
				using(var fileReader = File.OpenText(fullPath))
				{
					// on saute la première ligne qui contient le texte "Extraction des comptes utilisateurs"
					fileReader.ReadLine();
					
					// initialisation d'un objet CsvHelper avec l'accès en lecture sur le fichier CSV
					using(var csvReader = new CsvHelper.CsvReader(fileReader))
					{
						// on enregistre le mappage (en-tête CSV) avec (membres de la classe ENTUser) via ENTUserMap
						csvReader.Configuration.RegisterClassMap(typeof(ENTUserMap));
						
						// on passe en revue tout le fichier CSV
						ENTUsers.Users = csvReader.GetRecords<ENTUser>().ToDictionary(x => x.UID, x => x);
					}
					
					ADTreeView.UpdateList();
					
					label1.Text = string.Format("{0} comptes", ENTUsers.Users.Count);
				}
			}
			catch (IOException ex)
			{
				MessageBox.Show(ex.Message);
			}
			

		}
		
		void Button2Click(object sender, EventArgs e)
		{
			AD.Domain = txtDomain.Text.Trim();
			AD.LogonServer = txtLogonserver.Text.Trim();
			AD.UsersProfile = txtProfilePath.Text.Trim();
			AD.ElevesScript = txtElevesScript.Text.Trim();
			AD.ProfsScript = txtProfsScript.Text.Trim();
			
			label2.Text = "Création des entrées AD";
			AD.BuildENTOuStructure();
			
			label2.Text = "Création des entrées de stockage";
			Storage.BuildENTRootDirectoryStructure(cbxLocalDrives.SelectedItem.ToString());
			
			// crée les comptes via un thread séparé
			accountsCreator.RunWorkerAsync();
		}
		
		void AccountsCreatorDoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
		{
			BackgroundWorker bw = sender as BackgroundWorker;
			int i = 0;
			int added = 0;
			bw.ReportProgress(0, new int[] {0, ENTUsers.Users.Count, 0});
			
			foreach (var ENTUser in ENTUsers.Users.Values)
			{
				if (!ENTUser.CreateAccountAndDirectory())
					added++;
				
				i++;
				bw.ReportProgress(i * 100 / ENTUsers.Users.Count, new int[] {i, ENTUsers.Users.Count, ENTUsers.ADCreationFailed.Count, added});
			}
			
		}
		
		void AccountsCreatorProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
		{
			int[] data = e.UserState as int[];
			
			if (data[0] > 0)
			{
				label2.Text = string.Format("{2} % effectués ({0} comptes traités sur {1}), {3} ajoutés.", data[0], data[1], e.ProgressPercentage, data[3]);
			}
			else
			{
				label2.Text = "Connexion à Active Directory...";
			}
			

		}
		
		void TxtProfilePathTextChanged(object sender, EventArgs e)
		{
			txtFullProfilePath.Text = Path.Combine(txtLogonserver.Text, txtProfilePath.Text);
		}
	}
	

	
	
}
