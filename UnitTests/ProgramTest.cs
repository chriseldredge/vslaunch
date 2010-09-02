using NUnit.Framework; using NUnitIs = NUnit.Framework.Is;

using VSLaunch;

namespace UnitTests
{
	[TestFixture]
	public class ProgramTest
	{
		[Test]
		public void RebasePathNotNeeded()
		{
			var result = Program.RebasePath(@"c:\projects\fool\tools\vslaunch\vslaunch\program.cs",
			                                @"c:\projects\fool\tools\vslaunch\vslaunch.sln");
			Assert.That(result, NUnitIs.EqualTo(@"c:\projects\fool\tools\vslaunch\vslaunch\program.cs"));
		}

		[Test]
		public void RebasePath()
		{
			var result = Program.RebasePath(@"c:\projects\fool\tools\vslaunch\vslaunch\program.cs",
																			@"c:\working\projects\fool\tools\vslaunch\vslaunch.sln");
			Assert.That(result, NUnitIs.EqualTo(@"c:\working\projects\fool\tools\vslaunch\vslaunch\program.cs"));
		}
	}
}