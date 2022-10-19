import axios, { AxiosResponse, AxiosRequestConfig } from "axios";
import * as microsoftTeams from "@microsoft/teams-js";

export class AxiosJWTDecorator {
	/**
	* POST data to API
	* @param  {String} url Resource URI
	* @param  {Object} data Request body data
	*/
    public async post<T = any, R = AxiosResponse<T>>(
        url: string,
        data?: any,
        config?: AxiosRequestConfig,
        needAuthorizationHeader: boolean = true
    ): Promise<R> {
        try {
            if (needAuthorizationHeader) {
                config = await this.setupAuthorizationHeader(config);
            }
            return await axios.post(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* DELETE data to API
	* @param  {String} url Resource URI
	*/
    public async delete<T = any, R = AxiosResponse<T>>(
        url: string,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(config);
            return await axios.delete(url, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* PUT data to API
	* @param  {String} url Resource URI
	* @param  {Object} data Request body data
	*/
    public async put<T = any, R = AxiosResponse<T>>(
        url: string,
        data?: any,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(config);
            return await axios.put(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

    /**
    * PATCH data to API
    * @param  {String} url Resource URI
    * @param  {Object} data Request body data
    */
    public async patch<T = any, R = AxiosResponse<T>>(
        url: string,
        data?: any,
        config?: AxiosRequestConfig
    ): Promise<R> {
        try {
            config = await this.setupAuthorizationHeader(config);
            return await axios.patch(url, data, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

	/**
	* GET data to API
    * @param {String} url Resource URI
	*/
    public async get<T = any, R = AxiosResponse<T>>(
        url: string,
        config?: AxiosRequestConfig,
        needAuthorizationHeader: boolean = true
    ): Promise<R> {
        try {
            if (needAuthorizationHeader) {
                config = await this.setupAuthorizationHeader(config);
            }
            return await axios.get(url, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

    /**
     * GET data to API
     * @param url Resource URI
     */
    public async getCachedData<T = any, R = AxiosResponse<T>>(
        url: string,
        config?: AxiosRequestConfig,
        needAuthorizationHeader: boolean = true
    ): Promise<R> {
        try {
            if (needAuthorizationHeader) {
                config = await this.setupAuthorizationHeader(config);
                config.headers["Cache-Control"] = "max-age=1209600"
            }
            return await axios.get(url, config);
        } catch (error) {
            return Promise.resolve(this.handleError(error));
        }
    }

    /**
	* Handle error occurred during API call.
	* @param  {Object} error Error response object
	*/
    private handleError(error: any): any {
        if (error.hasOwnProperty("response")) {
            return error.response;
        } else {
            return error;
        }
    }

    private async setupAuthorizationHeader(
        config?: AxiosRequestConfig
    ): Promise<AxiosRequestConfig> {
        microsoftTeams.initialize();

        return new Promise<AxiosRequestConfig>((resolve, reject) => {
            const authTokenRequest = {
                successCallback: (token: string) => {
                    if (!config) {
                        config = axios.defaults;
                    }
                    config.headers["Authorization"] = `Bearer ${token}`;
                    resolve(config);
                },
                failureCallback: (error: string) => {
                    console.error("Error from getAuthToken: ", error);
                    window.location.href = `/signin?redirectUrl=${encodeURIComponent(window.location.href)}`;
                },
                resources: []
            };
            microsoftTeams.authentication.getAuthToken(authTokenRequest);
        });
    }
}

const axiosJWTDecoratorInstance = new AxiosJWTDecorator();
export default axiosJWTDecoratorInstance;