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
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import AssignmentIcon from '@mui/icons-material/Assignment';
import CheckCircleOutlineIcon from '@mui/icons-material/CheckCircleOutline';
import HourglassEmptyIcon from '@mui/icons-material/HourglassEmpty';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../../auth/useAuth';
import { citizenApi } from '../../api/citizenApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { EmptyState } from '../../components/common/EmptyState';
import { formatDate } from '../../utils/formatters';

export const CitizenDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['citizen', 'requests', { pageNumber: 1, pageSize: 5 }],
    queryFn: () => citizenApi.getMyRequests('', 1, 5),
  });

  return (
    <Box>
      <PageHeader
        title={`Hoş Geldiniz, ${user?.firstName} ${user?.lastName}`}
        subtitle="Bursa Büyükşehir Belediyesi Hizmet ve Talep Portalı üzerinden başvurularınızı takip edebilirsiniz."
        action={
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddCircleOutlineIcon />}
            onClick={() => navigate('/citizen/requests/new')}
            sx={{ py: 1, px: 2.5 }}
          >
            Yeni Başvuru Yap
          </Button>
        }
      />

      {/* Quick Stats Overview */}
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
                Toplam Başvurularım
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
              <HourglassEmptyIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Son Başvurular (Bu Sayfa)
              </Typography>
              <Typography variant="h5" sx={{ fontWeight: 700, color: '#0F172A' }}>
                {data ? data.items.length : '-'}
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
              <CheckCircleOutlineIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Hızlı İşlem
              </Typography>
              <Typography
                variant="body2"
                sx={{ fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
                onClick={() => navigate('/citizen/requests')}
              >
                Tüm Başvuruları İncele →
              </Typography>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Recent Requests Section */}
      <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0', backgroundColor: '#FFFFFF' }}>
        <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600, color: '#1E293B' }}>
              Son Başvurularım
            </Typography>
            {data && data.items.length > 0 && (
              <Button size="small" onClick={() => navigate('/citizen/requests')}>
                Tümünü Gör ({data.totalCount})
              </Button>
            )}
          </Box>

          {isLoading && <LoadingSkeleton rows={4} variant="table" />}

          {error && <ErrorAlert error={error} onRetry={() => refetch()} />}

          {!isLoading && !error && data && data.items.length === 0 && (
            <EmptyState
              title="Henüz bir başvurunuz bulunmuyor"
              description="Belediyemize bildirmek istediğiniz arıza veya talepleriniz için yeni bir başvuru oluşturabilirsiniz."
              actionText="İlk Başvuruyu Yap"
              onAction={() => navigate('/citizen/requests/new')}
            />
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid #F1F5F9' }}>
              <Table sx={{ minWidth: 650 }} aria-label="son başvurular tablosu">
                <TableHead sx={{ backgroundColor: '#F8FAFC' }}>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Başlık</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Kategori</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Durum</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Öncelik</TableCell>
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
                      sx={{ '&:last-child td, &:last-child th': { border: 0 }, cursor: 'pointer' }}
                      onClick={() => navigate(`/citizen/requests/${row.id}`)}
                    >
                      <TableCell component="th" scope="row" sx={{ fontWeight: 600, color: '#0F172A' }}>
                        {row.title}
                      </TableCell>
                      <TableCell>{row.categoryName}</TableCell>
                      <TableCell>
                        <StatusChip status={row.status} />
                      </TableCell>
                      <TableCell>
                        <PriorityChip priority={row.priority} />
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
                            navigate(`/citizen/requests/${row.id}`);
                          }}
                        >
                          Detay
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
