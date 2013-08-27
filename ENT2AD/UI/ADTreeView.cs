/*
 * Created by SharpDevelop.
 * User: administrateur
 * Date: 19/06/2013
 * Time: 07:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ENT2AD
{
	/// <summary>
	/// Description of ActiveDirectoryTreeView.
	/// </summary>
	public class ADTreeView: TreeView
	{
		public ADTreeView()
		{
		}
		
		public void UpdateList()
		{
			Nodes.Clear();
			
			foreach (var ENTUser in ENTUsers.Users.Values)
			{
				var userNode = new TreeNode(string.Format("{0} {1} ({2})", ENTUser.SurName, ENTUser.GivenName, ENTUser.UID));
				userNode.Tag = ENTUser.UID;	

				if (ENTUser.IsPupil)
				{
					TreeNode treenode;
					if (!Nodes.ContainsKey(ENTUser.Division))
					    treenode = Nodes.Add(ENTUser.Division, ENTUser.Division);
					else
						treenode = Nodes.Find(ENTUser.Division, false)[0];
								
					treenode.Nodes.Add(userNode);
				}
				else
				{
					Nodes.Add(userNode);
				}
				
			}
			
			Sort();
		}
	}
}
