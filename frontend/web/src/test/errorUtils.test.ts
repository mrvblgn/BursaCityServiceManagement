import { describe, it, expect } from 'vitest';
import { getErrorMessage } from '../utils/errorUtils';
import { AxiosError, AxiosHeaders, AxiosResponse } from 'axios';

describe('getErrorMessage', () => {
  it('should extract detail from ProblemDetails', () => {
    const error = new AxiosError(
      'Request failed',
      'ERR_BAD_REQUEST',
      undefined,
      undefined,
      {
        data: {
          title: 'Conflict',
          status: 409,
          detail: 'The assigned employee does not belong to the selected department.',
        },
        status: 409,
        statusText: 'Conflict',
        headers: {},
        config: { headers: new AxiosHeaders() },
      } as AxiosResponse
    );

    const message = getErrorMessage(error);
    expect(message).toBe('The assigned employee does not belong to the selected department.');
  });

  it('should extract title when detail is missing', () => {
    const error = new AxiosError(
      'Request failed',
      'ERR_BAD_REQUEST',
      undefined,
      undefined,
      {
        data: {
          title: 'Geçersiz İstek',
          status: 400,
        },
        status: 400,
        statusText: 'Bad Request',
        headers: {},
        config: { headers: new AxiosHeaders() },
      } as AxiosResponse
    );

    const message = getErrorMessage(error);
    expect(message).toBe('Geçersiz İstek');
  });

  it('should return safe Turkish fallback for standard Error instance', () => {
    const error = new Error('Ağ bağlantısı kesildi.');
    expect(getErrorMessage(error)).toBe('Ağ bağlantısı kesildi.');
  });

  it('should return generic Turkish message for unknown errors', () => {
    expect(getErrorMessage('Something random')).toBe('Beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.');
  });
});
