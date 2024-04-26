setTimeout(() => {
    const element = document.getElementById("heading");
    //element.innerHTML += " hacked!";
}, 5000);

let songRequestID = -1;

async function submitURL(e) {
    e.preventDefault();
    let p = document.createElement('p')
    p.innerText = "Clicked"
    document.body.appendChild(p);

    const text = document.getElementById("youtube-url-box").value;
    console.log("Requesting song with url: " + text);

    let filename = '';

    let url = "/yt-request-song?url=" + encodeURIComponent(text);
    let result = await fetch(url)

    let data = await result.json();
    songRequestID = data.songRequestID;

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
    let a = document.createElement('a');
    a.href = blob_url;
    a.download = filename;
    a.innerText = filename;

    document.body.appendChild(a);
}



async function fetchBurgers() {
    let response = await fetch("/burgers");
    let data = await response.json();
    
    return data;
}


let button = document.getElementById('submit-url');
button.addEventListener('click', submitURL);

let clearButton = document.getElementById('clear-button');
clearButton.addEventListener('click', (ev) => {
    document.getElementById('youtube-url-box').value = "";
});

let urlBox = document.getElementById('youtube-url-box');


