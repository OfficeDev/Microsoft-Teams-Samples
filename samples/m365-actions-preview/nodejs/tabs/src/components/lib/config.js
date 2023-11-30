const config = {
    initiateLoginEndpoint: process.env.REACT_APP_START_LOGIN_PAGE_URL,
    clientId: process.env.REACT_APP_CLIENT_ID,
    apiEndpoint: process.env.REACT_APP_FUNC_ENDPOINT,
    localStorage: process.env.REACT_APP_LCL_STRG,
    scopes: ["User.Read", "User.ReadBasic.All", "Files.Read"]
}

export default config;
