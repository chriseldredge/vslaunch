using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using EnvDTE80;
using EnvDTE;

namespace VSLaunch
{
	public class Program
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
			catch (InvalidOperationException e)
			{
				MessageBox.Show(e.Message);
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
			catch (COMException e)
			{
				throw new InvalidOperationException("Please open Visual Studio with an appropriate Solution.", e);
			}

			RetryComOperation(delegate
			{
				if (string.IsNullOrEmpty(dte2.Solution.FileName))
				{
					throw new InvalidOperationException("Please open an appropriate Solution.");
				}

				path = RebasePath(path, dte2.Solution.FileName);

				ProjectItem item = dte2.Solution.FindProjectItem(path);

				if (item != null)
				{
					item.Open(Constants.vsViewKindCode);
					item.Document.Activate();
				}
				else
				{
					// The file is not in the solution, open it without associating the solution.
					dte2.ItemOperations.OpenFile(path, EnvDTE.Constants.vsViewKindCode);
				}
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

		private delegate void Operation();

		private const int RPC_E_SERVERCALL_RETRYLATER = -2147417846;

		private void RetryComOperation(Operation operation)
		{
			const int numTries = 10;
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

		public static string RebasePath(string absolutePath, string solutionFile)
		{
			if (absolutePath.StartsWith(@"c:\projects\", StringComparison.InvariantCultureIgnoreCase))
			{
				int of = solutionFile.IndexOf(@"\projects\", StringComparison.InvariantCultureIgnoreCase);
				if (!solutionFile.StartsWith(@"c:\projects\", StringComparison.InvariantCultureIgnoreCase) && of > 0)
				{
					var relPath = absolutePath.Substring(12);
					var solutionPath = solutionFile.Substring(0, of + 10);

					return Path.Combine(solutionPath, relPath);
				}
			}
			return absolutePath;
		}
	}
}
