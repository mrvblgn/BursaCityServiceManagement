import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { PriorityChip } from '../components/common/PriorityChip';
import { Priority } from '../types/serviceRequest.types';

describe('PriorityChip', () => {
  const priorities: { priority: Priority; expectedLabel: string }[] = [
    { priority: 'Low', expectedLabel: 'Düşük' },
    { priority: 'Medium', expectedLabel: 'Orta' },
    { priority: 'High', expectedLabel: 'Yüksek' },
    { priority: 'Critical', expectedLabel: 'Kritik' },
  ];

  priorities.forEach(({ priority, expectedLabel }) => {
    it(`should render correct Turkish label for ${priority}`, () => {
      render(<PriorityChip priority={priority} />);
      expect(screen.getByText(expectedLabel)).toBeInTheDocument();
    });
  });

  it('should render Belirtilmemiş when priority is null or undefined', () => {
    render(<PriorityChip priority={null} />);
    expect(screen.getByText('Belirtilmemiş')).toBeInTheDocument();
  });
});
