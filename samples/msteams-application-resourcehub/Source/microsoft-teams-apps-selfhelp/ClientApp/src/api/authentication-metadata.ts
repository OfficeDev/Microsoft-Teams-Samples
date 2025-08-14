// <copyright file="authentication-metadata.ts" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

import axios from "./axios-decorator";
import { AxiosRequestConfig } from "axios";
import { getAPIRequestConfigParams } from "../helpers/api-helper";
import constants from "../constants/constants";

/**
* Get authentication metadata from API
* @param  {String} windowLocationOriginDomain Application base URL
* @param  {String} login_hint Login hint for SSO
*/
export const getAuthenticationConsentMetadata = async (windowLocationOriginDomain: string, login_hint: string): Promise<any> => {
    let url = `${constants.apiBaseUrl}/AuthenticationMetadata/consentUrl`;
    let config: AxiosRequestConfig = getAPIRequestConfigParams({
        windowLocationOriginDomain: windowLocationOriginDomain,
        loginhint: login_hint
    });

    return await axios.get(url, config, false);
}

/**
* Get application settinfs from API
*/
export const getApplicationSettingsMetadata = async (): Promise<any> => {
    let url = `${constants.apiBaseUrl}/AuthenticationMetadata/appsettings`;
    return await axios.get(url, undefined, false);
}