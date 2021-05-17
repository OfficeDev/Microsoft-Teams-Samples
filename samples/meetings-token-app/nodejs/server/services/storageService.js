const Memorystore = require('memorystorage');
// const store = new Memorystore('my-token-app');
const store = new Memorystore(process.env.clientId);

storeSave = (key, value)=>{
    store.setItem (key, parseWhileSaving(value));
    return parseWhileRetriving (store.getItem(key));
}

storeFetch = (key)=>{
    return parseWhileRetriving (store.getItem(key));
}

storeRemove = (key)=>{
    store.removeItem(key);
}

storeUpdate = (key, value)=>{
    store.removeItem(key);
    store.setItem (key, parseWhileSaving(value));
    return parseWhileRetriving (store.getItem(key));
}

storeLength = ()=>{
    return store.length;
}

storeCheck = (key)=>{
    return !!parseWhileRetriving (store.getItem(key));
}

parseWhileSaving = (data) => {
    let parsedData;
    try {
        parsedData = JSON.parse(data)
    } catch (e) {
        parsedData = data
    }
    return parsedData;
}

parseWhileRetriving = (data) => {
    let parsedData;
    try {
        parsedData = JSON.parse(data)
    } catch (e) {
        parsedData = data
    }
    return parsedData;
}

module.exports ={
    storeSave,
    storeFetch,
    storeRemove,
    storeUpdate,
    storeLength,
    storeCheck
}
