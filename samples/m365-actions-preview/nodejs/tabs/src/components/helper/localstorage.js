const storageKey = "m365store";

/**
 * @param item
 * Sets the data in the localStorage
*/
export function setValuesToLocalStorage(item) {
    const storeData = [];
    // Checking if item has values 
    if (item) {
        //Getting pre-existing data from localstorage
        const retrievedData = localStorage.getItem(storageKey);
        if (retrievedData) {

            const parsedData = JSON.parse(retrievedData);
            if (!isDataExist(item, parsedData)) {
                parsedData.storeData.push(item);

            } else {
                const index = parsedData.storeData.findIndex(x => x.id === item.id);
                parsedData.storeData.splice(index, 1, item);
            }

            const jsonString = JSON.stringify(parsedData);
            localStorage.setItem(storageKey, jsonString)
        }
        else {
            // saves data for the first time if no data exist in the localStorage
            storeData.push(item);
            const storeAll = {
                storeData: storeData
            }
            const jsonString = JSON.stringify(storeAll);
            localStorage.setItem(storageKey, jsonString)
        }
    }
}
/**
 * @param item
 * @param retrievedData
 * checks and returns boolean if item exist in retrieved data from localStorage
*/
export function isDataExist(item, retrievedData) {
    const val = retrievedData.storeData.find(data => data.id === item.id);
    if (val) {
        return true;
    }
    return false;
}
/**
 * Returns data info from localStorage
*/
export function getItemsFromLocalStorage() {
    const storeData = []
    const retrievedData = localStorage.getItem(storageKey);
    
    if (retrievedData) {
        return JSON.parse(retrievedData).storeData;
    }
    return storeData;
}
/**
 * Updates state of completion and description of the Task
 * @param id 
 * @param description 
 * @param isCompleted 
 */
export function updateItem(id, description, isCompleted) {
    const data = getItemsFromLocalStorage();
    const filteredData = data.find(x => x.id === id);
    console.log(id, description, isCompleted)

    if (isCompleted === false || isCompleted === true) {
        filteredData.isCompleted = isCompleted === true ? 1 : 0;
    }
    if (description) {
        filteredData.description = description;
    }
    console.log(filteredData)
    setValuesToLocalStorage(filteredData);
}
/**
 * Delete the task based on id
 * @param {string} id 
 */
export function onDeleteItem(id) {
    const storeData = getItemsFromLocalStorage();
    const index = storeData.findIndex(x => x.id === id);
    storeData.splice(index, 1);
    const storeAll = {
        storeData: storeData
    }
    const jsonString = JSON.stringify(storeAll);
    localStorage.setItem(storageKey, jsonString)
}
