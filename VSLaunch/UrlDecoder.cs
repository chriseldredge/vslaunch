using System;
using System.Collections.Specialized;
using System.Web;

namespace VSLaunch
{
	public class UrlDecoder
	{
		private readonly int lineNumber;
		private readonly string filePath;

		public UrlDecoder(Uri url)
		{
			NameValueCollection queryParams = HttpUtility.ParseQueryString(url.Query);

			string lineNumberString = queryParams["line"];

			if (string.IsNullOrEmpty(lineNumberString) || !Int32.TryParse(lineNumberString, out this.lineNumber))
			{
				this.lineNumber = 1;
			}

			filePath = url.AbsolutePath;
		}

		public int LineNumber
		{
			get { return lineNumber; }
		}

		public string FilePath
		{
			get { return filePath; }
		}
	}
}