export interface GenericResult<T> {
  statusCode: number
  data?: T
  url?: string
  errors?: string[]
}

export interface ApiErrorDetails {
  status: number
  messages: string[]
}
