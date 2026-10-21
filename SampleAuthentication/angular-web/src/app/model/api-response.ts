export interface ApiResponse {
    status: boolean;
    message: string;
}
export interface ApiDataResponse<T> extends ApiResponse {
    data: T;
}