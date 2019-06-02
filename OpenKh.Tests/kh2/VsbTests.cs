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

        [Theory]
        [InlineData("test.vsb")]
        public void ExportAllEntries(string fileName)
        {
            using (var stream = File.OpenRead($"kh2/res/{fileName}"))
            {
                var dirPath = $"kh2/res/_ex";
                if (Directory.Exists(dirPath))
                    Directory.Delete(dirPath);

                Directory.CreateDirectory(dirPath);
                foreach (var file in Vsb.Open(stream))
                {
                    file.ToWav(Path.Join(dirPath, $"{file.StreamName}.wav"));
                }
            }

            //get file count from folder, check with count from file
        }
    }
}
