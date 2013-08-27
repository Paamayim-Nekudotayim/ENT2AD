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
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices.ActiveDirectory;
using System.Globalization;
using System.Linq;
using System.Text;
using CsvHelper.Configuration;

namespace ENT2AD
{
	public class ENTUser
	{
		private const int SAM_LENGTH = 20;
		private const int UID_LENGTH = 8;
		
		public string School { get; set; }
		public string UID { get; set; }
		public string Title { get; set; }
		
		string surName;
		public string SurName {
			get { return surName; }
			set { surName = (new CultureInfo("fr-FR")).TextInfo.ToTitleCase(value.ToLower()); }
		}
		
		string givenName;		
		public string GivenName {
			get { return givenName.Split(',')[0]; }
			set { givenName = (new CultureInfo("fr-FR")).TextInfo.ToTitleCase(value.ToLower()); }
		}
		
		public string Category { get; set; }
		
		string login;
		public string Login {
			get { return login.ToLower(); }
			set
			{
				login = value;
				UserPrincipalName = String.Format("{0}@{1}", Login, Domain.GetCurrentDomain());
			}
		}
		
		string password;
		public string Password {
			get { return String.IsNullOrEmpty(password) ? "0000" : password; }
			set { password = value; }
		}
		public string Division { get; set; }
				
		public bool IsPupil {
			get
			{
				return !String.IsNullOrEmpty(Division);
			}
		}
		
		public string UserPrincipalName { get; set; }
		
		public string SAMAccountName
		{
			get
			{
				string name = Login.Split('.').Last();
				
				if (name.Length > SAM_LENGTH - UID_LENGTH - 1)
					name = name.Substring(0, SAM_LENGTH - UID_LENGTH - 1);
				
				return String.Format("{0}.{1}", name, UID);
			}			
		}
		
		public bool CreateAccount()
		{
			bool isExisting;
			
			using (var user = AD.CreateNewUser(this, out isExisting))
			{
				if (!isExisting)
				{
					if (IsPupil)
					{
						AD.AddUserToGroup(user, "ENT_Eleves", "Élèves présents sur l'ENT");
						AD.AddUserToGroup(user, AD.CLASSE_GROUPNAME_PREFIX + Division, "Classe de " + Division);
					}
					else
					{
						AD.AddUserToGroup(user, "ENT_Professeurs", "Professeurs présents sur l'ENT");
					}
				}
				else
				{
					if (IsPupil)
					{
						string oldDivision = user.GetProperty("division");
						
						// si la classe a changé, on migre l'utilisateur
						if (oldDivision != Division)
						{
							AD.UpdateUserGroup(user, AD.CLASSE_GROUPNAME_PREFIX + Division, AD.CLASSE_GROUPDESC_PREFIX + Division);
							AD.UpdateUserDivision(user, Division);
							Storage.MoveUserDirectory(SAMAccountName, oldDivision, Division);
						}	
					}
				}
			}
			
			return isExisting;
		}
		
		public void CreateUserDirectory()
		{
			Storage.CreateUserDirectory(this);
		}
		
		public void CreateAccountAndDirectory()
		{
			bool isExisting = CreateAccount();
			
			if (!isExisting)
				CreateUserDirectory();
		}
	}
	
	public sealed class ENTUserMap : CsvClassMap<ENTUser>
	{
		public override void CreateMap()
		{
			Map( m => m.School		).Name( "Etablissement" );
			Map( m => m.UID			).Name( "UID" );
			Map( m => m.Title		).Name( "Civilité" );
			Map( m => m.SurName		).Name( "Nom" );
			Map( m => m.GivenName	).Name( "Prénom" );
			Map( m => m.Category	).Name( "Catégorie" );
			Map( m => m.Login		).Name( "Identifiant" );
			Map( m => m.Password	).Name( "Mot de passe" );
			Map( m => m.Division	).Name( "Classe" );
		}
	}
	
	public static class ENTUsers
	{
		public static Dictionary<string, ENTUser> Users { get; set; }		
		
		static List<ENTUser> m_ADCreationFailed = new List<ENTUser>();
		public static List<ENTUser> ADCreationFailed {
			get { return m_ADCreationFailed; }
			set { m_ADCreationFailed = value; }
		}
	}
}
