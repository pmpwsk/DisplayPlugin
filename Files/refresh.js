let incomingEvent = new EventSource(`refresh-event${window.location.search}`);
onbeforeunload = (event) => { incomingEvent.close(); };
incomingEvent.onmessage = (event) => {
    if (event.data === "refresh")
        window.location.reload();
};