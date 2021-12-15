const Memorystore = require('memorystorage');
const store = new Memorystore(process.env.MicrosoftAppId);

const storeSave = (key, value) => {
    store.setItem(key, parseWhileSaving(value));
    return parseWhileRetriving(store.getItem(key));
};

const storeFetch = (key) => {
    return parseWhileRetriving(store.getItem(key));
};

const storeRemove = (key) => {
    store.removeItem(key);
};

const storeUpdate = (key, value) => {
    store.removeItem(key);
    store.setItem(key, parseWhileSaving(value));
    return parseWhileRetriving(store.getItem(key));
};

const storeLength = () => {
    return store.length;
};

const storeCheck = (key) => {
    return !!parseWhileRetriving(store.getItem(key));
};

const parseWhileSaving = (data) => {
    let parsedData;
    try {
        parsedData = JSON.parse(data);
    } catch (e) {
        parsedData = data;
    }
    return parsedData;
};

const parseWhileRetriving = (data) => {
    let parsedData;
    try {
        parsedData = JSON.parse(data);
    } catch (e) {
        parsedData = data;
    }
    return parsedData;
};

module.exports = {
    storeSave,
    storeFetch,
    storeRemove,
    storeUpdate,
    storeLength,
    storeCheck
};
