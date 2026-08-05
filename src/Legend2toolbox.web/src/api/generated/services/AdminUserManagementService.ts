/* generated using openapi-typescript-codegen -- do not edit */
/* istanbul ignore file */
/* tslint:disable */
/* eslint-disable */
import type { AssignRoleRequest } from '../models/AssignRoleRequest';
import type { GetUserByNameRequest } from '../models/GetUserByNameRequest';
import type { ToggleLockRequest } from '../models/ToggleLockRequest';
import type { UpdateUserRequest } from '../models/UpdateUserRequest';
import type { CancelablePromise } from '../core/CancelablePromise';
import { OpenAPI } from '../core/OpenAPI';
import { request as __request } from '../core/request';
export class AdminUserManagementService {
    /**
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAdminUsersClients(): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/admin/users/clients',
        });
    }
    /**
     * @param pageNumber
     * @param pageSize
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAdminUsers(
        pageNumber?: number,
        pageSize?: number,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/admin/users',
            query: {
                'pageNumber': pageNumber,
                'pageSize': pageSize,
            },
        });
    }
    /**
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static getApiAdminUsersName(
        requestBody: GetUserByNameRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'GET',
            url: '/api/admin/users/name',
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param userId
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static putApiAdminUsersLock(
        userId: string,
        requestBody: ToggleLockRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/admin/users/{userId}/lock',
            path: {
                'userId': userId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param userId
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static postApiAdminUsersRoles(
        userId: string,
        requestBody: AssignRoleRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'POST',
            url: '/api/admin/users/{userId}/roles',
            path: {
                'userId': userId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param userId
     * @param requestBody
     * @returns any OK
     * @throws ApiError
     */
    public static putApiAdminUsersUpdate(
        userId: string,
        requestBody: UpdateUserRequest,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/admin/users/{userId}/update',
            path: {
                'userId': userId,
            },
            body: requestBody,
            mediaType: 'application/json',
        });
    }
    /**
     * @param userId
     * @returns any OK
     * @throws ApiError
     */
    public static putApiAdminUsersRemove(
        userId: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'PUT',
            url: '/api/admin/users/{userId}/remove',
            path: {
                'userId': userId,
            },
        });
    }
    /**
     * @param userId
     * @returns any OK
     * @throws ApiError
     */
    public static deleteApiAdminUsers(
        userId: string,
    ): CancelablePromise<any> {
        return __request(OpenAPI, {
            method: 'DELETE',
            url: '/api/admin/users/{userId}',
            path: {
                'userId': userId,
            },
        });
    }
}
