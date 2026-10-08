using System.Media;
using Accounting.Utilities;
using Xunit;

namespace Accounting.Tests;

public sealed class AppSoundTests
{
    [Fact]
    public void ResolvePath_UsesApplicationBaseDirectory_NotCurrentDirectory()
    {
        string path = AppSound.ResolvePath("jurnal_selesai.wav");

        Assert.Equal(Path.Combine(AppContext.BaseDirectory, "wav", "jurnal_selesai.wav"), path);
    }

    [Fact]
    public void Play_MissingFile_DoesNotThrow()
    {
        using var player = new SoundPlayer();

        Exception? error = Record.Exception(() => AppSound.Play(player, "tidak_ada_file.wav"));

        Assert.Null(error);
    }
}
