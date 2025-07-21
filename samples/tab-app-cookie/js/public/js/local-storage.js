function getAllLocalStorage() {
    if (!localStorage) {
        document.getElementById("localStorage").innerHTML = "Local Storage API not supported";
        return;
    }

    if (localStorage.length === 0) {
        document.getElementById("localStorage").innerHTML = "No local storage items";
        return;
    }

    let html = "<table>";
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        const value = localStorage.getItem(key);
        html += `<tr><td>${key}</td><td>${value}</td></tr>`;
    }
    html += "</table>";
    document.getElementById("localStorage").innerHTML = html;
}

function clearAllLocalStorage() {
    if (!localStorage) {
        document.getElementById("localStorage").innerHTML = "Local Storage API not supported";
        return;
    }

    localStorage.clear();

    getAllLocalStorage();
}

function setInLocalStorage() {
    const keyElement = document.getElementById("key");
    const valueElement = document.getElementById("value");

    if (!localStorage) {
        document.getElementById("localStorage").innerHTML = "Local Storage API not supported";
        return;
    }

    localStorage.setItem(keyElement.value, valueElement.value);

    getAllLocalStorage();

    keyElement.value = "";
    valueElement.value = "";
}