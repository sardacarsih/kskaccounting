namespace Accounting.Updater
{
    /// <summary>
    /// Command line contract between Accounting.exe and Accounting.Updater.exe:
    /// <c>--pid &lt;id&gt; --zip &lt;path&gt; --target &lt;dir&gt; --exe Accounting.exe --version &lt;v&gt; [--elevated]</c>.
    /// </summary>
    internal sealed record UpdaterArguments(
        int? ProcessId,
        string ZipPath,
        string TargetDirectory,
        string ExeName,
        string Version,
        bool Elevated)
    {
        public static UpdaterArguments Parse(IReadOnlyList<string> args)
        {
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            bool elevated = false;

            for (int i = 0; i < args.Count; i++)
            {
                string key = args[i];
                if (string.Equals(key, "--elevated", StringComparison.OrdinalIgnoreCase))
                {
                    elevated = true;
                    continue;
                }

                if (!key.StartsWith("--", StringComparison.Ordinal) || i + 1 >= args.Count)
                {
                    throw new ArgumentException($"Argumen tidak dikenal: {key}");
                }

                values[key[2..]] = args[++i];
            }

            int? pid = null;
            if (values.TryGetValue("pid", out string? pidText))
            {
                if (!int.TryParse(pidText, out int parsedPid) || parsedPid <= 0)
                {
                    throw new ArgumentException($"PID tidak valid: {pidText}");
                }

                pid = parsedPid;
            }

            string zip = Require(values, "zip");
            string target = Require(values, "target");
            string exe = values.TryGetValue("exe", out string? exeName) && !string.IsNullOrWhiteSpace(exeName)
                ? exeName
                : "Accounting.exe";

            if (exe.IndexOfAny(new[] { '\\', '/' }) >= 0)
            {
                throw new ArgumentException("--exe harus berupa nama file, bukan path.");
            }

            return new UpdaterArguments(
                pid,
                Path.GetFullPath(zip),
                Path.GetFullPath(target),
                exe,
                values.TryGetValue("version", out string? version) ? version : string.Empty,
                elevated);
        }

        public IEnumerable<string> ToArgumentList(bool elevated)
        {
            if (ProcessId.HasValue)
            {
                yield return "--pid";
                yield return ProcessId.Value.ToString();
            }

            yield return "--zip";
            yield return ZipPath;
            yield return "--target";
            yield return TargetDirectory;
            yield return "--exe";
            yield return ExeName;
            yield return "--version";
            yield return Version;

            if (elevated)
            {
                yield return "--elevated";
            }
        }

        private static string Require(Dictionary<string, string> values, string key)
        {
            if (!values.TryGetValue(key, out string? value) || string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Argumen --{key} wajib diisi.");
            }

            return value;
        }
    }
}
