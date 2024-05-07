using System.IO;
using System.Collections.Generic;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Diagnostics;
using System.Xml;
using Microsoft.AspNetCore.WebUtilities;
using System.Net.Http;

using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Hosting.Internal;

namespace youtube_dl_api
{

    //./yt-dlp.exe --skip-download --print "%(duration>%H:%M:%S.%s)s %(creator)s %(uploader)s - %(title)s" https://www.youtube.com/watch?v=v2H4l9RpkwM

    // ./yt-dlp.exe -x --audio-format mp3 -o "%(title)s.%(ext)s" [URL]
    // ./yt-dlp.exe -x --audio-format mp3 --no-playlist -o "%(title)s.%(ext)s" "


    public class YoutubeManager
    {
        public readonly struct YTVideoData
        {
            public string Id { get; }
            public string Title { get; }
            public TimeSpan Duration { get; }

            public ThumbnailDetails Thumbnails { get; }

            public YTVideoData(string id, string title, TimeSpan duration, ThumbnailDetails thumbnails)
            {
                Id = id;
                Title = title;
                Duration = duration;
                Thumbnails = thumbnails;
            }

        }
        private static readonly HttpClient client = new HttpClient();

        public static string? YTAPI_KEY;

        private static string ytDlpFile = "notset";
        private static OSPlatform platform;

        public static int IdPos = 0;
        public static List<int> ProcessingList = new List<int>();
        public static Dictionary<int, string> FinishedList = new Dictionary<int, string>();
        public static List<int> FailedList = new List<int>();

        public static int GenId() { return IdPos++; }

        public static void SetOs(OSPlatform platform)
        {
            YoutubeManager.platform = platform;
            if (platform.Equals(OSPlatform.Windows))
            {
                ytDlpFile = "";
            }
            else if (platform.Equals(OSPlatform.Linux))
            {
                ytDlpFile = "./yt-dlp_linux ";
            }
        }

        public static IResult GetFinishedSong(int newId)
        {
            if (!FinishedList.ContainsKey(newId))
                return Results.NotFound("Finished Song ID not found!");

            string filename = FinishedList[newId];
            FinishedList.Remove(newId);
            FileInfo fileInfo = new FileInfo(filename);
            FileStream filestream = File.OpenRead(fileInfo.FullName);
            return Results.File(filestream, contentType: "video/mp3", fileDownloadName: filename.Substring(3 + 20), enableRangeProcessing: true);

        }
        public static YTVideoData? GetVideoData(string url)
        {
            if (YTAPI_KEY == null)
                return null;

            Regex rx = new Regex(@"((?<=(v|V)/)|(?<=be/)|(?<=(\?|\&)v=)|(?<=embed/))([\w-]+)");
            Match match = rx.Match(url);
            if (!match.Success)
                return null;

            string videoId = match.Value;

            YouTubeService youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = YTAPI_KEY,
                ApplicationName = "YTM"
            });

            var request = youtubeService.Videos.List("snippet,contentDetails");
            request.Id = videoId;
            VideoListResponse response = request.Execute();

            string title = response.Items[0].Snippet.Title;
            ThumbnailDetails thumbnails = response.Items[0].Snippet.Thumbnails;
            string duration = response.Items[0].ContentDetails.Duration;
            TimeSpan ts = XmlConvert.ToTimeSpan(duration);

            Console.WriteLine("Title: " + title);

            return new YTVideoData(videoId, title, ts, thumbnails);
        }

        public static void DownloadSong(string url, int newId)
        {
            Console.WriteLine("IS THIS FUKCING RUNNING???");

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Process process = new Process();
            //startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            // todo extracting audio when file already exists causes errors



            DateTime time = DateTime.Now;
            string timeString = time.ToString("dd-MM-yyyy_HH_mm_ss_");
            process.StartInfo.FileName = platform == OSPlatform.Windows ? "cmd.exe" : "/bin/bash";
            if (platform.Equals(OSPlatform.Windows))
                process.StartInfo.Arguments = $"/C cd yt && yt-dlp.exe -x --audio-format mp3 --no-playlist --embed-metadata -o \"{timeString}%(title)s.%(ext)s\" \"" + url + "\"";
            else if (platform.Equals(OSPlatform.Linux))
                process.StartInfo.Arguments = $"-c \"mkdir -p yt; cd yt; ./yt-dlp_linux  -x --audio-format mp3 --no-playlist --embed-metadata -o '{timeString}%(title)s.%(ext)s' '" + url + "'\"";
            //process.StartInfo.RedirectStandardOutput = true;
            //process.StartInfo.RedirectStandardInput = true;
            //process.StartInfo.RedirectStandardError = true;

            //process.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            //process.OutputDataReceived += new DataReceivedEventHandler(MyProcOutputHandler);
            process.Start();
            //process.BeginOutputReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            { 
                // Failed to get song do something
                Console.WriteLine($"Process ended with exit code: {process.ExitCode}");
                Console.Error.WriteLine("Process Output: " + process.StandardOutput.ReadToEnd());

                FailedList.Add(newId);
                ProcessingList.Remove(newId);
                return;
            }
            string filename = "";

            Console.WriteLine($"[Finish]: Download of song of ID: {newId}");

            // Get song file name
            string[] files = Directory.GetFiles("yt/");

            foreach (string file in files)
            {
                if (file.Contains(timeString))
                {
                    filename = file;
                }
            }

            if (filename == "")
            {
                Console.WriteLine("Failed to find file!");
                FailedList.Add(newId);
                return;
            }
            Console.WriteLine($"Filename: {filename}");

            FinishedList.Add(newId, filename);
            ProcessingList.Remove(newId);
        }

        private static void MyProcOutputHandler(object sendingProcess,
            DataReceivedEventArgs outLine)
        {
            // Collect the sort command output. 
            if (!string.IsNullOrEmpty(outLine.Data))
            {
                Console.WriteLine($"Process Output: {outLine.Data}");
            }
        }

        public static async Task<IResult> RequestDownloadSongAsync(string url)
        {
            Console.WriteLine($"Requesting Download Song URL: {url}");

            int newId = GenId();
            Console.WriteLine("New Song Request ID: " + newId);

            ProcessingList.Add(newId);

            Dictionary<string, string> values = new Dictionary<string, string>
            {
                { "url" , url },
                { "newId", newId + "" }
            };
            string toSend = QueryHelpers.AddQueryString("http://localhost:8080/private-download-song", values);
            Console.WriteLine($"Sending to: {toSend}");
            client.GetAsync(toSend);

            YTVideoData? videoData = GetVideoData(url);
            if (videoData != null)
            {
                return Results.Ok(new { SongRequestID = newId, VideoData = videoData });
            }

            return Results.Ok(new { SongRequestID = newId });
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
