async function Upload() {
    HideError();
    var files = document.getElementById("files").files;
    if (files.length === 0) {
        ShowError("No files selected!");
        return;
    }
    var form = new FormData();
    for (var f of files)
    form.append("file", f);
    var request = new XMLHttpRequest();
    request.open("POST", "files/upload");
    request.upload.addEventListener("progress", event => {
        document.getElementById("upload").innerText = `${((event.loaded / event.total) * 100).toFixed(2)}%`;
    });
    request.onreadystatechange = () => {
        if (request.readyState == 4) {
            switch (request.status) {
                case 200:
                    document.getElementById("upload").innerText = 'Done!';
                    window.location.reload();
                    break;
                default:
                    document.getElementById("upload").innerText = 'Upload';
                    ShowError("Connection failed. A possible cause is that at least one of the selected files might be too large.");
                    break;
            }
        }
    };
    request.send(form);
}

async function Delete(key) {
    HideError();
    var button = document.getElementById(`delete-${key}`);
    if (button.innerText !== "Delete?")
        button.innerText = "Delete?";
    else if (await SendRequest(`files/delete?key=${key}`, "POST", true) === 200)
        window.location.reload();
    else ShowError("Connection failed.");
}