function escapeHtml(text) {
    const div = document.createElement("div");
    div.textContent = text;
    return div.innerHTML;
}

function getAllLocalStorage() {
    if (!localStorage) {
        document.getElementById("localStorage").textContent = "Local Storage API not supported";
        return;
    }

    if (localStorage.length === 0) {
        document.getElementById("localStorage").textContent = "No local storage items";
        return;
    }

    let html = "<table>";
    for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        const value = localStorage.getItem(key);
        html += `<tr><td>${escapeHtml(key)}</td><td>${escapeHtml(value)}</td></tr>`;
    }
    html += "</table>";
    document.getElementById("localStorage").innerHTML = html;
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
