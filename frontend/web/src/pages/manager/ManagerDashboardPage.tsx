import React from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Grid,
  Paper,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
} from '@mui/material';
import RateReviewIcon from '@mui/icons-material/RateReview';
import AssignmentIcon from '@mui/icons-material/Assignment';
import CheckCircleIcon from '@mui/icons-material/CheckCircle';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../../auth/useAuth';
import { managerApi } from '../../api/managerApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { EmptyState } from '../../components/common/EmptyState';
import { formatDate } from '../../utils/formatters';

export const ManagerDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Load recent municipal requests
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['manager', 'requests', { pageNumber: 1, pageSize: 6 }],
    queryFn: () => managerApi.getMunicipalRequests({ pageNumber: 1, pageSize: 6 }),
  });

  return (
    <Box>
      <PageHeader
        title={`Yönetim Paneli — ${user?.firstName} ${user?.lastName}`}
        subtitle="Bursa Büyükşehir Belediyesi Birim Yönetim Paneli"
        action={
          <Button
            variant="contained"
            color="primary"
            startIcon={<RateReviewIcon />}
            onClick={() => navigate('/manager/requests?status=New')}
          >
            İnceleme Bekleyenler
          </Button>
        }
      />

      {/* Metrics Cards */}
      <Grid container spacing={2.5} sx={{ mb: 4 }}>
        <Grid item xs={12} sm={4}>
          <Paper
            elevation={0}
            sx={{
              p: 2.5,
              borderRadius: 3,
              border: '1px solid #E2E8F0',
              backgroundColor: '#FFFFFF',
              display: 'flex',
              alignItems: 'center',
              gap: 2,
            }}
          >
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                backgroundColor: '#EFF6FF',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'primary.main',
              }}
            >
              <AssignmentIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Toplam Başvuru Sayısı
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#0F172A' }}>
                {data ? data.totalCount : '-'}
              </Typography>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={4}>
          <Paper
            elevation={0}
            sx={{
              p: 2.5,
              borderRadius: 3,
              border: '1px solid #E2E8F0',
              backgroundColor: '#FFFFFF',
              display: 'flex',
              alignItems: 'center',
              gap: 2,
            }}
          >
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                backgroundColor: '#FFFBEB',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'warning.main',
              }}
            >
              <RateReviewIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Hızlı Filtre
              </Typography>
              <Typography
                variant="body2"
                sx={{ fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
                onClick={() => navigate('/manager/requests?status=Reviewing')}
              >
                İncelenen Talepler →
              </Typography>
            </Box>
          </Paper>
        </Grid>

        <Grid item xs={12} sm={4}>
          <Paper
            elevation={0}
            sx={{
              p: 2.5,
              borderRadius: 3,
              border: '1px solid #E2E8F0',
              backgroundColor: '#FFFFFF',
              display: 'flex',
              alignItems: 'center',
              gap: 2,
            }}
          >
            <Box
              sx={{
                width: 48,
                height: 48,
                borderRadius: 2,
                backgroundColor: '#ECFDF5',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'success.main',
              }}
            >
              <CheckCircleIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Kapanış Onayı Bekleyenler
              </Typography>
              <Typography
                variant="body2"
                sx={{ fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
                onClick={() => navigate('/manager/requests?status=Resolved')}
              >
                Çözülen Talepleri İncele →
              </Typography>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Recent Municipal Requests */}
      <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600, color: '#1E293B' }}>
              Son Gelen Hizmet Talepleri
            </Typography>
            {data && data.items.length > 0 && (
              <Button size="small" onClick={() => navigate('/manager/requests')}>
                Tümünü Yönet ({data.totalCount})
              </Button>
            )}
          </Box>

          {isLoading && <LoadingSkeleton rows={5} variant="table" />}

          {error && <ErrorAlert error={error} onRetry={() => refetch()} />}

          {!isLoading && !error && data && data.items.length === 0 && (
            <EmptyState
              title="Talep Bulunamadı"
              description="Sistemde henüz kayıtlı bir hizmet başvurusu bulunmamaktadır."
            />
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <TableContainer component={Paper} elevation={0}>
              <Table sx={{ minWidth: 750 }} aria-label="yönetici son başvurular tablosu">
                <TableHead sx={{ backgroundColor: '#F8FAFC' }}>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Başlık</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Vatandaş</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Kategori</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Durum</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Öncelik</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Birim / Personel</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Tarih</TableCell>
                    <TableCell align="right" sx={{ fontWeight: 600, color: '#475569' }}>
                      İşlem
                    </TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.items.map((row) => (
                    <TableRow
                      key={row.id}
                      hover
                      sx={{ cursor: 'pointer' }}
                      onClick={() => navigate(`/manager/requests/${row.id}`)}
                    >
                      <TableCell component="th" scope="row">
                        <Typography variant="body2" sx={{ fontWeight: 600, color: '#0F172A' }}>
                          {row.title}
                        </Typography>
                      </TableCell>
                      <TableCell>{row.citizenName}</TableCell>
                      <TableCell>{row.categoryName}</TableCell>
                      <TableCell>
                        <StatusChip status={row.status} />
                      </TableCell>
                      <TableCell>
                        <PriorityChip priority={row.priority} />
                      </TableCell>
                      <TableCell>
                        <Typography variant="caption" sx={{ display: 'block', fontWeight: 600, color: '#334155' }}>
                          {row.assignedDepartmentName || 'Atanmadı'}
                        </Typography>
                        {row.assignedEmployeeName && (
                          <Typography variant="caption" color="text.secondary">
                            {row.assignedEmployeeName}
                          </Typography>
                        )}
                      </TableCell>
                      <TableCell sx={{ color: 'text.secondary', fontSize: '0.85rem' }}>
                        {formatDate(row.createdAt)}
                      </TableCell>
                      <TableCell align="right">
                        <Button
                          size="small"
                          variant="outlined"
                          startIcon={<VisibilityIcon />}
                          onClick={(e) => {
                            e.stopPropagation();
                            navigate(`/manager/requests/${row.id}`);
                          }}
                        >
                          İncele
                        </Button>
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};
