import { OpenAPI } from '@/api/generated/core/OpenAPI'
import { request } from '@/api/generated/core/request'

export interface AuditDetail {
  id: string
  sequence: number
  entityType: string
  entityId: string
  changeType: string
  oldValues: string
  newValues: string
}

export interface AuditLog {
  id: string
  traceId: string
  occurredAtUnixMs: number
  actorUserId?: string
  actorUserName?: string
  subjectUserId?: string
  actorType: string
  module: string
  action: string
  targetId?: string
  targetName?: string
  clientIp?: string
  peerIp?: string
  outcome: string
  dataStatus: string
  errorCode?: string
  captureIncomplete: boolean
  details: AuditDetail[]
}

export interface AuditFilter {
  pageNumber: number
  pageSize: number
  from?: string
  to?: string
  module?: string
  action?: string
  outcome?: string
  clientIp?: string
  userId?: string
  account?: string
  targetId?: string
}

export interface AuditPage {
  items: AuditLog[]
  totalCount: number
}

export const AuditService = {
  list(all: boolean, filter: AuditFilter) {
    return request<AuditPage>(OpenAPI, {
      method: 'GET',
      url: all ? '/api/admin/audit/' : '/api/audit/',
      query: filter,
    })
  },
  detail(all: boolean, id: string) {
    return request<AuditLog>(OpenAPI, {
      method: 'GET',
      url: all ? '/api/admin/audit/{id}' : '/api/audit/{id}',
      path: { id },
    })
  },
}
