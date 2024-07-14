async function Create() {
    HideError();
    var name = document.getElementById("name").value.trim();
    if (name === "")
        ShowError("Enter a name!");
    else {
        var response = await SendRequest(`views/create?name=${encodeURIComponent(name)}`, "POST");
        if (typeof response === "string")
            window.location.assign(`views/edit?id=${response}`);
        else if (response === 302)
            ShowError("Another view already uses this name!");
        else ShowError("Connection failed!");
    }
}