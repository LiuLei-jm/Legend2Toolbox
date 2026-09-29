import type { CancelablePromise } from '@/api/generated/core/CancelablePromise'
import { OpenAPI } from '@/api/generated/core/OpenAPI'
import { request } from '@/api/generated/core/request'

export interface MembershipStatus {
  isActive: boolean
  startTime?: string
  expireTime?: string
}

export const AdminMembershipService = {
  adjustDays(userId: string, days: number): CancelablePromise<MembershipStatus> {
    return request(OpenAPI, {
      method: 'POST',
      url: '/api/admin/users/{userId}/membership-time',
      path: { userId },
      body: { days },
      mediaType: 'application/json',
    })
  },
}
