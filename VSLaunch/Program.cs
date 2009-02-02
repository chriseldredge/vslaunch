using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE80;
using Microsoft.VisualBasic;
using EnvDTE;

namespace VSLaunch
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length < 1)
			{
				return;
			}

			try
			{
				UrlDecoder decoder = new UrlDecoder(new UriBuilder(args[0]).Uri);

				if (!File.Exists(decoder.FilePath))
				{
					MessageBox.Show(string.Format("The file '{0}' does not exist.", decoder.FilePath));
					return;
				}

				new Program().LaunchFile(Path.GetFullPath(decoder.FilePath), decoder.LineNumber);	
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n" + e.StackTrace);
			}
		}

		private void LaunchFile(string path, int lineNumber)
		{
			DTE2 dte2;
			string progId = "VisualStudio.DTE." + System.Configuration.ConfigurationManager.AppSettings["VisualStudioVersion"];

			try
			{
				// First, get a running Visual Studio instance
				
				dte2 = (DTE2)Marshal.GetActiveObject(progId);
			}
			catch (COMException)
			{
				// If that fails, start Visual Studio
				dte2 = (DTE2) Interaction.CreateObject(progId, "");
				dte2.UserControl = true;
			}

			RetryComOperation(delegate
			{
				// TODO: open a solution if one is not opened.
				foreach (Project project in dte2.Solution.Projects)
				{
					if (string.IsNullOrEmpty(project.FileName))
					{
						continue;
					}
					
					string projectDirectory = Path.GetDirectoryName(project.FileName);
					if (path.StartsWith(projectDirectory, StringComparison.InvariantCultureIgnoreCase))
					{
						if (OpenFileInProject(project, path, projectDirectory))
						{
							return;	
						}
					}
				}

				// The file is not in the solution, open it without associating the solution.
				dte2.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindCode);
			});

			TextSelection ts = null;

			RetryComOperation(delegate
			{
				ts = (TextSelection)dte2.ActiveDocument.Selection;
			});

			RetryComOperation(delegate
			{
				try
				{
					ts.GotoLine(lineNumber, true);
				}
				catch (ArgumentException)
				{
				}
			});

			RetryComOperation(delegate
			{
				dte2.MainWindow.Visible = true;
			});
		}

		private bool OpenFileInProject(Project project, string path, string projectDirectory)
		{
			string projectRelativeFilePath = Path.GetDirectoryName(path.Substring(projectDirectory.Length+1));
			string[] folders = projectRelativeFilePath.Split(Path.DirectorySeparatorChar);

			ProjectItems group = project.ProjectItems;

			foreach (string nested in folders)
			{
				try
				{
					group = group.Item(nested).ProjectItems;
				}
				catch (ArgumentException)
				{
					return false;
				}
			}

			string fileName = Path.GetFileName(path);

			int extension = fileName.IndexOf(".aspx.");
			if (extension > 0)
			{
				// Index.aspx.cs and Index.aspx.designer.cs will be children of Index.aspx
				group = group.Item(fileName.Substring(0, extension+5)).ProjectItems;
			}

			ProjectItem item;

			try
			{
				item = group.Item(fileName);
			}
			catch (ArgumentException)
			{
				return false;
			}

			item.Open(EnvDTE.Constants.vsViewKindCode);
			item.Document.Activate();
			return true;
		}

		private delegate void Operation();

		private static readonly int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

		private void RetryComOperation(Operation operation)
		{
			int numTries = 10;
			for (int i = 0; i < numTries; i++)
			{
				try
				{
					operation();
					return;
				}
				catch (COMException e)
				{
					if (i + 1 >= numTries || e.ErrorCode != RPC_E_SERVERCALL_RETRYLATER)
					{
						throw;
					}
				}

				System.Threading.Thread.Sleep(500);
			}
		}
	}
}
