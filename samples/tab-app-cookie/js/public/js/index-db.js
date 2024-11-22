let globalDBInstance;
let store;
let indexDBElement;
let nameInput;
const dbStoreName = "myStore";
const dbName = "myDatabase";

function initElementReferences() {
  indexDBElement = document.getElementById("indexDB");
  nameInput = document.getElementById("name");
}

function setNoDBNotice() {
  console.error("No database connection");
  indexDBElement.innerHTML = "No database connection; create a database first.";
}

function getDatabaseEntries() {
  if (!globalDBInstance) {
    console.error("No database connection");
    return;
  }

  const store = globalDBInstance
    .transaction(dbStoreName, "readonly")
    .objectStore(dbStoreName);
  const index = store.index("id");

  const request = index.openCursor();
  let html = "<table>";

  request.onsuccess = (event) => {
    const cursor = event.target.result;
    if (cursor) {
      html += `<tr><td>${JSON.stringify(cursor.value)}</td></tr>`;
      cursor.continue();
    } else {
      html += "</table>";
      indexDBElement.innerHTML = html;
      console.log("No more entries");
    }
  };
}

function addToIndexdb() {
  if (!globalDBInstance) {
    setNoDBNotice();
    return;
  }

  const name = nameInput.value;
  const store = globalDBInstance
    .transaction(dbStoreName, "readwrite")
    .objectStore(dbStoreName);
  store.add({ name });

  store.transaction.oncomplete = (event) => {
    getDatabaseEntries();
    nameInput.value = "";
  };
}

function clearIndexdb() {
  if (!globalDBInstance) {
    setNoDBNotice();
    return;
  }

  const store = globalDBInstance
    .transaction(dbStoreName, "readwrite")
    .objectStore(dbStoreName);
  store.clear();

  store.transaction.oncomplete = (event) => {
    getDatabaseEntries();
  };
}

function destroyIndexdb() {
  if (!globalDBInstance) {
    console.error("No database connection");
    return;
  }

  globalDBInstance.close();

  window.indexedDB.databases().then((dbs) => {
    dbs.forEach((db) => {
      console.log(`Deleting database ${db.name}`);
      const deleteRequest = window.indexedDB.deleteDatabase(db.name);
      deleteRequest.onsuccess = function (e) {
        console.log("success");
        globalDBInstance = null;
        indexDBElement.innerHTML = "";
      };
      deleteRequest.onblocked = function (e) {
        console.log("blocked: " + e);
      };
      deleteRequest.onerror = function (e) {
        console.log("error: " + e);
      };
    });
  });
}

function initDatabase() {
  const request = window.indexedDB.open(dbName, 1);

  request.onerror = (event) => {
    console.error("Error opening database");
    console.error(event);
  };
  request.onsuccess = (event) => {
    console.log("Connected to database");
    globalDBInstance = event.target.result;

    getDatabaseEntries();
  };

  request.onupgradeneeded = (event) => {
    console.log("Connected to database");
    globalDBInstance = event.target.result;

    // Create a new object store
    store = globalDBInstance.createObjectStore(dbStoreName, {
      autoIncrement: true,
      keyPath: "id",
    });
    store.createIndex("id", "id", { unique: true });
    store.createIndex("name", "name", { unique: false });

    // Add some initial data
    store.transaction.oncomplete = (event) => {
      const store = globalDBInstance
        .transaction(dbStoreName, "readwrite")
        .objectStore(dbStoreName);

      // Add data to the object store
      store.add({ id: 1, name: "John" });
      console.log("Transaction complete");

      getDatabaseEntries();
    };
  };
}

function initOnLoad() {
  initElementReferences();
  initDatabase();
}
