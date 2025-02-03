function sortEvents() {
    let criteria = document.getElementById("sortCriteria").value;
    let list = document.getElementById("eventList");
    let items = list.querySelectorAll('li');
    let itemsArr = Array.from(items);

    if (criteria === "location") {
        itemsArr.sort((a, b) => {
            let locA = a.innerText.split("Location: ")[1].split('\n')[0].trim(); // Corrected to fetch location text correctly
            let locB = b.innerText.split("Location: ")[1].split('\n')[0].trim(); // Corrected to fetch location text correctly
            return locA.localeCompare(locB);
        });
    } else if (criteria === "date") {
        itemsArr.sort((a, b) => {
            let dateA = new Date(a.querySelector('.event-date').innerText.split(": ")[1].trim());
            let dateB = new Date(b.querySelector('.event-date').innerText.split(": ")[1].trim());
            return dateA - dateB;
        });
    }

    for (var i = 0; i < itemsArr.length; i++) {
        list.appendChild(itemsArr[i]); // Re-append each item in the sorted order
    }
}

document.addEventListener('DOMContentLoaded', function () {
    sortEvents(); // Sort by default criteria on load
});
