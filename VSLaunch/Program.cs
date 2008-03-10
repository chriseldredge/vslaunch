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

				new Program().LaunchFile(decoder.FilePath, decoder.LineNumber);	
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message + "\n" + e.StackTrace);
			}
		}

		private void LaunchFile(string path, int lineNumber)
		{
			DTE2 dte2;

			try
			{
				dte2 = (DTE2) Marshal.GetActiveObject("VisualStudio.DTE.8.0");
			}
			catch (COMException)
			{
				dte2 = (DTE2) Interaction.CreateObject("VisualStudio.DTE.8.0", "");
				dte2.UserControl = true;
			}

			RetryComOperation(delegate
			{
				dte2.ItemOperations.OpenFile(path, null);
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
