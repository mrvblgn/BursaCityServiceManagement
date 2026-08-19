import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Divider,
  Paper,
  Stepper,
  Step,
  StepLabel,
  StepContent,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import CategoryIcon from '@mui/icons-material/Category';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { citizenApi } from '../../api/citizenApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip, statusConfig } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { formatDate, formatCoordinates } from '../../utils/formatters';

export const CitizenDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data: request, isLoading, error, refetch } = useQuery({
    queryKey: ['citizen', 'requests', id],
    queryFn: () => citizenApi.getRequestById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return (
      <Box>
        <PageHeader
          title="Başvuru Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/citizen/requests')}>
              Geri Dön
            </Button>
          }
        />
        <LoadingSkeleton variant="detail" />
      </Box>
    );
  }

  if (error || !request) {
    return (
      <Box>
        <PageHeader
          title="Başvuru Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/citizen/requests')}>
              Geri Dön
            </Button>
          }
        />
        <ErrorAlert error={error || new Error('Başvuru bulunamadı.')} onRetry={() => refetch()} />
      </Box>
    );
  }

  return (
    <Box>
      <PageHeader
        title={request.title}
        subtitle={`Başvuru ID: ${request.id}`}
        action={
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/citizen/requests')}>
            Listeye Dön
          </Button>
        }
      />

      <Grid container spacing={3}>
        {/* Main Details */}
        <Grid item xs={12} md={8}>
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', mb: 3 }}>
            <CardContent sx={{ p: 3 }}>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A' }}>
                  Başvuru Bilgileri
                </Typography>
                <Box sx={{ display: 'flex', gap: 1 }}>
                  <StatusChip status={request.status} size="medium" />
                  <PriorityChip priority={request.priority} size="medium" />
                </Box>
              </Box>

              <Divider sx={{ my: 2 }} />

              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Açıklama:
              </Typography>
              <Typography variant="body1" sx={{ color: '#334155', whiteSpace: 'pre-line', mb: 3 }}>
                {request.description || 'Açıklama belirtilmemiş.'}
              </Typography>

              <Grid container spacing={2} sx={{ mt: 1 }}>
                <Grid item xs={12} sm={6}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CategoryIcon color="action" fontSize="small" />
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        Kategori
                      </Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        {request.categoryName}
                      </Typography>
                    </Box>
                  </Box>
                </Grid>

                <Grid item xs={12} sm={6}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <CalendarTodayIcon color="action" fontSize="small" />
                    <Box>
                      <Typography variant="caption" color="text.secondary">
                        Oluşturulma Tarihi
                      </Typography>
                      <Typography variant="body2" sx={{ fontWeight: 600 }}>
                        {formatDate(request.createdAt)}
                      </Typography>
                    </Box>
                  </Box>
                </Grid>
              </Grid>
            </CardContent>
          </Card>

          {/* Location Card */}
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <LocationOnIcon color="primary" /> Konum ve Adres
              </Typography>
              <Divider sx={{ my: 1.5 }} />

              <Typography variant="body1" sx={{ fontWeight: 500, color: '#1E293B', mb: 1 }}>
                {request.location?.addressText || 'Adres tarifi belirtilmemiş.'}
              </Typography>

              <Typography variant="body2" color="text.secondary">
                Koordinatlar: {formatCoordinates(request.location?.latitude, request.location?.longitude)}
              </Typography>
            </CardContent>
          </Card>
        </Grid>

        {/* Status History Timeline */}
        <Grid item xs={12} md={4}>
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', height: '100%' }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2 }}>
                Süreç Takibi
              </Typography>
              <Divider sx={{ my: 1.5 }} />

              {request.statusHistory.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                  Henüz bir durum güncellemesi bulunmuyor.
                </Typography>
              ) : (
                <Stepper orientation="vertical" activeStep={request.statusHistory.length - 1} sx={{ mt: 2 }}>
                  {request.statusHistory.map((history) => (
                    <Step key={history.id} active completed>
                      <StepLabel
                        optional={
                          <Typography variant="caption" color="text.secondary">
                            {formatDate(history.changedAt)}
                          </Typography>
                        }
                      >
                        <Typography variant="body2" sx={{ fontWeight: 600 }}>
                          {statusConfig[history.newStatus]?.label || history.newStatus}
                        </Typography>
                      </StepLabel>
                      <StepContent>
                        {history.note && (
                          <Paper
                            elevation={0}
                            sx={{
                              p: 1.5,
                              backgroundColor: '#F8FAFC',
                              borderRadius: 1.5,
                              border: '1px solid #E2E8F0',
                              mb: 1,
                            }}
                          >
                            <Typography variant="caption" sx={{ color: '#475569', display: 'block', fontStyle: 'italic' }}>
                              "{history.note}"
                            </Typography>
                          </Paper>
                        )}
                      </StepContent>
                    </Step>
                  ))}
                </Stepper>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>
    </Box>
  );
};
