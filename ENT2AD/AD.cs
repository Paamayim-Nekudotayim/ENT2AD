/*
 * Created by SharpDevelop.
 * User: administrateur
 * Date: 19/06/2013
 * Time: 07:59
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ENT2AD
{
	/// <summary>
	/// Description of ActiveDirectory.
	/// </summary>
	public static class AD
	{
		private const string ROOT_OU_NAME = "ENT_Root";
		
		public const string ELEVES_ROOT_OU_NAME = "Eleves";		
		public const string PROFS_ROOT_OU_NAME = "Profs";
		public const string GROUPS_ROOT_OU_NAME = "Groupes";
		
		public const string CLASSE_GROUPNAME_PREFIX = "ENT_Classe_";
		public const string CLASSE_GROUPDESC_PREFIX = "Classe de ";
		public const string PROFS_GROUPNAME = "ENT_Professeurs";
		
		public static string Domain { get; set; }
		
		static string logonServer;
		public static string LogonServer {
			get { return logonServer; }
			set 
			{
				if (!value.StartsWith(@"\\"))
					logonServer = @"\\" + value;
				else
					logonServer = value;
			}
		}
		
		public static string UsersProfile { get; set; }
		public static string ElevesScript { get; set; }
		public static string ProfsScript { get; set; }
		
		/// <summary>
		/// Obtient ou définit le contexte ActiveDirectory du domaine actuel sous la forme d'une requête LDAP de type DC=domain,DC=tld
		/// </summary>
		public static string NamingContext { get; set; }
		
		/// <summary>
		/// Obtient le chemin racine de la base LDAP
		/// </summary>
		private static string LdapRootPath
		{
			get
			{
				return String.Format("LDAP://{0}", NamingContext);
			}
		}
		
		private static string LdapENTRootPath
		{
			get
			{
				return String.Format("LDAP://OU={0},{1}", ROOT_OU_NAME, NamingContext);
			}			
		}
		
		public static void BuildENTOuStructure()
		{
			CreateENTOu(ELEVES_ROOT_OU_NAME, "Élèves présents dans l'ENT");
			CreateENTOu(PROFS_ROOT_OU_NAME, "Professeurs présents dans l'ENT");
			CreateENTOu(GROUPS_ROOT_OU_NAME, "Groupes de sécurité des utilisateurs ENT");
		}
		
		private static bool IsENTRootOUExisting()
		{
			try {
				var test = new DirectoryEntry(LdapENTRootPath).Guid;
				return true;
			} catch (Exception) {
				return false;
			}			
		}
		
		/// <summary>
		/// Obtient une valeur indiquant si l'annuaire contient l'unité d'organisation donnée à n'importe quel niveau de hiérarchie.
		/// </summary>
		/// <param name="name">Nom de l'unité d'organisation</param>
		/// <returns></returns>
		public static bool IsOUExisting(string name)
		{
			try {
				using (var deBase = new DirectoryEntry(LdapENTRootPath))
				{
					DirectorySearcher ouSrc = new DirectorySearcher(deBase);
					ouSrc.Filter = String.Format("(OU={0})", name);
					ouSrc.SearchScope = SearchScope.Subtree;
					SearchResult srOU = ouSrc.FindOne();
					return srOU != null;
				}     
			} catch (Exception) {
				return false;
			}
			   	
		}
		
		public static string GetOUProperty(string ldapPath, string name)
		{
			try {
				using (var ou = new DirectoryEntry(ldapPath))
				{
					return ou.Properties[name].Value.ToString();
				}
			} catch (Exception) {
				return String.Empty;
				throw;
			}
			
		}
		
		/// <summary>
		/// Crée une unité d'organisation à la racine de l'annuaire.
		/// </summary>
		/// <param name="name">Nom de l'unité d'organisation</param>
		/// <param name="description">Description de l'unité d'organisation</param>
		public static void CreateENTOu(string name, string description)
		{
			if (!IsENTRootOUExisting())
			{
				using (var DirectoryEntry = new DirectoryEntry(LdapRootPath))
					using (DirectoryEntry ou = DirectoryEntry.Children.Add("OU=" + ROOT_OU_NAME, "OrganizationalUnit"))
					{
						ou.Properties["description"].Value = "Racine ENT";
						ou.Properties["desktopProfile"].Value = UsersProfile;
						ou.CommitChanges();
					}
			}
			
			if (IsOUExisting(name))
				return;
						
			using (var DirectoryEntry = new DirectoryEntry(LdapENTRootPath))
				using (DirectoryEntry ou = DirectoryEntry.Children.Add("OU=" + name, "OrganizationalUnit"))
				{
					ou.Properties["description"].Value = description;
					
					if (name == ELEVES_ROOT_OU_NAME)
					{
						ou.Properties["wWWHomePage"].Value = ElevesScript;
					}
					
					if (name == PROFS_ROOT_OU_NAME)
					{
						ou.Properties["wWWHomePage"].Value = ProfsScript;
					}
					
					ou.CommitChanges();
				}
			
			
		}
		
		/// <summary>
		/// Validates the username and password of a given user
		/// </summary>
		/// <param name="sUserName">The username to validate</param>
		/// <param name="sPassword">The password of the username to validate</param>
		/// <returns>Returns True of user is valid</returns>
		public static bool ValidateCredentials(string sUserName, string sPassword)
		{
			PrincipalContext oPrincipalContext = GetPrincipalContext();
		    return oPrincipalContext.ValidateCredentials(sUserName, sPassword);
		
		}
		
		/// <summary>
		/// Checks if the User Account is Expired
		/// </summary>
		/// <param name="sUserName">The username to check</param>
		/// <returns>Returns true if Expired</returns>
		public static bool IsUserExpired(string sUserName)
		{
		    UserPrincipal oUserPrincipal = GetUser(sUserName);
		    if (oUserPrincipal.AccountExpirationDate != null)
		    {
		        return false;
		    }
		    else
		    {
		        return true;
		    }
		}
		
		/// <summary>
		/// Checks if user exsists on AD
		/// </summary>
		/// <param name="sUserName">The username to check</param>
		/// <returns>Returns true if username Exists</returns>
		public static bool IsUserExisting(string upn, out UserPrincipal user)
		{
			UserPrincipal foundUser = GetUser(upn);
			
		    if (foundUser == null)
		    {
		    	user = null;
		        return false;
		    }
		    else
		    {
		    	user = foundUser;
		        return true;
		    }
		}
		
		/// <summary>
		/// Checks if user accoung is locked
		/// </summary>
		/// <param name="sUserName">The username to check</param>
		/// <returns>Retruns true of Account is locked</returns>
		public static bool IsAccountLocked(string upn)
		{
		    UserPrincipal oUserPrincipal = GetUser(upn);
		    return oUserPrincipal.IsAccountLockedOut();
		}


		#region Search Methods
		
		/// <summary>
		/// Gets a certain user on Active Directory
		/// </summary>
		/// <param name="sUserName">The username to get</param>
		/// <returns>Returns the UserPrincipal Object</returns>
		public static UserPrincipal GetUser(string upn)
		{
			UserPrincipal oUserPrincipal = UserPrincipal.FindByIdentity(GetPrincipalContext(), IdentityType.UserPrincipalName, upn);
		    return oUserPrincipal;
		}
		
		/// <summary>
		/// Gets a certain group on Active Directory
		/// </summary>
		/// <param name="sGroupName">The group to get</param>
		/// <returns>Returns the GroupPrincipal Object</returns>
		public static GroupPrincipal GetGroup(string name)
		{
			PrincipalContext oPrincipalContext = GetPrincipalContext();
		
		    GroupPrincipal oGroupPrincipal = GroupPrincipal.FindByIdentity(oPrincipalContext, name);
		    return oGroupPrincipal;
		}
		
		#endregion
		
		#region User Account Methods
		
		/// <summary>
		/// Sets the user password
		/// </summary>
		/// <param name="sUserName">The username to set</param>
		/// <param name="sNewPassword">The new password to use</param>
		public static bool SetUserPassword(string upn, string password)
		{
		    try
		    {
		    	using (UserPrincipal user = GetUser(upn))
		    	{
		    		user.SetPassword(password);
		    		user.SetProperty("info", password);
		    	}
		        
		        return true;
		    }
		    catch (Exception)
		    {
		        return false;
		    }
		
		}
		
		public static bool SetUserPassword(UserPrincipal user, string password)
		{
		    try
		    {
		    	using (user)
		    	{
		    		user.SetPassword(password);
			    	user.SetProperty("info", password);
		    	}
		        
		        return true;
		    }
		    catch (Exception)
		    {
		        return false;
		    }
		
		}
		
		public static void UpdateUserDivision(UserPrincipal user, string division)
		{
		    using (user)
	    	{
		    	user.Description = division;
		    	user.Save();
		    	user.SetProperty("division", division);
	    	}  
		}
		
		/// <summary>
		/// Enables a disabled user account
		/// </summary>
		/// <param name="sUserName">The username to enable</param>
		public static void EnableUserAccount(string upn)
		{
			using (UserPrincipal user = GetUser(upn))
			{
				user.Enabled = true;
		    	user.Save();
			}		    
		}
		
		/// <summary>
		/// Force disbaling of a user account
		/// </summary>
		/// <param name="sUserName">The username to disable</param>
		public static void DisableUserAccount(string upn)
		{
			using (UserPrincipal user = GetUser(upn))
			{
				user.Enabled = false;
		   		user.Save();
			}		    
		}
		
//		/// <summary>
//		/// Force expire password of a user
//		/// </summary>
//		/// <param name="sUserName">The username to expire the password</param>
//		public static void ExpireUserPassword(string upn)
//		{
//		    UserPrincipal oUserPrincipal = GetUser(upn);
//		    oUserPrincipal.ExpirePasswordNow();
//		    oUserPrincipal.Save();
//		
//		}
		
//		/// <summary>
//		/// Unlocks a locked user account
//		/// </summary>
//		/// <param name="sUserName">The username to unlock</param>
//		public static void UnlockUserAccount(string upn)
//		{
//		    UserPrincipal oUserPrincipal = GetUser(upn);
//		    oUserPrincipal.UnlockAccount();
//		    oUserPrincipal.Save();
//		}
		
		public static UserPrincipal CreateNewUser(ENTUser ENTUser, out bool isExisting)
		{
			UserPrincipal foundUser;
			isExisting = false;
			
			if (IsUserExisting(ENTUser.UserPrincipalName, out foundUser))
			{
				isExisting = true;
				return UpdateExistingUser(foundUser, ENTUser); // mise à jour du mot de passe de l'utilisateur
			}			

	    	var ctx = GetPrincipalContext(ENTUser.IsPupil ? "Eleves" : "Profs");
			var user = new UserPrincipal(ctx, ENTUser.SAMAccountName, ENTUser.Password, true);
			
			user.UserPrincipalName = ENTUser.UserPrincipalName;
			user.GivenName = ENTUser.GivenName;
			user.Surname = ENTUser.SurName;
			user.Name = String.Format("{0} {1} {2}", ENTUser.SurName, ENTUser.GivenName, ENTUser.UID);
			user.Description = ENTUser.IsPupil ? ENTUser.Division : "Professeur";
			user.DisplayName = String.Format("{0} {1}", ENTUser.GivenName, ENTUser.SurName);
			user.EmployeeId = ENTUser.UID;
			user.PasswordNotRequired = false;
			user.UserCannotChangePassword = true;
			user.PasswordNeverExpires = true;
			user.Save();
			
			user.SetProperty("profilePath", Path.Combine(LogonServer, UsersProfile));
			user.SetProperty("scriptPath", ENTUser.IsPupil ? ElevesScript : ProfsScript);
			user.SetProperty("employeeType", ENTUser.IsPupil ? "Élève" : "Professeur");
			user.SetProperty("info", ENTUser.Password);
			user.SetProperty("personalTitle", ENTUser.Title);
			user.SetProperty("businessCategory", "ENT");
			user.SetProperty("division", ENTUser.IsPupil ? ENTUser.Division : null);
			
			return user;
		}
		
		public static UserPrincipal UpdateExistingUser(UserPrincipal user, ENTUser ENTUser)
		{
			if (user.GetProperty("info") != ENTUser.Password)
			{
				SetUserPassword(user, ENTUser.Password);
			}
			
//			if (user.GetProperty("division") != ENTUser.Division)
//			{
//				SetUserDivision(user, ENTUser.Division);
//			}
			
			return user;
		}
		
		/// <summary>
		/// Deletes a user in Active Directory
		/// </summary>
		/// <param name="sUserName">The username you want to delete</param>
		/// <returns>Returns true if successfully deleted</returns>
		public static bool DeleteUser(string sUserName)
		{
		    try
		    {
		        UserPrincipal oUserPrincipal = GetUser(sUserName);
		
		        oUserPrincipal.Delete();
		        return true;
		    }
		    catch
		    {
		        return false;
		    }
		}
		
		#endregion
		
		#region Group Methods
		
		/// <summary>
		/// Creates a new group in Active Directory
		/// </summary>
		/// <param name="sOU">The OU location you want to save your new Group</param>
		/// <param name="sGroupName">The name of the new group</param>
		/// <param name="sDescription">The description of the new group</param>
		/// <param name="oGroupScope">The scope of the new group</param>
		/// <param name="bSecurityGroup">True is you want this group to be a security group, false if you want this as a distribution group</param>
		/// <returns>Retruns the GroupPrincipal object</returns>
		public static GroupPrincipal CreateNewGroup(string sOU, string sGroupName, string sDescription, GroupScope oGroupScope, bool bSecurityGroup)
		{
			PrincipalContext oPrincipalContext = GetPrincipalContext(sOU);
		
		    GroupPrincipal oGroupPrincipal = new GroupPrincipal(oPrincipalContext, sGroupName);
		    oGroupPrincipal.Description = sDescription;
		    oGroupPrincipal.GroupScope = oGroupScope;
		    oGroupPrincipal.IsSecurityGroup = bSecurityGroup;
		    oGroupPrincipal.Save();
		
		    return oGroupPrincipal;
		}
		
		/// <summary>
		/// Adds the user for a given group
		/// </summary>
		/// <param name="sUserName">The user you want to add to a group</param>
		/// <param name="sGroupName">The group you want the user to be added in</param>
		/// <returns>Returns true if successful</returns>
		public static bool AddUserToGroup(UserPrincipal user, string name, string description)
		{						
			bool newGroup = false;
		    GroupPrincipal group = GetGroup(name);
		    
		    if (group == null)
		    {
		    	newGroup = true;
		    	group = CreateNewGroup("Groupes", name, description, GroupScope.Global, true);
		    }
		    
		    if (newGroup || !IsUserGroupMember(user, group))
	        {
	            group.Members.Add(user);
	            group.Save();
	        }
	        return true;
		}
		
		/// <summary>
		/// Change un élève de groupe de classe.
		/// </summary>
		/// <param name="user">Utilisateur présent dans l'annuaire</param>
		/// <param name="name">Nom du groupe dans lequel l'utilisateur sera placé</param>
		/// <param name="description">Description du groupe dans lequel l'utilisateur sera placé</param>
		public static void UpdateUserGroup(UserPrincipal user, string name, string description)
		{
			RemoveUserFromGroup(user, GetGroup(CLASSE_GROUPNAME_PREFIX + user.GetProperty("division")));
			AddUserToGroup(user, name, description);
		}
		
		/// <summary>
		/// Removes user from a given group
		/// </summary>
		/// <param name="sUserName">The user you want to remove from a group</param>
		/// <param name="sGroupName">The group you want the user to be removed from</param>
		/// <returns>Returns true if successful</returns>
		public static bool RemoveUserFromGroup(UserPrincipal user, GroupPrincipal group)
		{
		    try
		    {
		        if (group != null)
		        {
		            if (IsUserGroupMember(user, group))
		            {
		                group.Members.Remove(user);
		                group.Save();
		            }
		        }
		        return true;
		    }
		    catch
		    {
		        return false;
		    }
		}
		
		/// <summary>
		/// Checks if user is a member of a given group
		/// </summary>
		/// <param name="sUserName">The user you want to validate</param>
		/// <param name="sGroupName">The group you want to check the membership of the user</param>
		/// <returns>Returns true if user is a group member</returns>
		public static bool IsUserGroupMember(UserPrincipal user, GroupPrincipal group)
		{
	
		    if (group != null)
		    {
		        return group.Members.Contains(user);
		    }
		    else
		    {
		        return false;
		    }
		}
		
		///// <summary>
		///// Gets a list of the users group memberships
		///// </summary>
		///// <param name="sUserName">The user you want to get the group memberships</param>
		///// <returns>Returns an arraylist of group memberships</returns>
		//public static ArrayList GetUserGroups(string sUserName)
		//{
		//    ArrayList myItems = new ArrayList();
		//    UserPrincipal oUserPrincipal = GetUser(sUserName);
		//
		//    PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetGroups();
		//
		//    foreach (Principal oResult in oPrincipalSearchResult)
		//    {
		//        myItems.Add(oResult.Name);
		//    }
		//    return myItems;
		//}
		
		///// <summary>
		///// Gets a list of the users authorization groups
		///// </summary>
		///// <param name="sUserName">The user you want to get authorization groups</param>
		///// <returns>Returns an arraylist of group authorization memberships</returns>
		//public static ArrayList GetUserAuthorizationGroups(string sUserName)
		//{
		//    ArrayList myItems = new ArrayList();
		//    UserPrincipal oUserPrincipal = GetUser(sUserName);
		//
		//    PrincipalSearchResult<Principal> oPrincipalSearchResult = oUserPrincipal.GetAuthorizationGroups();
		//
		//    foreach (Principal oResult in oPrincipalSearchResult)
		//    {
		//        myItems.Add(oResult.Name);
		//    }
		//    return myItems;
		//}
		
		#endregion
		
		#region Helper Methods
		
		/// <summary>
		/// Gets the base principal context
		/// </summary>
		/// <returns>Retruns the PrincipalContext object</returns>
		public static PrincipalContext GetPrincipalContext()
		{
			PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain, Domain);
		    return oPrincipalContext;
		}
		
		
		/// <summary>
		/// Gets the principal context on specified OU
		/// </summary>
		/// <param name="sOU">The OU you want your Principal Context to run on</param>
		/// <returns>Retruns the PrincipalContext object</returns>
		public static PrincipalContext GetPrincipalContext(string ou)
		{
			PrincipalContext oPrincipalContext = new PrincipalContext(ContextType.Domain, Domain, GetLdapPathFromOUs(ou));
		    return oPrincipalContext;
		}
		
		#endregion
		
		public static string GetLdapPathFromOUs()
		{
			return String.Format("OU={0},{1}", ROOT_OU_NAME, NamingContext);
		}
		
		public static string GetLdapPathFromOUs(string ou)
		{
			return String.Format("OU={0},OU={1},{2}", ou, ROOT_OU_NAME, NamingContext);
		}
		
	}
	
	public static class AccountManagementExtensions
    {

        public static string GetProperty(this Principal principal, string property)
        {
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            if (directoryEntry.Properties.Contains(property))
                return directoryEntry.Properties[property].Value.ToString();
            else
                return string.Empty;
        }

        public static void SetProperty(this Principal principal, string property, string value)
        {
            var directoryEntry = principal.GetUnderlyingObject() as DirectoryEntry;
            directoryEntry.Properties[property].Value = value;
            directoryEntry.CommitChanges();
        }


    }

}
