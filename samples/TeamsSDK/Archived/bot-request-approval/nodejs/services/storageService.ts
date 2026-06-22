const store = new Map<string, any>();

export const storeSave = (key: string, value: any): any => {
    store.set(key, value);
    return store.get(key);
};

export const storeFetch = (key: string): any => {
    return store.get(key);
};

export const storeUpdate = (key: string, value: any): any => {
    store.set(key, value);
    return store.get(key);
};

export const storeCheck = (key: string): boolean => {
    return store.has(key) && !!store.get(key);
};
