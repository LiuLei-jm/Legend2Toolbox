/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateScriptFileRequest } from '../models/CreateScriptFileRequest';
import type { UpdateScriptFileRequest } from '../models/UpdateScriptFileRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ScriptFileService {
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiScriptsFiles(
        requestBody: CreateScriptFileRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/scripts/files',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static putApiScriptsFiles(
        id: string,
        requestBody: UpdateScriptFileRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/scripts/files/{id}',
            path: {
                'id': id,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiScriptsFiles(
        id: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/scripts/files/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static getApiScriptsFiles(
        id: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/scripts/files/{id}',
            path: {
                'id': id,
            },
        });
    }
    /**
     * @param scriptSetId
     * @returns any OK
     * @throws ApiError
     */
    public static getApiScriptsFilesBySet(
        scriptSetId: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/scripts/files/by-set/{scriptSetId}',
            path: {
                'scriptSetId': scriptSetId,
            },
        });
    }
}
