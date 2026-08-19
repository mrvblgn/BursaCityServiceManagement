import React, { useState } from 'react';
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
  Alert,
  CircularProgress,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import CategoryIcon from '@mui/icons-material/Category';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { employeeApi } from '../../api/employeeApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip, statusConfig } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { ResolveModal } from './components/ResolveModal';
import { formatDate, formatCoordinates } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/errorUtils';

export const EmployeeDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [resolveOpen, setResolveOpen] = useState(false);
  const [actionError, setActionError] = useState<string | null>(null);

  // Fetch assigned request detail
  const { data: request, isLoading, error, refetch } = useQuery({
    queryKey: ['employee', 'requests', id],
    queryFn: () => employeeApi.getAssignedRequestById(id!),
    enabled: !!id,
  });

  const invalidateQueries = () => {
    queryClient.invalidateQueries({ queryKey: ['employee', 'requests'] });
    queryClient.invalidateQueries({ queryKey: ['employee', 'requests', id] });
  };

  // Start Work Mutation
  const startWorkMutation = useMutation({
    mutationFn: () => employeeApi.startWork(id!),
    onSuccess: () => {
      invalidateQueries();
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  // Resolve Mutation
  const resolveMutation = useMutation({
    mutationFn: (note?: string) => employeeApi.resolveRequest(id!, note),
    onSuccess: () => {
      invalidateQueries();
      setResolveOpen(false);
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  if (isLoading) {
    return (
      <Box>
        <PageHeader
          title="Görev Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/employee/requests')}>
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
          title="Görev Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/employee/requests')}>
              Geri Dön
            </Button>
          }
        />
        <ErrorAlert error={error || new Error('Görev bulunamadı veya yetkiniz yok.')} onRetry={() => refetch()} />
      </Box>
    );
  }

  return (
    <Box>
      <PageHeader
        title={request.title}
        subtitle={`Görev ID: ${request.id}`}
        action={
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/employee/requests')}>
            Listeye Dön
          </Button>
        }
      />

      {actionError && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setActionError(null)}>
          {actionError}
        </Alert>
      )}

      {/* Action Bar */}
      <Card sx={{ mb: 3, borderRadius: 2, border: '1px solid #E2E8F0', backgroundColor: '#F8FAFC' }}>
        <CardContent sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Typography variant="body2" sx={{ fontWeight: 600, color: '#334155' }}>
              Görev Durumu:
            </Typography>
            <StatusChip status={request.status} size="medium" />
            <PriorityChip priority={request.priority} size="medium" />
          </Box>

          <Box>
            {request.status === 'Assigned' && (
              <Button
                variant="contained"
                color="primary"
                size="large"
                startIcon={startWorkMutation.isPending ? <CircularProgress size={18} color="inherit" /> : <PlayArrowIcon />}
                disabled={startWorkMutation.isPending}
                onClick={() => startWorkMutation.mutate()}
                sx={{ px: 3, fontWeight: 600 }}
              >
                {startWorkMutation.isPending ? 'Başlatılıyor...' : 'Çalışmayı Başlat (İşlemde Yap)'}
              </Button>
            )}

            {request.status === 'InProgress' && (
              <Button
                variant="contained"
                color="success"
                size="large"
                startIcon={<CheckCircleOutlineIcon />}
                onClick={() => setResolveOpen(true)}
                sx={{ px: 3, fontWeight: 600 }}
              >
                Çözümlendi Olarak İşaretle
              </Button>
            )}

            {request.status === 'Resolved' && (
              <Typography variant="body2" sx={{ color: 'success.dark', fontWeight: 600 }}>
                ✓ Çözüm kaydedildi. Birim yöneticisinin kapanış onayı bekleniyor.
              </Typography>
            )}

            {request.status === 'Closed' && (
              <Typography variant="body2" sx={{ color: 'text.secondary', fontWeight: 500 }}>
                Bu görev kapatılmıştır.
              </Typography>
            )}
          </Box>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Main Details */}
        <Grid item xs={12} md={8}>
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', mb: 3 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2 }}>
                Görev ve Talep Açıklaması
              </Typography>
              <Divider sx={{ my: 2 }} />

              <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                Vatandaş Açıklaması:
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
                        Kayıt Tarihi
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

          {/* Location */}
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2, display: 'flex', alignItems: 'center', gap: 1 }}>
                <LocationOnIcon color="primary" /> Müdahale Konumu / Adresi
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

      {/* Resolve Modal */}
      <ResolveModal
        open={resolveOpen}
        isLoading={resolveMutation.isPending}
        error={actionError}
        onConfirm={(note) => resolveMutation.mutate(note)}
        onClose={() => setResolveOpen(false)}
      />
    </Box>
  );
};
