import { GenericResponse } from './generic-response';
import { IsoDateTime } from './iso-date';

export interface LoginData {
  accessToken: string;
  expiresAt: IsoDateTime;
}

export interface LoginDataResponse extends GenericResponse<LoginData> {}
