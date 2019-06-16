using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class VagTests
    {
        [Theory]
        [InlineData("v4.vag")]
        public void IsVersion4VagFile(string fileName)
        {
            Vag vag = new Vag(File.OpenRead($"kh2/res/{fileName}"));
            Assert.Equal(4, vag.Version);
        }
    }
}
