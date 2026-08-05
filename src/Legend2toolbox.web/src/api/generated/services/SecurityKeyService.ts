/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class SecurityKeyService {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static postApiSecurityKey(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/security-key',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiSecurityKey(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/security-key',
        });
    }
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiSecurityKeyClients(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/security-key/clients',
        });
    }
}
