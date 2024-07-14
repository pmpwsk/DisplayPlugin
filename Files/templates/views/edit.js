let saveButton = document.getElementById("save");

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
            window.location.assign("../views");
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
            case 302: ShowError("Another view template already uses this name!"); break;
            default: ShowError("Connection failed!"); break;
        }
}

function Changed() {
    HideError();
    saveButton.innerText = "Save";
    saveButton.className = "green";
}

async function Save() {
    HideError();
    if (await SendRequest(`edit/save?id=${GetId()}&${Pair("before-elements")}&${Pair("before-each-element")}&${Pair("after-elements")}`, "POST", true) === 200) {
        saveButton.innerText = "Saved!";
        saveButton.className = "";
    } else ShowError("Connection failed!");
}

function Pair(id) {
    return `${id}=${encodeURIComponent(document.getElementById(`${id}`).value)}`;
}