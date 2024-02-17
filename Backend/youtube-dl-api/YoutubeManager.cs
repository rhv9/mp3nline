namespace youtube_dl_api.youtubemanager
{
    public class YoutubeManager
    {

        public static void GetSong(string url)
        {
            Console.WriteLine(url);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(); 
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;   
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C yt-dlp.exe -x --audio-format mp3 " + url;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Failed to get song do something

            }
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("yt-dlp process exit code: " + process.ExitCode);

            int i = 0;
            foreach (var item in output.Split())
            {
                Console.WriteLine("Output message " + ++i + ": " + item);
            }
        }
    }
}
