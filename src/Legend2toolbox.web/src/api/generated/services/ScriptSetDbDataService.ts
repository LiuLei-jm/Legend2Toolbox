/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { CreateScriptSetDbDataRequest } from '../models/CreateScriptSetDbDataRequest';
import type { UpdateScriptSetDbDataRequest } from '../models/UpdateScriptSetDbDataRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class ScriptSetDbDataService {
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiScriptsDbDatas(
        requestBody: CreateScriptSetDbDataRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/scripts/db-datas',
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
    public static putApiScriptsDbDatas(
        id: string,
        requestBody: UpdateScriptSetDbDataRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/scripts/db-datas/{id}',
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
    public static deleteApiScriptsDbDatas(
        id: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/scripts/db-datas/{id}',
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
    public static getApiScriptsDbDatasBySet(
        scriptSetId: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/scripts/db-datas/by-set/{scriptSetId}',
            path: {
                'scriptSetId': scriptSetId,
            },
        });
    }
}
