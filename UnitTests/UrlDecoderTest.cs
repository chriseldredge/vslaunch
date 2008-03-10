using System;
using NUnit.Framework;
using VSLaunch;

namespace UnitTests
{
	[TestFixture]
	public class UrlDecoderTest
	{
		private UrlDecoder dec;

		[Test]
		public void Parse()
		{
			dec = new UrlDecoder(new UriBuilder("randomproto://c:/projects/Fool/Fool.Apps.Content.Service/Metadata/DefaultMetadataService.cs?line=204").Uri);

			Assert.AreEqual("c:/projects/Fool/Fool.Apps.Content.Service/Metadata/DefaultMetadataService.cs", dec.FilePath);
			Assert.AreEqual(204, dec.LineNumber);
		}
	}
}
