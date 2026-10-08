using System.IO.Compression;
using Accounting.Updater;

namespace Accounting.Tests;

public sealed class UpdateInstallerTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "AccountingInstallerTests", Guid.NewGuid().ToString("N"));
    private readonly string target;
    private readonly string zipPath;
    private readonly List<string> log = new();

    public UpdateInstallerTests()
    {
        target = Path.Combine(root, "app");
        zipPath = Path.Combine(root, "package.zip");
        Directory.CreateDirectory(target);
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public void Install_ReplacesFiles_AddsNewFiles_AndPreservesSiteConfig()
    {
        WriteTarget("Accounting.dll", "lama");
        WriteTarget("Utilities/config.json", "{\"site\":\"KSK\"}");
        WriteTarget("logs/financeApp.log", "log lama");
        CreateZip(
            ("Accounting.dll", "baru"),
            ("Laporan/baru.repx", "repx"),
            ("Utilities/config.json", "{\"site\":\"DEFAULT\"}"),
            ("logs/financeApp.log", "ditimpa"));

        InstallResult result = new UpdateInstaller(log.Add).Install(zipPath, target, "2.0.0.0");

        Assert.Equal(2, result.FilesWritten);
        Assert.Equal("baru", ReadTarget("Accounting.dll"));
        Assert.Equal("repx", ReadTarget("Laporan/baru.repx"));
        Assert.Equal("{\"site\":\"KSK\"}", ReadTarget("Utilities/config.json"));
        Assert.Equal("log lama", ReadTarget("logs/financeApp.log"));
        Assert.False(Directory.Exists(Path.Combine(target, UpdateInstaller.StagingFolderName)));

        string backup = Directory.GetDirectories(Path.Combine(target, UpdateInstaller.BackupFolderName)).Single();
        Assert.Equal("lama", File.ReadAllText(Path.Combine(backup, "Accounting.dll")));
    }

    [Fact]
    public void Install_FailureMidway_RestoresOriginalFiles()
    {
        WriteTarget("a.dll", "lama-a");
        WriteTarget("b.dll", "lama-b");
        CreateZip(("a.dll", "baru-a"), ("b.dll", "baru-b"), ("c.dll", "baru-c"));

        // Lock one destination so the copy fails after other files were already replaced.
        using (var locked = new FileStream(Path.Combine(target, "b.dll"), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            Assert.ThrowsAny<IOException>(() => new UpdateInstaller(log.Add).Install(zipPath, target, "2.0.0.0"));
        }

        Assert.Equal("lama-a", ReadTarget("a.dll"));
        Assert.Equal("lama-b", ReadTarget("b.dll"));
        Assert.False(File.Exists(Path.Combine(target, "c.dll")));
    }

    [Fact]
    public void Rollback_AfterSuccessfulInstall_RestoresPreviousVersion()
    {
        WriteTarget("Accounting.dll", "lama");
        CreateZip(("Accounting.dll", "baru"), ("GLMigrator.exe", "migrator"));
        var installer = new UpdateInstaller(log.Add);

        InstallResult result = installer.Install(zipPath, target, "2.0.0.0");
        installer.Rollback(result);

        Assert.Equal("lama", ReadTarget("Accounting.dll"));
        Assert.False(File.Exists(Path.Combine(target, "GLMigrator.exe")));
    }

    [Fact]
    public void MigrationRunner_WithoutMigratorInPackage_IsSkipped()
    {
        MigrationOutcome outcome = DatabaseMigrationRunner.Run(target, log.Add, TimeSpan.FromSeconds(5));

        Assert.Equal(MigrationStatus.Skipped, outcome.Status);
    }

    [Fact]
    public void MigrationRunner_WithoutConnectionConfig_Fails()
    {
        WriteTarget(DatabaseMigrationRunner.MigratorExeName, "bukan exe");

        MigrationOutcome outcome = DatabaseMigrationRunner.Run(target, log.Add, TimeSpan.FromSeconds(5));

        Assert.Equal(MigrationStatus.Failed, outcome.Status);
        Assert.Contains("config.json", outcome.Error);
    }

    [Fact]
    public void ExtractSafely_RejectsPathTraversal()
    {
        using (ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
        {
            ZipArchiveEntry entry = archive.CreateEntry("../evil.dll");
            using var writer = new StreamWriter(entry.Open());
            writer.Write("x");
        }

        Assert.Throws<InvalidDataException>(() => UpdateInstaller.ExtractSafely(zipPath, Path.Combine(root, "staging")));
        Assert.False(File.Exists(Path.Combine(root, "evil.dll")));
    }

    [Theory]
    [InlineData("Utilities/config.json", true)]
    [InlineData("utilities\\CONFIG.json", true)]
    [InlineData("logs/x.log", true)]
    [InlineData("_update_backup/x/Accounting.dll", true)]
    [InlineData("Accounting.dll", false)]
    [InlineData("Accounting.Updater.exe", false)]
    [InlineData("Utilities/financeicon.ico", false)]
    public void IsPreserved_MatchesSiteSpecificPaths(string path, bool expected)
    {
        Assert.Equal(expected, UpdateInstaller.IsPreserved(path));
    }

    [Fact]
    public void Parse_ReadsArguments()
    {
        UpdaterArguments args = UpdaterArguments.Parse(new[]
        {
            "--pid", "123", "--zip", zipPath, "--target", target, "--exe", "Accounting.exe", "--version", "2.1.0.0", "--elevated"
        });

        Assert.Equal(123, args.ProcessId);
        Assert.Equal(Path.GetFullPath(target), args.TargetDirectory);
        Assert.Equal("2.1.0.0", args.Version);
        Assert.True(args.Elevated);
        Assert.Equal(args with { Elevated = false }, UpdaterArguments.Parse(args.ToArgumentList(elevated: false).ToArray()));
    }

    [Theory]
    [InlineData("--zip", "a.zip")]
    [InlineData("--zip", "a.zip", "--target", "x", "--exe", "..\\evil.exe")]
    [InlineData("--pid", "abc", "--zip", "a.zip", "--target", "x")]
    public void Parse_InvalidArguments_Throws(params string[] args)
    {
        Assert.Throws<ArgumentException>(() => UpdaterArguments.Parse(args));
    }

    private void WriteTarget(string relative, string content)
    {
        string path = Path.Combine(target, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
    }

    private string ReadTarget(string relative) => File.ReadAllText(Path.Combine(target, relative));

    private void CreateZip(params (string Path, string Content)[] entries)
    {
        using ZipArchive archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
        foreach ((string path, string content) in entries)
        {
            using var writer = new StreamWriter(archive.CreateEntry(path).Open());
            writer.Write(content);
        }
    }
}
