class TokenStore {
    constructor() {
        this.tokens = new Map();
    }

    setToken(userId, token) {
        this.tokens.set(userId, token);
    }

    getToken(userId) {
        return this.tokens.get(userId) || null;
    }

    removeToken(userId) {
        this.tokens.delete(userId);
    }

    static getInstance() {
        if (!TokenStore.instance) {
            TokenStore.instance = new TokenStore();
        }
        return TokenStore.instance;
    }
}

module.exports = TokenStore.getInstance();