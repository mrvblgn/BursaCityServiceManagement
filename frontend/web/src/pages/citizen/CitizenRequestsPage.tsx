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
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { citizenApi } from '../../api/citizenApi';
import { RequestStatus } from '../../types/serviceRequest.types';
import { PageHeader } from '../../components/common/PageHeader';
import { StatusChip } from '../../components/common/StatusChip';
import { PriorityChip } from '../../components/common/PriorityChip';
import { LoadingSkeleton } from '../../components/common/LoadingSkeleton';
import { ErrorAlert } from '../../components/common/ErrorAlert';
import { EmptyState } from '../../components/common/EmptyState';
import { formatDate } from '../../utils/formatters';

const statusFilterOptions: { label: string; value: RequestStatus | '' }[] = [
  { label: 'Tüm Durumlar', value: '' },
  { label: 'Yeni', value: 'New' },
  { label: 'İnceleniyor', value: 'Reviewing' },
  { label: 'Atandı', value: 'Assigned' },
  { label: 'İşlemde', value: 'InProgress' },
  { label: 'Çözüldü', value: 'Resolved' },
  { label: 'Kapatıldı', value: 'Closed' },
  { label: 'Reddedildi', value: 'Rejected' },
  { label: 'İptal Edildi', value: 'Cancelled' },
];

export const CitizenRequestsPage: React.FC = () => {
  const navigate = useNavigate();

  const [status, setStatus] = useState<RequestStatus | ''>('');
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  const { data, isLoading, error, refetch } = useQuery({
    queryKey: ['citizen', 'requests', { pageNumber: page + 1, pageSize, status }],
    queryFn: () => citizenApi.getMyRequests(status, page + 1, pageSize),
  });

  const handleChangePage = (_: unknown, newPage: number) => {
    setPage(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(0);
  };

  return (
    <Box>
      <PageHeader
        title="Başvurularım"
        subtitle="Belediyemize yapmış olduğunuz tüm hizmet ve arıza başvurularının güncel listesi."
        action={
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddCircleOutlineIcon />}
            onClick={() => navigate('/citizen/requests/new')}
          >
            Yeni Başvuru Yap
          </Button>
        }
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
                id="status-filter"
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

      {/* Requests Table */}
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
                title="Başvuru Bulunamadı"
                description={
                  status
                    ? 'Seçili filtre kriterine uygun başvuru bulunmamaktadır.'
                    : 'Henüz kayıtlı bir başvurunuz bulunmuyor.'
                }
                actionText={status ? 'Filtreyi Temizle' : 'Yeni Başvuru Yap'}
                onAction={() => {
                  if (status) setStatus('');
                  else navigate('/citizen/requests/new');
                }}
              />
            </Box>
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <>
              <TableContainer component={Paper} elevation={0}>
                <Table sx={{ minWidth: 700 }} aria-label="başvurular tablosu">
                  <TableHead sx={{ backgroundColor: '#F8FAFC' }}>
                    <TableRow>
                      <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Başlık</TableCell>
                      <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Kategori</TableCell>
                      <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Durum</TableCell>
                      <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Öncelik</TableCell>
                      <TableCell sx={{ fontWeight: 600, color: '#475569' }}>Başvuru Tarihi</TableCell>
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
                        onClick={() => navigate(`/citizen/requests/${row.id}`)}
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

              <TablePagination
                rowsPerPageOptions={[5, 10, 25]}
                component="div"
                count={data.totalCount}
                rowsPerPage={pageSize}
                page={page}
                onPageChange={handleChangePage}
                onRowsPerPageChange={handleChangeRowsPerPage}
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
