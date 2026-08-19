import React from 'react';
import { Box, Skeleton, Card, CardContent } from '@mui/material';

interface LoadingSkeletonProps {
  rows?: number;
  variant?: 'table' | 'cards' | 'detail';
}

export const LoadingSkeleton: React.FC<LoadingSkeletonProps> = ({ rows = 5, variant = 'table' }) => {
  if (variant === 'cards') {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
        {Array.from({ length: rows }).map((_, index) => (
          <Card key={index} variant="outlined">
            <CardContent>
              <Skeleton variant="text" width="60%" height={32} />
              <Skeleton variant="text" width="40%" height={20} />
              <Box sx={{ display: 'flex', gap: 1, mt: 2 }}>
                <Skeleton variant="rounded" width={80} height={24} />
                <Skeleton variant="rounded" width={80} height={24} />
              </Box>
            </CardContent>
          </Card>
        ))}
      </Box>
    );
  }

  if (variant === 'detail') {
    return (
      <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Skeleton variant="text" width="50%" height={40} />
        <Skeleton variant="rectangular" width="100%" height={120} sx={{ borderRadius: 2 }} />
        <Skeleton variant="rectangular" width="100%" height={200} sx={{ borderRadius: 2 }} />
      </Box>
    );
  }

  return (
    <Box sx={{ width: '100%', py: 1 }}>
      {Array.from({ length: rows }).map((_, index) => (
        <Skeleton key={index} variant="rectangular" height={52} sx={{ my: 1, borderRadius: 1 }} />
      ))}
    </Box>
  );
};
