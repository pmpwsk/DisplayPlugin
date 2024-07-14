function GetId() {
    return (new URLSearchParams(window.location.search)).get("id");
}

async function Delete() {
    HideError();
    var deleteButton = document.getElementById("delete");
    if (deleteButton.innerText !== "Delete?")
        deleteButton.innerText = "Delete?";
    else
        if (await SendRequest(`edit/delete?id=${GetId()}`, "POST", true) === 200)
            window.location.assign("../displays");
        else ShowError("Connection failed!");
}

async function Rename() {
    HideError();
    var name = document.getElementById("name").value.trim();
    if (name === "")
        ShowError("Enter a name!");
    else
        switch (await SendRequest(`edit/rename?id=${GetId()}&name=${encodeURIComponent(name)}`, "POST", true)) {
            case 200: window.location.reload(); break;
            case 302: ShowError("Another display already uses this name!"); break;
            default: ShowError("Connection failed!"); break;
        }
}

async function SetView() {
    HideError();
    if (await SendRequest(`edit/set-view?id=${GetId()}&view=${document.getElementById("view").value}`, "POST", true) === 200)
        window.location.reload();
    else ShowError("Connection failed!");
}

async function Refresh() {
    HideError();
    var button = document.getElementById("refresh");
    button.innerText = "Refreshing...";
    if (await SendRequest(`edit/refresh?id=${GetId()}`, "POST", true) === 200)
        button.innerText = "Refreshed!";
    else {
        button.innerText = "Refresh";
        ShowError("Connection failed!");
    }
}