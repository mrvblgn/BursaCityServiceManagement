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
  Stack,
  CircularProgress,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import RateReviewIcon from '@mui/icons-material/RateReview';
import AssignmentIndIcon from '@mui/icons-material/AssignmentInd';
import BlockIcon from '@mui/icons-material/Block';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import ReplayIcon from '@mui/icons-material/Replay';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import CategoryIcon from '@mui/icons-material/Category';
import BusinessIcon from '@mui/icons-material/Business';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { managerApi } from '../../api/managerApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip, statusConfig } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { AssignModal } from './components/AssignModal';
import { NoteActionModal } from './components/NoteActionModal';
import { formatDate, formatCoordinates } from '../../utils/formatters';
import { getErrorMessage } from '../../utils/errorUtils';

export const ManagerDetailPage: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [assignOpen, setAssignOpen] = useState(false);
  const [rejectOpen, setRejectOpen] = useState(false);
  const [closeOpen, setCloseOpen] = useState(false);
  const [reopenOpen, setReopenOpen] = useState(false);

  const [actionError, setActionError] = useState<string | null>(null);

  // Fetch request detail
  const { data: request, isLoading, error, refetch } = useQuery({
    queryKey: ['manager', 'requests', id],
    queryFn: () => managerApi.getRequestById(id!),
    enabled: !!id,
  });

  const invalidateQueries = () => {
    queryClient.invalidateQueries({ queryKey: ['manager', 'requests'] });
    queryClient.invalidateQueries({ queryKey: ['manager', 'requests', id] });
  };

  // Start Review Mutation
  const reviewMutation = useMutation({
    mutationFn: () => managerApi.startReview(id!),
    onSuccess: () => {
      invalidateQueries();
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  // Reject Mutation
  const rejectMutation = useMutation({
    mutationFn: (note?: string) => managerApi.rejectRequest(id!, note),
    onSuccess: () => {
      invalidateQueries();
      setRejectOpen(false);
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  // Close Mutation
  const closeMutation = useMutation({
    mutationFn: (note?: string) => managerApi.closeRequest(id!, note),
    onSuccess: () => {
      invalidateQueries();
      setCloseOpen(false);
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  // Reopen Mutation
  const reopenMutation = useMutation({
    mutationFn: (note?: string) => managerApi.reopenRequest(id!, note),
    onSuccess: () => {
      invalidateQueries();
      setReopenOpen(false);
      setActionError(null);
    },
    onError: (err) => setActionError(getErrorMessage(err)),
  });

  if (isLoading) {
    return (
      <Box>
        <PageHeader
          title="Talep Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/manager/requests')}>
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
          title="Talep Detayı"
          action={
            <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/manager/requests')}>
              Geri Dön
            </Button>
          }
        />
        <ErrorAlert error={error || new Error('Talep bulunamadı.')} onRetry={() => refetch()} />
      </Box>
    );
  }

  return (
    <Box>
      <PageHeader
        title={request.title}
        subtitle={`Başvuru ID: ${request.id}`}
        action={
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/manager/requests')}>
            Listeye Dön
          </Button>
        }
      />

      {actionError && (
        <Alert severity="error" sx={{ mb: 3 }} onClose={() => setActionError(null)}>
          {actionError}
        </Alert>
      )}

      {/* Workflow Action Bar */}
      <Card sx={{ mb: 3, borderRadius: 2, border: '1px solid #E2E8F0', backgroundColor: '#F8FAFC' }}>
        <CardContent sx={{ p: 2, display: 'flex', alignItems: 'center', justifyContent: 'space-between', flexWrap: 'wrap', gap: 2 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
            <Typography variant="body2" sx={{ fontWeight: 600, color: '#334155' }}>
              Mevcut Durum:
            </Typography>
            <StatusChip status={request.status} size="medium" />
            <PriorityChip priority={request.priority} size="medium" />
          </Box>

          <Stack direction="row" spacing={1.5}>
            {request.status === 'New' && (
              <>
                <Button
                  variant="contained"
                  color="primary"
                  startIcon={reviewMutation.isPending ? <CircularProgress size={18} color="inherit" /> : <RateReviewIcon />}
                  disabled={reviewMutation.isPending}
                  onClick={() => reviewMutation.mutate()}
                >
                  İncelemeye Al
                </Button>
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<BlockIcon />}
                  onClick={() => setRejectOpen(true)}
                >
                  Reddet
                </Button>
              </>
            )}

            {request.status === 'Reviewing' && (
              <>
                <Button
                  variant="contained"
                  color="primary"
                  startIcon={<AssignmentIndIcon />}
                  onClick={() => setAssignOpen(true)}
                >
                  Görevi Ata
                </Button>
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<BlockIcon />}
                  onClick={() => setRejectOpen(true)}
                >
                  Reddet
                </Button>
              </>
            )}

            {request.status === 'Resolved' && (
              <>
                <Button
                  variant="contained"
                  color="success"
                  startIcon={<CheckCircleIcon />}
                  onClick={() => setCloseOpen(true)}
                >
                  Başvuruyu Kapat (Onayla)
                </Button>
                <Button
                  variant="outlined"
                  color="warning"
                  startIcon={<ReplayIcon />}
                  onClick={() => setReopenOpen(true)}
                >
                  Yeniden Aç
                </Button>
              </>
            )}

            {['Assigned', 'InProgress', 'Closed', 'Rejected', 'Cancelled'].includes(request.status) && (
              <Typography variant="caption" sx={{ color: 'text.secondary', alignSelf: 'center', fontWeight: 500 }}>
                {request.status === 'Closed' && 'Bu başvuru başarıyla kapatılmıştır.'}
                {request.status === 'Rejected' && 'Bu başvuru reddedilmiştir.'}
                {request.status === 'Assigned' && 'Birim ve personel ataması yapılmıştır; saha çalışması beklenmektedir.'}
                {request.status === 'InProgress' && 'Saha personeli çalışma yürütmektedir.'}
              </Typography>
            )}
          </Stack>
        </CardContent>
      </Card>

      <Grid container spacing={3}>
        {/* Main Details */}
        <Grid item xs={12} md={8}>
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', mb: 3 }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2 }}>
                Başvuru ve Hizmet Bilgileri
              </Typography>
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
                        Başvuru Tarihi
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

          {/* Location & Assignment Details */}
          <Grid container spacing={3}>
            <Grid item xs={12} sm={6}>
              <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', height: '100%' }}>
                <CardContent sx={{ p: 3 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 700, color: '#0F172A', mb: 1.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <LocationOnIcon color="primary" /> Konum ve Adres
                  </Typography>
                  <Typography variant="body2" sx={{ fontWeight: 500, color: '#1E293B', mb: 1 }}>
                    {request.location?.addressText || 'Adres tarifi belirtilmemiş.'}
                  </Typography>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                    Koordinatlar: {formatCoordinates(request.location?.latitude, request.location?.longitude)}
                  </Typography>
                </CardContent>
              </Card>
            </Grid>

            <Grid item xs={12} sm={6}>
              <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', height: '100%' }}>
                <CardContent sx={{ p: 3 }}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 700, color: '#0F172A', mb: 1.5, display: 'flex', alignItems: 'center', gap: 1 }}>
                    <BusinessIcon color="primary" /> Görevlendirme
                  </Typography>
                  <Box sx={{ mb: 1 }}>
                    <Typography variant="caption" color="text.secondary">
                      Atanan Birim ID
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {request.assignedDepartmentId || 'Henüz atanmadı'}
                    </Typography>
                  </Box>
                  <Box>
                    <Typography variant="caption" color="text.secondary">
                      Atanan Personel ID
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {request.assignedEmployeeId || 'Henüz atanmadı'}
                    </Typography>
                  </Box>
                </CardContent>
              </Card>
            </Grid>
          </Grid>
        </Grid>

        {/* Status History Timeline */}
        <Grid item xs={12} md={4}>
          <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', height: '100%' }}>
            <CardContent sx={{ p: 3 }}>
              <Typography variant="h6" sx={{ fontWeight: 700, color: '#0F172A', mb: 2 }}>
                Süreç ve Tarihçe
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
                        <Typography variant="caption" color="text.secondary" sx={{ display: 'block' }}>
                          Kullanıcı: {history.changedByUserId.substring(0, 8)}...
                        </Typography>
                      </StepContent>
                    </Step>
                  ))}
                </Stepper>
              )}
            </CardContent>
          </Card>
        </Grid>
      </Grid>

      {/* Modals */}
      <AssignModal
        open={assignOpen}
        requestId={request.id}
        onClose={() => setAssignOpen(false)}
        onSuccess={() => invalidateQueries()}
      />

      <NoteActionModal
        open={rejectOpen}
        title="Başvuruyu Reddet"
        description="Bu başvuruyu belediyemiz sorumluluk alanı dışında olduğu veya mükerrer olduğu gerekçesiyle reddediyorsunuz."
        noteLabel="Ret Gerekçesi"
        confirmText="Reddet"
        confirmColor="error"
        isLoading={rejectMutation.isPending}
        error={actionError}
        onConfirm={(note) => rejectMutation.mutate(note)}
        onClose={() => setRejectOpen(false)}
      />

      <NoteActionModal
        open={closeOpen}
        title="Başvuruyu Kapat ve Çözümü Onayla"
        description="Saha personeli tarafından çözümlenen bu başvurunun tamamlandığını onaylayarak kaydı kapatıyorsunuz."
        noteLabel="Kapanış / Kontrol Notu"
        confirmText="Kapat ve Onayla"
        confirmColor="success"
        isLoading={closeMutation.isPending}
        error={actionError}
        onConfirm={(note) => closeMutation.mutate(note)}
        onClose={() => setCloseOpen(false)}
      />

      <NoteActionModal
        open={reopenOpen}
        title="Başvuruyu Yeniden Aç"
        description="Çözülen başvurudaki çalışmanın yetersiz olduğunu düşünüyorsanız, başvuruyu tekrar işlemde durumuna alabilirsiniz."
        noteLabel="Yeniden Açma Gerekçesi"
        confirmText="Yeniden Aç"
        confirmColor="warning"
        isLoading={reopenMutation.isPending}
        error={actionError}
        onConfirm={(note) => reopenMutation.mutate(note)}
        onClose={() => setReopenOpen(false)}
      />
    </Box>
  );
};
