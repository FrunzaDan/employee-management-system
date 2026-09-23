import { GenericResponse } from './generic-response';

export interface LoginData {
  accessToken: string;
  validUntil: string; // UTC, ISO 8601
}

export interface LoginDataResponse extends GenericResponse<LoginData> {}
