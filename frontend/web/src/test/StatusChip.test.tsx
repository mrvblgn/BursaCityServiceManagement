import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { StatusChip } from '../components/common/StatusChip';
import { RequestStatus } from '../types/serviceRequest.types';

describe('StatusChip', () => {
  const statuses: { status: RequestStatus; expectedLabel: string }[] = [
    { status: 'New', expectedLabel: 'Yeni' },
    { status: 'Reviewing', expectedLabel: 'İnceleniyor' },
    { status: 'Assigned', expectedLabel: 'Atandı' },
    { status: 'InProgress', expectedLabel: 'İşlemde' },
    { status: 'Resolved', expectedLabel: 'Çözüldü' },
    { status: 'Closed', expectedLabel: 'Kapatıldı' },
    { status: 'Rejected', expectedLabel: 'Reddedildi' },
    { status: 'Cancelled', expectedLabel: 'İptal Edildi' },
  ];

  statuses.forEach(({ status, expectedLabel }) => {
    it(`should render correct Turkish label for ${status}`, () => {
      render(<StatusChip status={status} />);
      expect(screen.getByText(expectedLabel)).toBeInTheDocument();
    });
  });
});
