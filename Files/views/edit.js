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
            case 302: ShowError("Another view already uses this name!"); break;
            default: ShowError("Connection failed!"); break;
        }
}

async function SetTemplate() {
    HideError();
    if (await SendRequest(`edit/set-template?id=${GetId()}&template=${document.getElementById("template").value}`, "POST", true) === 200)
        window.location.reload();
    else ShowError("Connection failed!");
}

async function ElementAdd(index) {
    HideError();
    var selection = document.getElementById(`add-${index}`).value;
    if (selection !== "default") {
        if (await SendRequest(`edit/add-element?id=${GetId()}&index=${index}&template=${selection}`, "POST", true) === 200)
            window.location.reload();
        else ShowError("Connection failed!");
    }
}

async function ElementDelete(index) {
    HideError();
    var deleteButton = document.getElementById(`delete-${index}`);
    if (deleteButton.innerText !== "Delete?")
        deleteButton.innerText = "Delete?";
    else if (await SendRequest(`edit/delete-element?id=${GetId()}&index=${index}`, "POST", true) === 200)
        window.location.reload();
    else ShowError("Connection failed!");
}

function ElementChanged(index) {
    HideError();
    var saveButton = document.getElementById(`save-${index}`);
    saveButton.innerText = "Save";
    saveButton.className = "green";
}

async function ElementSave(index, valuesCount) {
    HideError();
    var saveButton = document.getElementById(`save-${index}`);
    saveButton.innerText = "Saving...";
    saveButton.className = "green";
    var values = [];
    for (var i = 0; i < valuesCount; i++)
        values.push(encodeURIComponent(document.getElementById(`value-${index}-${i}`).value));
    if (await SendRequest(`edit/save-element?id=${GetId()}&index=${index}&values=${encodeURIComponent(values.join("&"))}`, "POST", true) === 200) {
        saveButton.innerText = "Saved!";
        saveButton.className = "";
        window.location.reload();
    } else {
        saveButton.innerText = "Save";
        saveButton.className = "green";
        ShowError("Connection failed!");
    }
}