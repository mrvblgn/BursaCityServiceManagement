import React from 'react';
import { describe, it, expect, vi } from 'vitest';
import { render, screen } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { AssignModal } from '../pages/manager/components/AssignModal';
import * as referenceApiModule from '../api/referenceApi';

describe('AssignModal', () => {
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: { retry: false },
    },
  });

  it('should render assignment fields when opened', async () => {
    vi.spyOn(referenceApiModule.referenceApi, 'getDepartments').mockResolvedValue([
      { id: '10000000-0000-0000-0000-000000000001', name: 'Fen İşleri' },
    ]);

    render(
      <QueryClientProvider client={queryClient}>
        <AssignModal
          open={true}
          requestId="req-123"
          onClose={vi.fn()}
          onSuccess={vi.fn()}
        />
      </QueryClientProvider>
    );

    expect(screen.getByText('Görevi Birim ve Personele Ata')).toBeInTheDocument();
    expect(screen.getByLabelText(/Görevlendirilecek Birim/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Saha Personeli/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/Öncelik Derecesi/i)).toBeInTheDocument();
  });
});
