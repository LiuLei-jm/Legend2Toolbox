/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { UpdateMaterialFileRequest } from '../models/UpdateMaterialFileRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class MaterialFileService {
    /**
     * @param formData
     * @returns any OK
     * @throws ApiError
     */
    public static postApiScriptsMaterial(
        formData?: {
            scriptSetId: string;
            file: Blob;
            targetPath: string;
            password?: string;
            fileSize?: number;
        },
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/scripts/material',
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }
    /**
     * @param id
     * @param formData
     * @returns any OK
     * @throws ApiError
     */
    public static putApiScriptsMaterial(
        id: string,
        formData?: {
            req: UpdateMaterialFileRequest;
        },
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/scripts/material/{id}',
            path: {
                'id': id,
            },
            formData: formData,
            mediaType: 'multipart/form-data',
        });
    }
    /**
     * @param id
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiScriptsMaterial(
        id: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/scripts/material/{id}',
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
    public static getApiScriptsMaterialBySet(
        scriptSetId: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/scripts/material/by-set/{scriptSetId}',
            path: {
                'scriptSetId': scriptSetId,
            },
        });
    }
}
