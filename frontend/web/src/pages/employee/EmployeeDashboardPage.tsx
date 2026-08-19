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
import AssignmentIcon from '@mui/icons-material/Assignment';
import ConstructionIcon from '@mui/icons-material/Construction';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { useAuth } from '../../auth/useAuth';
import { employeeApi } from '../../api/employeeApi';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { EmptyState } from '../../components/common/EmptyState';
import { formatDate } from '../../utils/formatters';

export const EmployeeDashboardPage: React.FC = () => {
  const { user } = useAuth();
  const navigate = useNavigate();

  // Load employee's assigned requests
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['employee', 'requests', { pageNumber: 1, pageSize: 6 }],
    queryFn: () => employeeApi.getMyAssignedRequests('', 1, 6),
  });

  return (
    <Box>
      <PageHeader
        title={`Saha Görev Paneli — ${user?.firstName} ${user?.lastName}`}
        subtitle="Bursa Büyükşehir Belediyesi Saha Personeli Görev Yönetim Portalı"
        action={
          <Button
            variant="contained"
            color="primary"
            startIcon={<ConstructionIcon />}
            onClick={() => navigate('/employee/requests?status=Assigned')}
          >
            Yeni Atanan Görevlerim
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
                Toplam Atanan Görevlerim
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
                backgroundColor: '#F3E8FF',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                color: 'secondary.main',
              }}
            >
              <ConstructionIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Hızlı Filtre
              </Typography>
              <Typography
                variant="body2"
                sx={{ fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
                onClick={() => navigate('/employee/requests?status=InProgress')}
              >
                İşlemdeki Görevlerim →
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
              <AssignmentIcon fontSize="medium" />
            </Box>
            <Box>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
                Tüm Liste
              </Typography>
              <Typography
                variant="body2"
                sx={{ fontWeight: 600, color: 'primary.main', cursor: 'pointer' }}
                onClick={() => navigate('/employee/requests')}
              >
                Tüm Görevleri Listele →
              </Typography>
            </Box>
          </Paper>
        </Grid>
      </Grid>

      {/* Recent Assigned Tasks */}
      <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: { xs: 2, sm: 3 } }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
            <Typography variant="h6" sx={{ fontWeight: 600, color: '#1E293B' }}>
              Bana Atanan Son Görevler
            </Typography>
            {data && data.items.length > 0 && (
              <Button size="small" onClick={() => navigate('/employee/requests')}>
                Tümünü Gör ({data.totalCount})
              </Button>
            )}
          </Box>

          {isLoading && <LoadingSkeleton rows={4} variant="table" />}

          {error && <ErrorAlert error={error} onRetry={() => refetch()} />}

          {!isLoading && !error && data && data.items.length === 0 && (
            <EmptyState
              title="Atanmış Görev Yok"
              description="Şu anda size atanmış bekleyen veya işlemde olan bir saha görevi bulunmamaktadır."
            />
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <TableContainer component={Paper} elevation={0}>
              <Table sx={{ minWidth: 650 }} aria-label="personel görevler tablosu">
                <TableHead sx={{ backgroundColor: '#F8FAFC' }}>
                  <TableRow>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Başlık</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Kategori</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Durum</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Öncelik</TableCell>
                    <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Atanma / Talep Tarihi</TableCell>
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
                      onClick={() => navigate(`/employee/requests/${row.id}`)}
                    >
                      <TableCell component="th" scope="row">
                        <Typography variant="body2" sx={{ fontWeight: 600, color: '#0F172A' }}>
                          {row.title}
                        </Typography>
                        {row.location?.addressText && (
                          <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 280, display: 'block' }}>
                            {row.location.addressText}
                          </Typography>
                        )}
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
                            navigate(`/employee/requests/${row.id}`);
                          }}
                        >
                          Detay ve İşlem
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
