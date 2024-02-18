namespace youtube_dl_api.youtubemanager
{

    //./yt-dlp.exe --skip-download --print "%(duration>%H:%M:%S.%s)s %(creator)s %(uploader)s - %(title)s" https://www.youtube.com/watch?v=v2H4l9RpkwM

    // ./yt-dlp.exe -x --audio-format mp3 -o "%(title)s.%(ext)s" [URL]
    public class YoutubeManager
    {

        public static IResult GetSong(string url)
        {
            Console.WriteLine(url);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(); 
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;   
            startInfo.FileName = "cmd.exe";
            // todo extracting audio when file already exists causes errors
            startInfo.Arguments = "/C yt-dlp.exe -x --audio-format mp3 -o \"%(title)s.%(ext)s\" " + url;
            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Failed to get song do something
                return Results.NotFound();
            }
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("yt-dlp process exit code: " + process.ExitCode);
            
            string filename = "";
            foreach (string line in output.Split('\n'))
            {
                Console.WriteLine("HEHE:D: " + line);
                if (line.Contains("[ExtractAudio] Destination: "))
                {
                    int textLen = "[ExtractAudio] Destination: ".Length;
                    filename = line.Substring(textLen, line.Length - textLen);
                }
            }

            if (filename == "")
            {
                // For some reason file name was not found
                return Results.BadRequest();
            }


            FileInfo fileInfo = new FileInfo(filename);
            FileStream filestream = System.IO.File.OpenRead(fileInfo.FullName);
            return Results.File(filestream, contentType: "video/mp4", fileDownloadName: filename, enableRangeProcessing: true);
        }
    }
}
