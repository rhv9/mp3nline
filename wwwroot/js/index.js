setTimeout(() => {
    const element = document.getElementById("heading");
    element.innerHTML += " hacked!";
}, 5000);


function submitURL() {
    const text = document.getElementById("youtube-url-box").value;

    //console.log("Fetching burgers...");
    //let data = fetchBurgers().then(data => {
    //    console.log(data[0]);
    //});
    let url = "/yt-get-song?url=" + text;
    fetch(url)
        .then(res => res.blob())
        .then(blob => {
            let file = window.URL.createObjectURL(blob);
        });
}

async function fetchBurgers() {
    let response = await fetch("/burgers");
    let data = await response.json();
    
    return data;
}

