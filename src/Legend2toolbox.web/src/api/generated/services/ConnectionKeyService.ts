/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ConnectionKeyService {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiConnectionKey(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/connection-key',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiConnectionKey(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/connection-key',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiConnectionKeyClients(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/connection-key/clients',
        });
    }
}
