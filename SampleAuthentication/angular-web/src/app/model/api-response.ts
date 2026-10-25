import { HttpStatusCode } from '@angular/common/http';

export interface ApiResponse {
    status: boolean;
    message: string;
}
export interface ApiDataResponse<T> extends ApiResponse {
    data: T;
}

export class ErrorDetailResponse{    
    statusCode: HttpStatusCode = HttpStatusCode.BadRequest;
    message: string = "";
}