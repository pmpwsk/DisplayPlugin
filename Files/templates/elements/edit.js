function GetId() {
    return (new URLSearchParams(window.location.search)).get("id");
}

async function Save() {
    HideError();
    var saveButton = document.getElementById("save");
    saveButton.innerText = "Saving...";
    saveButton.className = "green";
    if (await SendRequest(`edit/save?id=${GetId()}&code=${encodeURIComponent(document.getElementById("code").value)}`, "POST", true) === 200) {
        saveButton.innerText = "Saved!";
        saveButton.className = "";
        window.location.reload();
    } else {
        saveButton.innerText = "Save";
        saveButton.className = "green";
        ShowError("Connection failed!");
    }
}

function CodeChanged() {
    HideError();
    var saveButton = document.getElementById("save");
    saveButton.innerText = "Save";
    saveButton.className = "green";
}

async function Delete() {
    HideError();
    var deleteButton = document.getElementById("delete");
    if (deleteButton.innerText !== "Delete?")
        deleteButton.innerText = "Delete?";
    else
        if (await SendRequest(`edit/delete?id=${GetId()}`, "POST", true) === 200)
            window.location.assign("../elements");
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
            case 302: ShowError("Another element template already uses this name!"); break;
            default: ShowError("Connection failed!"); break;
        }
}

async function ComponentAdd(index) {
    HideError();
    var selection = document.getElementById(`add-${index}`).value;
    if (selection !== "default") {
        if (await SendRequest(`edit/add-component?id=${GetId()}&index=${index}&type=${selection}`, "POST", true) === 200)
            window.location.reload();
        else ShowError("Connection failed!");
    }
}

async function ComponentDelete(index) {
    HideError();
    var deleteButton = document.getElementById(`delete-${index}`);
    if (deleteButton.innerText !== "Delete?")
        deleteButton.innerText = "Delete?";
    else if (await SendRequest(`edit/delete-component?id=${GetId()}&index=${index}`, "POST", true) === 200)
        window.location.reload();
    else ShowError("Connection failed!");
}

function ComponentChanged(index) {
    HideError();
    var saveButton = document.getElementById(`save-${index}`);
    saveButton.innerText = "Save";
    saveButton.className = "green";
}

async function ComponentSave(index) {
    HideError();
    var name = document.getElementById(`name-${index}`).value.trim();
    if (name === "") {
        ShowError("Enter a name!");
        return;
    }
    var saveButton = document.getElementById(`save-${index}`);
    saveButton.innerText = "Saving...";
    saveButton.className = "green";
    var typesInput = document.getElementById(`types-${index}`);
    if (await SendRequest(`edit/save-component?id=${GetId()}&index=${index}&name=${name}${(typesInput == null ? "" : `&types=${typesInput.value}`)}`, "POST", true) === 200) {
        saveButton.innerText = "Saved!";
        saveButton.className = "";
        window.location.reload();
    } else {
        saveButton.innerText = "Save";
        saveButton.className = "green";
        ShowError("Connection failed!");
    }
}