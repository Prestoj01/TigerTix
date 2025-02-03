function searchEvents() {
    let criteria = document.getElementById("searchCriteria").value.toLowerCase();
    let list = document.getElementById("eventList");
    let items = list.querySelectorAll('li');

    items.forEach(event => {
        const title = event.querySelector(".event-title").textContent.toLowerCase();
        if (title.includes(criteria)) { 
            event.style.display = ""; // if the search contains the name of the available events
        }
        else {
            event.style.display = "none";
        }
    });
}