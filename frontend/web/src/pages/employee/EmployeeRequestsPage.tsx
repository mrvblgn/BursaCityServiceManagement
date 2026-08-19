import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  TablePagination,
  TextField,
  MenuItem,
  Grid,
} from '@mui/material';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { employeeApi } from '../../api/employeeApi';
import { RequestStatus } from '../../types/serviceRequest.types';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { EmptyState } from '../../components/common/EmptyState';
import { formatDate } from '../../utils/formatters';

const statusFilterOptions: { label: string; value: RequestStatus | '' }[] = [
  { label: 'Tüm Görevlerim', value: '' },
  { label: 'Yeni Atananlar (Assigned)', value: 'Assigned' },
  { label: 'İşlemdekiler (InProgress)', value: 'InProgress' },
  { label: 'Çözümlenenler (Resolved)', value: 'Resolved' },
  { label: 'Kapatılanlar (Closed)', value: 'Closed' },
];

export const EmployeeRequestsPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const initialStatus = (searchParams.get('status') as RequestStatus | null) || '';
  const [status, setStatus] = useState<RequestStatus | ''>(initialStatus);
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['employee', 'requests', { pageNumber: page + 1, pageSize, status }],
    queryFn: () => employeeApi.getMyAssignedRequests(status, page + 1, pageSize),
  });

  return (
    <Box>
      <PageHeader
        title="Görevlerim"
        subtitle="Birim yöneticisi tarafından şahsınıza atanmış belediye saha hizmeti ve onarım talepleri."
      />

      {/* Filter Bar */}
      <Card sx={{ mb: 3, borderRadius: 2, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={4}>
              <TextField
                select
                fullWidth
                size="small"
                id="employee-status-filter"
                label="Duruma Göre Filtrele"
                value={status}
                onChange={(e) => {
                  setStatus(e.target.value as RequestStatus | '');
                  setPage(0);
                }}
              >
                {statusFilterOptions.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Table */}
      <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: 0 }}>
          {isLoading && (
            <Box sx={{ p: 3 }}>
              <LoadingSkeleton rows={5} variant="table" />
            </Box>
          )}

          {error && (
            <Box sx={{ p: 3 }}>
              <ErrorAlert error={error} onRetry={() => refetch()} />
            </Box>
          )}

          {!isLoading && !error && data && data.items.length === 0 && (
            <Box sx={{ p: 4 }}>
              <EmptyState
                title="Görev Bulunamadı"
                description={
                  status
                    ? 'Seçili filtreye uygun atanmış görev bulunmamaktadır.'
                    : 'Şu anda size atanmış aktif bir görev bulunmamaktadır.'
                }
                actionText={status ? 'Filtreyi Temizle' : undefined}
                onAction={status ? () => setStatus('') : undefined}
              />
            </Box>
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <>
              <TableContainer component={Paper} elevation={0}>
                <Table sx={{ minWidth: 700 }} aria-label="personel görev tablosu">
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
                        sx={{ cursor: 'pointer' }}
                        onClick={() => navigate(`/employee/requests/${row.id}`)}
                      >
                        <TableCell component="th" scope="row">
                          <Typography variant="body2" sx={{ fontWeight: 600, color: '#0F172A' }}>
                            {row.title}
                          </Typography>
                          {row.location?.addressText && (
                            <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 300, display: 'block' }}>
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
                            Detay & İşlem
                          </Button>
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>

              <TablePagination
                rowsPerPageOptions={[5, 10, 25]}
                component="div"
                count={data.totalCount}
                rowsPerPage={pageSize}
                page={page}
                onPageChange={(_, newPage) => setPage(newPage)}
                onRowsPerPageChange={(e) => {
                  setPageSize(parseInt(e.target.value, 10));
                  setPage(0);
                }}
                labelRowsPerPage="Sayfa başına:"
                labelDisplayedRows={({ from, to, count }) => `${from}-${to} / ${count}`}
              />
            </>
          )}
        </CardContent>
      </Card>
    </Box>
  );
};
