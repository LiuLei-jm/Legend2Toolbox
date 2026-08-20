export interface CardItem {
    id: string
    owner: string
    durationInDays: number
    startTime: string
    endTime: string
    faceValue: number
    amount: number
    cdk: string
    isExpired: boolean
}