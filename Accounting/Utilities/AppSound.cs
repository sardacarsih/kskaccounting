using System;
using System.IO;
using System.Media;

namespace Accounting.Utilities
{
    /// <summary>
    /// Memutar file suara dari folder wav di samping Accounting.exe. Tidak pernah melempar exception:
    /// file hilang atau perangkat audio tidak tersedia cukup dilewati.
    /// </summary>
    internal static class AppSound
    {
        public static string ResolvePath(string fileName) =>
            Path.Combine(AppContext.BaseDirectory, "wav", fileName);

        public static void Play(SoundPlayer player, string fileName)
        {
            try
            {
                string path = ResolvePath(fileName);
                if (!File.Exists(path))
                {
                    return;
                }

                player.SoundLocation = path;
                player.Play();
            }
            catch (Exception)
            {
                // Suara hanya pelengkap; jangan ganggu proses utama.
            }
        }
    }
}
