function getAllLocalStorage() {
    const container = document.getElementById("localStorage");
    container.innerHTML = "";

    if (!localStorage) {
        container.textContent = "Local Storage API not supported";
        return;
    }

    if (localStorage.length === 0) {
        container.textContent = "No local storage items";
        return;
    }

    const table = document.createElement("table");
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        const value = localStorage.getItem(key);
        const row = table.insertRow();
        row.insertCell().textContent = key;
        row.insertCell().textContent = value;
    }
    container.appendChild(table);
}

function clearAllLocalStorage() {
    if (!localStorage) {
        document.getElementById("localStorage").textContent = "Local Storage API not supported";
        return;
    }

    localStorage.clear();

    getAllLocalStorage();
}

function setInLocalStorage() {
    const keyElement = document.getElementById("key");
    const valueElement = document.getElementById("value");

    if (!localStorage) {
        document.getElementById("localStorage").textContent = "Local Storage API not supported";
        return;
    }

    localStorage.setItem(keyElement.value, valueElement.value);

    getAllLocalStorage();

    keyElement.value = "";
    valueElement.value = "";
}