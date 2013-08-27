/*
 * Crée par SharpDevelop.
 * Utilisateur: administrateur
 * Date: 11/07/2013
 * Heure: 09:13
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace ENT2AD.UI
{
	/// <summary>
	/// Description of DrivesComboBox.
	/// </summary>
	public class DrivesComboBox : ComboBox
	{
		public DrivesComboBox()
		{
			if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
				return;			
			
			Items.Clear();
			long availableFreeSpace = 0;
			foreach (DriveInfo Drive in DriveInfo.GetDrives())
			{
				if (Drive.DriveType == DriveType.Fixed && Drive.DriveFormat == "NTFS")
				{
					Items.Add(Drive.Name);
					if (Drive.AvailableFreeSpace >= availableFreeSpace)
					{
						SelectedItem = Drive.Name;
						availableFreeSpace = Drive.AvailableFreeSpace;
					}
				}
			}
		}
	}
}
