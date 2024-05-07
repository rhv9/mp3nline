let songRequestID = -1;

var containerResult = document.getElementsByClassName("container-results")[0]
var containerResultData = containerResult.getElementsByClassName("container-results-data")[0];
var mainProgressBar;



// --- Progress Bar ----

class ProgressBar {
    constructor() {
        this.parentElem = document.createElement("div");
        this.parentElem.className = "pb-grey";
        this.pbbar = document.createElement("div");
        this.pbbar.className = "pb-bar";
        this.parentElem.appendChild(this.pbbar);
        this.enabledID = -1;
    }


    enableClassic() {
        if (this.enabledID != -1)
            return;

        let pbXTransform = 0;
        this.enabledID = setInterval(() => {
            this.pbbar.style.transform = `translate(${pbXTransform}%)`
            pbXTransform += 2;

            if (pbXTransform > 500)
                pbXTransform = -100;
        }, 16.7);
    }
    enableLeftRight() {
        if (this.enabledID != -1)
            return;

        this.pbbar.style.transition = "1s ease-in-out";
        let pbXTransform = true;
        this.enabledID = setInterval(() => {
            if (pbXTransform)
                this.pbbar.style.transform = `translate(${500}%)`
            else
                this.pbbar.style.transform = `translate(${-100}%)`
            pbXTransform = !pbXTransform;
        }, 1000);
    }

    disableAnimation() {
        if (this.enabledID)
            clearInterval(this.enabledID);

        this.pbbar.style.transition = "";
        this.pbbar.style.transform = `translate(${-100}%)`;

        this.enabledID = -1;
    }

    remove() {
        this.parentElem.remove();
    }

    setShow(show) {
        this.parentElem.style.display = show ? "block" : "none";
    }


}

class VideoResults {

    static SetTitle(title) {
        document.getElementsByClassName("span-title")[0].textContent = title;
    }

    /**
     * @param {string} duration
     */
    static SetDuration(duration) {
        var durString = duration.substring(0, 1) == "00" ? duration + " hours" : duration.substring(3) + " minutes";
        document.getElementById('span-duration').textContent = durString;
    }

    static SetImg(url) {
        document.getElementById('img-video').src = url;
    }

    static SetFromVideoData(videoData) {
        VideoResults.SetTitle(videoData.title);
        VideoResults.SetDuration(videoData.duration);
        
        if (videoData.thumbnails.maxres != null)
            VideoResults.SetImg(videoData.thumbnails.maxres.url);
        else if (videoData.thumbnails.high != null)
            VideoResults.SetImg(videoData.thumbnails.high.url);
        else if (videoData.thumbnails.medium != null)
            VideoResults.SetImg(videoData.thumbnails.medium.url);
        else if (videoData.thumbnails.standard != null)
            VideoResults.SetImg(videoData.thumbnails.standard.url);
    }
}

/**
 * @param {boolean} state
 */
function resultContainerActive(active) {
    document.getElementsByClassName("container-results")[0]
        .style.display = active ? "block" : "none";

}

function resultContainerDataActive(active) {
    document.getElementsByClassName("container-results-data")[0]
        .style.display = active ? "block" : "none";

}

let SongTitle = "notset";

async function submitURL(e) {
    e.preventDefault();

    // make result container visible
    resultContainerActive(true);
    mainProgressBar.setShow(true);
    mainProgressBar.enableLeftRight();

    const text = document.getElementById("youtube-url-box").value;
    console.log("Requesting song with url: " + text);

    let filename = '';

    let url = "/yt-request-song?url=" + encodeURIComponent(text);
    let result = await fetch(url)

    let data = await result.json();

    songRequestID = data.songRequestID;
    console.log(data.videoData);
    SongTitle = data.videoData.title;
    VideoResults.SetFromVideoData(data.videoData);

    resultContainerDataActive(true);
    mainProgressBar.setShow(false);
    document.getElementById("btn-download").disabled = true;
    document.getElementById("btn-download").innerText = "Processing...";
    PingForSong();
}

async function PingForSong() {
    if (songRequestID == -1) {
        console.log("PingForSong called before getting a songRequestID. This should never happen.");
        return;
    }
    while (true) {
        let url = "/yt-ping-song-status?id=" + songRequestID;
        let response = await fetch(url);
        let status = await response.json();

        console.log("Pinged for status")
        console.log(status)

        if (status.status == "Processing") {
        }
        else if (status.status == "Failed") {
            console.log("Error: Song request failed?");
            break;
        }
        else if (status.status == "Finished") {
            DownloadSong()
            break;
        }
        else {
            console.log("This should never run. No status for PingForSong");
        }
            
        await new Promise(resolve => setTimeout(resolve, 500));
    }
}

let a;

async function DownloadSong() {
    console.log("Downloading Song ID: " + songRequestID);

    let url = "/yt-get-finished-song?id=" + songRequestID;
    let result = await fetch(url);

    const header = result.headers.get('Content-Disposition');
    const parts = header.split(';');

    const filename = decodeURIComponent(parts[2].split('=')[1]).slice(7);

    console.log("Filename: " + filename);

    let blob = await result.blob();

    console.log('Got blob')

    let blob_url = window.URL.createObjectURL(blob);
    a = document.createElement('a');
    a.href = blob_url;
    a.style.display = "none";
    a.download = SongTitle + ".mp3";
    a.innerText = SongTitle + ".mp3";


    document.getElementById("btn-download").disabled = false;
    document.getElementById("btn-download").innerText = "Download :D";
}

function OnClickBtnDownload(ev) {
    console.log("CLICKED!!");
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
}

document.getElementById('btn-download').addEventListener('click', OnClickBtnDownload);


let downloadButton = document.getElementById('submit-url');
downloadButton.addEventListener('click', submitURL);

let clearButton = document.getElementById('clear-button');
clearButton.addEventListener('click', (ev) => {
    document.getElementById('youtube-url-box').value = "";
});



mainProgressBar = new ProgressBar();
containerResult.insertBefore(mainProgressBar.parentElem, containerResultData);
