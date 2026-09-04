export interface UserInfo {
  username: string
  nickname: string
  email: string
  phoneNumber: string
  lastLoginAt: string
  roles: string[]
  userId: string
}
export interface AdminUserInfo {
    id: string
    username: string
    email: string
    roles: string[]
    isLockedOut: boolean
    lockoutEnd?: string
}