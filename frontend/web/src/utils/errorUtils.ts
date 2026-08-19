import axios from 'axios';
import { ProblemDetails } from '../types/auth.types';

export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ProblemDetails | undefined;
    if (data) {
      if (data.detail && typeof data.detail === 'string') {
        return data.detail;
      }
      if (data.title && typeof data.title === 'string') {
        return data.title;
      }
      if (data.errors && typeof data.errors === 'object') {
        const errorMessages = Object.values(data.errors).flat();
        if (errorMessages.length > 0) {
          return errorMessages.join(' ');
        }
      }
    }

    if (error.response?.status === 401) {
      return 'Oturum süreniz doldu veya giriş bilgileriniz geçersiz.';
    }
    if (error.response?.status === 403) {
      return 'Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır.';
    }
    if (error.response?.status === 404) {
      return 'Talep edilen kayıt bulunamadı.';
    }
    if (error.response?.status === 409) {
      return 'İşlem mevcut durumla çakışmaktadır.';
    }
    if (error.response?.status === 422) {
      return 'Geçersiz durum geçişi veya veri kuralı ihlali.';
    }
    if (error.response?.status === 500) {
      return 'Sunucu tarafında beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.';
    }
  }

  if (error instanceof Error) {
    return error.message;
  }

  return 'Beklenmeyen bir hata oluştu. Lütfen tekrar deneyiniz.';
}
