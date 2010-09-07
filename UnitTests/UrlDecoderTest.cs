using System;
using NUnit.Framework; using Is = NUnit.Framework.Is;
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

		[Test]
		public void ParseDecodesEscapedCharacters()
		{
			dec = new UrlDecoder(new UriBuilder("vslaunch://c:/projects/Fool/Folder%20With%20Spaces/Foo.txt?line=21").Uri);

			Assert.AreEqual("c:/projects/Fool/Folder With Spaces/Foo.txt", dec.FilePath);
			Assert.AreEqual(21, dec.LineNumber);
		}
	}
}
