using System.IO;

namespace youtube_dl_api.youtubemanager
{

    //./yt-dlp.exe --skip-download --print "%(duration>%H:%M:%S.%s)s %(creator)s %(uploader)s - %(title)s" https://www.youtube.com/watch?v=v2H4l9RpkwM

    // ./yt-dlp.exe -x --audio-format mp3 -o "%(title)s.%(ext)s" [URL]
    // ./yt-dlp.exe -x --audio-format mp3 --no-playlist -o "%(title)s.%(ext)s" "
    public class YoutubeManager
    {

        public static IResult GetSong(string url)
        {
            Console.WriteLine("\n\nSTARTING YT\n\n");

            Console.OutputEncoding = System.Text.Encoding.UTF8;
            Console.WriteLine(url);

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo(); 
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;   
            startInfo.FileName = "cmd.exe";
            // todo extracting audio when file already exists causes errors
            startInfo.Arguments = "/C cd yt && yt-dlp.exe -x --audio-format mp3 --no-playlist -o \"%(title)s.%(ext)s\" \"" + url + "\"";
            startInfo.RedirectStandardOutput = true;
            startInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            startInfo.RedirectStandardError = true;
            process.StartInfo = startInfo;
            process.Start();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Failed to get song do something
                Console.WriteLine($"Process ended with exit code: {process.ExitCode}");
                Console.Error.WriteLine(process.StandardOutput.ReadToEnd());
                return Results.NotFound();
            }
            string output = process.StandardOutput.ReadToEnd();
            Console.WriteLine("yt-dlp process exit code: " + process.ExitCode);
            
            string filename = "";

            Console.WriteLine("\n\nDONE\n\n");


            // Get song file name
            string[] files = System.IO.Directory.GetFiles("yt/");
            
            if (files.Length != 2)
                // There should always be two files, why is there two?
                return Results.BadRequest("Downloading song left more than 1 file.");

            foreach (string file in files)
            {
                Console.WriteLine($"File: {file}");
                if (file == "yt/yt-dlp.exe")
                    continue;

                filename = file;
            }

            FileInfo fileInfo = new FileInfo(filename);
            FileStream filestream = System.IO.File.OpenRead(fileInfo.FullName);
            return Results.File(filestream, contentType: "video/mp3", fileDownloadName: filename.Substring(3), enableRangeProcessing: true);

        }
    }
}
