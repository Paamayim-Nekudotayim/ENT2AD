/*
 * Crée par SharpDevelop.
 * Utilisateur: administrateur
 * Date: 02/07/2013
 * Heure: 10:15
 * 
 * Pour changer ce modèle utiliser Outils | Options | Codage | Editer les en-têtes standards.
 */
using System;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Management;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ENT2AD
{
	/// <summary>
	/// Permet de manipuler l'espace de stockage pour les données physiques (répertoires, documents) des utilisateurs ENT.
	/// </summary>
	public static class Storage
	{
		private const string ROOT_DIRNAME = "ENT_Root";
		private const string ELEVES_ROOT_DIRNAME = "Eleves";
		private const string PROFS_ROOT_DIRNAME = "Profs";
		private const string CLASSE_COMMONROOTDIR_DIRNAME = "_Classe_";
		private const string COMMUN_DIRNAME = "Commun";
		private const string RESSOURCES_DIRNAME = "Ressources";
		private const string TRAVAUX_DIRNAME = "Travaux";
		
		public static string RootDrive { get; set; }
		public static string RootDirectory { get; set; }
		
		public static string ElevesRootDirectory
		{
			get
			{
				return Path.Combine(RootDirectory, ELEVES_ROOT_DIRNAME);
			}
		}
		
		public static void SetRootDirectory(string drive)
		{
			RootDrive = drive;
			RootDirectory = CombineRootAndPath(RootDrive, ROOT_DIRNAME);
			
			// définition des règles d'accès pour chaque SID
			FileSystemAccessRule builtinAdministrators = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, WindowsIdentity.GetCurrent().User.AccountDomainSid),
																	 FileSystemRights.FullControl,
																	 InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
																	 PropagationFlags.None,
																	 AccessControlType.Allow);
			
			FileSystemAccessRule localSystem = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, WindowsIdentity.GetCurrent().User.AccountDomainSid),
																	 FileSystemRights.FullControl,
																	 InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
																	 PropagationFlags.None,
																	 AccessControlType.Allow);
			
			FileSystemAccessRule creatorOwner = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.CreatorOwnerSid, WindowsIdentity.GetCurrent().User.AccountDomainSid),
																	 FileSystemRights.FullControl,
																	 InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
																	 PropagationFlags.InheritOnly,
																	 AccessControlType.Allow);
			
			// création d'un object de sécurité vide auquel on ajoute les règles créées précédemment
			DirectorySecurity dirSec = new DirectorySecurity();
			dirSec.AddAccessRule(builtinAdministrators);
			dirSec.AddAccessRule(localSystem);
			dirSec.AddAccessRule(creatorOwner);
			
			// création du dossier racine ENT_Root
			Directory.CreateDirectory(RootDirectory, dirSec);
		}
		
		public static void BuildENTRootDirectoryStructure(string localRootDrive)
		{
			SetRootDirectory(localRootDrive);
			Storage.CreateAndShareDirectory(ELEVES_ROOT_DIRNAME);
			Storage.CreateAndShareDirectory(PROFS_ROOT_DIRNAME);
		}
		
		public static void CreateAndShareDirectory(string name)
		{
			string path = Path.Combine(RootDirectory, name);
			
			DirectoryInfo rootSubDir = Directory.CreateDirectory(path);
			DirectorySecurity dirSec = new DirectorySecurity();
			
			// permet de cocher la case "Inclure les autorisations pouvant être héritées du parent"
			dirSec.SetAccessRuleProtection(false, true);
			rootSubDir.SetAccessControl(dirSec);
			
			if (!IsShared(path))
				ShareFolder(path, name + "$");
		}
		
		public static void CreateUserDirectory(ENTUser ENTUser)
		{
			string path;
			
			if (ENTUser.IsPupil) {
				// Eleves\1S1
				path = Path.Combine(ELEVES_ROOT_DIRNAME, ENTUser.Division);
				
				// D:\ENT_Root\Eleves\1S1\_Classe_
				string classeRootPath = Path.Combine(RootDirectory, path, CLASSE_COMMONROOTDIR_DIRNAME);
				
				if (!Directory.Exists(classeRootPath))
				{
					CreateRootClasseDirectories(classeRootPath, ENTUser.Division);
				}
			}
			else
			{
				path = PROFS_ROOT_DIRNAME;
			}
						
			string userDirPath = Path.Combine(RootDirectory, path, ENTUser.SAMAccountName);
			
			FileSystemAccessRule userRule = new FileSystemAccessRule(ENTUser.SAMAccountName,
			                                                         FileSystemRights.Modify,
			                                                         InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
			                                                         PropagationFlags.None,
			                                                         AccessControlType.Allow);
			
			DirectorySecurity dirSec = new DirectorySecurity();
			dirSec.AddAccessRule(userRule);
			
			DirectoryInfo userDir = Directory.CreateDirectory(userDirPath, dirSec);
			
			// permet de cocher la case "Inclure les autorisations pouvant être héritées du parent"
			dirSec.SetAccessRuleProtection(false, true);
			userDir.SetAccessControl(dirSec);
			
			Directory.CreateDirectory(Path.Combine(userDirPath, "Documents"));
		}
		
		public static void MoveUserDirectory(string samaccountname, string oldDivision, string newDivision)
		{
			// D:\ENT_Root\Eleves\1S1\_Classe_
			string classeRootPath = Path.Combine(ElevesRootDirectory, newDivision, CLASSE_COMMONROOTDIR_DIRNAME);
			
			if (!Directory.Exists(classeRootPath))
			{
				CreateRootClasseDirectories(classeRootPath, newDivision);
			}
			
			Directory.Move(Path.Combine(ElevesRootDirectory, oldDivision, samaccountname),
			               Path.Combine(ElevesRootDirectory, newDivision, samaccountname));
		}
		
		private static void CreateRootClasseDirectories(string classeRootPath, string division)
		{
			// nouvelle règle pour autoriser la lecture au groupe de tous les élèves de la classe (par ex. "ENT_Classe_1S1")
			FileSystemAccessRule canRead = new FileSystemAccessRule(AD.CLASSE_GROUPNAME_PREFIX + division,
			                                                         FileSystemRights.ReadAndExecute,
			                                                         InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
			                                                         PropagationFlags.None,
			                                                         AccessControlType.Allow);
			
			// nouvelle règle pour autoriser la lecture et l'écriture au groupe de tous les élèves de la classe (par ex. "ENT_Classe_1S1")
			FileSystemAccessRule canModify = new FileSystemAccessRule(AD.CLASSE_GROUPNAME_PREFIX + division,
			                                                         FileSystemRights.Modify,
			                                                         InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
			                                                         PropagationFlags.None,
			                                                         AccessControlType.Allow);
			
			// création du répertoire "D:\ENT_Root\Eleves\1S1\_Classe_"
			Directory.CreateDirectory(classeRootPath);
			
			// récupération de la liste des droits du dossier qu'on vient de créer "D:\ENT_Root\Eleves\1S1\_Classe_"
			DirectorySecurity dirSec = Directory.GetAccessControl(classeRootPath);
			
			// ajout de la règle "lecture" à la liste des droits du dossier qu'on vient de créer "D:\ENT_Root\Eleves\1S1\_Classe_"
			dirSec.AddAccessRule(canRead);
			
			// création du dossier "D:\ENT_Root\Eleves\1S1\_Classe_\Ressources" avec les nouveaux droits de lecture pour le groupe de la classe
			Directory.CreateDirectory(Path.Combine(classeRootPath, RESSOURCES_DIRNAME), dirSec);
			
			// ajout de la règle "modification" à la liste des droits du dossier qu'on vient de créer "D:\ENT_Root\Eleves\1S1\_Classe_"
			dirSec.AddAccessRule(canModify);									
			
			// création du dossier "D:\ENT_Root\Eleves\1S1\_Classe_\Commun" avec les nouveaux droits de modification pour le groupe de la classe
			Directory.CreateDirectory(Path.Combine(classeRootPath, COMMUN_DIRNAME), dirSec);
			
			// création du dossier "D:\ENT_Root\Eleves\1S1\_Classe_\Travaux" avec les nouveaux droits de modification pour le groupe de la classe
			Directory.CreateDirectory(Path.Combine(classeRootPath, TRAVAUX_DIRNAME), dirSec);
		}
		
		/// <summary>
		/// Combines the root and path to ensure the path always relative to the root and not below it or in some other root.
		/// This does not check if the resulting path exists or if access is allowed.
		/// Path can not contain ".." anywhere in the path. Path can not be rooted, it must be a relative path.
		/// </summary>
		/// <param name="root"></param>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string CombineRootAndPath(string root, string path)
		{
			// Path can not be rooted. Must be realitive.
			// Path can not contain ".." anywhere.
			if ( root == null )
			return null;
			
			if ( path == null )
			return null;
			
			if ( ! Path.IsPathRooted(root) )
			return null;
			
			if ( ! root.EndsWith(@"\"))
			root = root + @"\";
			
			path = path.Trim();
			
			if ( Path.IsPathRooted(path) )
			return null;
			
			string fullPath = Path.Combine(root, path);
			// Final test to make sure nothing unexpected in path would Combine
			// to produce something outside the root.
			if ( ! fullPath.StartsWith(root) )
			return null;
			
			if ( path.Contains("..") )
			return null;
			
			return fullPath;
		}

		private static void SetUserDirectoryRights(string path, string samaccountname)
		{
			FileSystemAccessRule userRule = new FileSystemAccessRule(samaccountname,
			                                                         FileSystemRights.Modify,
			                                                         InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
			                                                         PropagationFlags.None,
			                                                         AccessControlType.Allow);
						
			DirectorySecurity dirSec = Directory.GetAccessControl(Directory.GetParent(path).ToString());
			dirSec.AddAccessRule(userRule);
			Directory.SetAccessControl(path, dirSec);
		}
		
		private static void ShareFolder(string path, string name)
		{
			    // Create a ManagementClass object			
			    ManagementClass managementClass = new ManagementClass("Win32_Share");
			
			    // Create ManagementBaseObjects for in and out parameters			
			    ManagementBaseObject inParams = managementClass.GetMethodParameters("Create");
			
			    ManagementBaseObject outParams;
			
			    // Set the input parameters			
//			    inParams["Description"] = description;			
			    inParams["Name"] = name;			
			    inParams["Path"] = path;			
			    inParams["Type"] = 0x0; // Disk Drive
			
			    //Another Type:
			    // DISK_DRIVE = 0x0
			    // PRINT_QUEUE = 0x1
			    // DEVICE = 0x2
			    // IPC = 0x3
			    // DISK_DRIVE_ADMIN = 0x80000000
			    // PRINT_QUEUE_ADMIN = 0x80000001
			    // DEVICE_ADMIN = 0x80000002
			    // IPC_ADMIN = 0x8000003
			
			    //inParams["MaximumAllowed"] = int maxConnectionsNum;
			
			    
				SecurityIdentifier sid = new SecurityIdentifier(WellKnownSidType.WorldSid, WindowsIdentity.GetCurrent().User.AccountDomainSid);
				byte[] sidArray = new byte[sid.BinaryLength];
				sid.GetBinaryForm(sidArray, 0);
				
				ManagementObject everyoneTrustee = new ManagementClass("Win32_Trustee");
				everyoneTrustee["Domain"] = null;
				everyoneTrustee["Name"] = "Everyone";
				everyoneTrustee["SID"] = sidArray;
				
				ManagementObject userACE = new ManagementClass("Win32_Ace");
				userACE["AccessMask"] = 2032127; //Full access
				userACE["AceFlags"] = AceFlags.ObjectInherit | AceFlags.ContainerInherit;
				userACE["AceType"] = AceType.AccessAllowed;
				userACE["Trustee"] = everyoneTrustee; 
				
				ManagementObject securityDescriptor = new ManagementClass("Win32_SecurityDescriptor");
				securityDescriptor["ControlFlags"] = 4; //SE_DACL_PRESENT 
				securityDescriptor["DACL"] = new object[] { userACE };
				
				inParams["Access"] = securityDescriptor;

			    
			    
			    
			    
			    // Invoke the method on the ManagementClass object			
			    outParams = managementClass.InvokeMethod("Create", inParams, null);
			
			    // Check to see if the method invocation was successful			
//			    if ((uint) (outParams.Properties["ReturnValue"].Value) != 0)
//			    {
//			        throw new Exception("Unable to share directory.");
//			    }			
		}
		
		private static bool IsShared(string path)
		{
			ManagementClass mc = new ManagementClass("Win32_Share");
			ManagementObjectCollection moc = mc.GetInstances();
			
			foreach (ManagementObject mo in moc)
			{
				if (Convert.ToString(mo["Path"]) == path)
					return true;
			}
			
			return false;
		}
	}
}
