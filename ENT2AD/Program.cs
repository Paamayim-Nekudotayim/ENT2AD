/*
 * Created by SharpDevelop.
 * User: administrateur
 * Date: 18/06/2013
 * Time: 17:48
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.DirectoryServices;
using System.IO;
using System.Windows.Forms;

namespace ENT2AD
{
	/// <summary>
	/// Class with program entry point.
	/// </summary>
	internal sealed class Program
	{
		/// <summary>
		/// Program entry point.
		/// </summary>
		[STAThread]
		private static void Main(string[] args)
		{
			using (DirectoryEntry deRoot = new DirectoryEntry("LDAP://RootDSE"))
			{
				if (deRoot == null) {
					MessageBox.Show("Impossible d'accéder à la racine Active Directory (RootDSE is null).");
					return;
				}
				
				AD.NamingContext = deRoot.Properties["defaultNamingContext"].Value.ToString();
			}
			
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainForm());
		}
		
	}
}
