using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class VsbTests
    {
        [Theory]
        [InlineData("test.vsb")]
        public void HasRightAmountOfEntries(string fileName)
        {
            using (var stream = File.OpenRead($"kh2/res/{fileName}"))
            {
                Assert.Equal(5, Vsb.Count(stream));
            }
        }
    }
}
