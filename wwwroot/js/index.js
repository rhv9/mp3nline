setTimeout(() => {
    const element = document.getElementById("heading");
    element.innerHTML += " hacked!";
}, 5000);


function submitURL1() {
    const text = document.getElementById("youtube-url-box").value;

    //console.log("Fetching burgers...");
    //let data = fetchBurgers().then(data => {
    //    console.log(data[0]);
    //});
    let filename = '';

    console.log('fetching...')

    let url = "/yt-get-song?url=" + text;
    fetch(url)
        .then(res => {
            console.log('DOES THIS EVEN WORK?')
            const header = res.headers.get('Content-Disposition');
            const parts = header.split(';');
            filename = parts[1].split('=')[1];
            console.log("Filename: " + filename);
            return res.blob();
        })
        .then(blob => {
            let file = window.URL.createObjectURL(blob);
            let a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            
        });
}


async function submitURL(e) {
    e.preventDefault();
    console.log("DOES CONSOLE EVEN PRINT???");
    const text = document.getElementById("youtube-url-box").value;

    let filename = '';

    console.log('fetching...')

    let url = "/yt-get-song?url=" + encodeURIComponent(text);
    let result = await fetch(url)

    console.log('DOES THIS EVEN WORK?')

    const header = result.headers.get('Content-Disposition');
    const parts = header.split(';');

    filename = parts[1].split('=')[1];
    console.log("Filename: " + filename);
    let blob = await result.blob();

    console.log('Got blob')

    let file = window.URL.createObjectURL(blob);
    let a = document.createElement('a');
    a.href = url;
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


let urlBox = document.getElementById('youtube-url-box');
urlBox.value = "https://www.youtube.com/watch?v=utB8vYW06rI&list=PLeEG5MBW9D2TMK5jTZifHm5QXCpLDyA96&index=5";

