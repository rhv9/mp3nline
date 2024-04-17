using System.IO;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;

namespace youtube_dl_api.youtubemanager
{

    //./yt-dlp.exe --skip-download --print "%(duration>%H:%M:%S.%s)s %(creator)s %(uploader)s - %(title)s" https://www.youtube.com/watch?v=v2H4l9RpkwM

    // ./yt-dlp.exe -x --audio-format mp3 -o "%(title)s.%(ext)s" [URL]
    // ./yt-dlp.exe -x --audio-format mp3 --no-playlist -o "%(title)s.%(ext)s" "


    public class YoutubeManager
    {
        public static int IdPos = 0;
        public static List<int> ProcessingList = new List<int>();
        public static Dictionary<int, string> FinishedList = new Dictionary<int, string>();
        public static List<int> FailedList = new List<int>();

        public static int GenId() { return IdPos++; }

        public static IResult GetFinishedSong(int newId)
        {
            if (!FinishedList.ContainsKey(newId))
                return Results.NotFound("Finished Song ID not found!");
            
            string filename = FinishedList[newId];
            FinishedList.Remove(newId);
            FileInfo fileInfo = new FileInfo(filename);
            FileStream filestream = System.IO.File.OpenRead(fileInfo.FullName);
            return Results.File(filestream, contentType: "video/mp3", fileDownloadName: filename.Substring(3), enableRangeProcessing: true);

        }
        public static void DownloadSong(string url, int newId)
        {
            

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            System.Diagnostics.Process process = new System.Diagnostics.Process();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            // todo extracting audio when file already exists causes errors
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/C cd yt && yt-dlp.exe -x --audio-format mp3 --no-playlist -o \"%(title)s.%(ext)s\" \"" + url + "\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            process.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // Failed to get song do something
                Console.WriteLine($"Process ended with exit code: {process.ExitCode}");
                Console.Error.WriteLine(process.StandardOutput.ReadToEnd());

                FailedList.Add(newId);
                return;
            }
            string output = process.StandardOutput.ReadToEnd();
            string filename = "";

            Console.WriteLine($"[Finish]: Download of song of ID: {newId}");

            // Get song file name
            string[] files = System.IO.Directory.GetFiles("yt/");

            if (files.Length != 2)
            {
                // There should always be two files, why is there two?
                FailedList.Add(newId);
                return;
            }
                
            foreach (string file in files)
            {
                if (file == "yt/yt-dlp.exe")
                    continue;

                filename = file;
            }
            Console.WriteLine($"Filename: {filename}");

            FinishedList.Add(newId, filename);
            ProcessingList.Remove(newId);
        }

        private static void MyProcOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine($"Process Output: {outLine.Data}");
            }
        }

        public static IResult RequestDownloadSong(string url)
        {
            Console.WriteLine($"Requesting Download Song URL: {url}");

            int newId = GenId();
            Console.WriteLine("New Song Request ID: " + newId);

            ProcessingList.Add(newId);

            Task.Run(() => { DownloadSong(url, newId); });

            return Results.Ok(new {SongRequestID = newId});
        }

        public static IResult GetSongStatus(int id)
        {
            Console.WriteLine($"------------ID: {id}");
            Console.WriteLine("ProcessingList:");
            ProcessingList.ForEach(p => { Console.WriteLine(p); });
            Console.WriteLine("FinishedList:");
            foreach ((int key, string value) in FinishedList)
                Console.WriteLine($"{key}: {value}");
            Console.WriteLine("FailedList:");
            FailedList.ForEach(p => { Console.WriteLine(p); });

            if (ProcessingList.Contains(id))
            {
                return Results.Ok(new { Status = "Processing" });
            }
            if (FinishedList.ContainsKey(id))
            {
                return Results.Ok(new { Status = "Finished", SongRequestID = id });
            }
            if (FailedList.Contains(id))
            {
                FailedList.Remove(id);
                return Results.Ok(new { Status = "Failed" });
            }
            return Results.NotFound(new { Status = "NotFound" });
        }
    }
}
