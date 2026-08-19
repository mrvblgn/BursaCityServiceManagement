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
import FilterAltOffIcon from '@mui/icons-material/FilterAltOff';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { managerApi } from '../../api/managerApi';
import { referenceApi } from '../../api/referenceApi';
import { Priority, RequestStatus } from '../../types/serviceRequest.types';
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

const priorityFilterOptions: { label: string; value: Priority | '' }[] = [
  { label: 'Tüm Öncelikler', value: '' },
  { label: 'Düşük', value: 'Low' },
  { label: 'Orta', value: 'Medium' },
  { label: 'Yüksek', value: 'High' },
  { label: 'Kritik', value: 'Critical' },
];

export const ManagerRequestsPage: React.FC = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  const initialStatus = (searchParams.get('status') as RequestStatus | null) || '';
  const [status, setStatus] = useState<RequestStatus | ''>(initialStatus);
  const [categoryId, setCategoryId] = useState<string>('');
  const [departmentId, setDepartmentId] = useState<string>('');
  const [priority, setPriority] = useState<Priority | ''>('');

  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);

  // Load lookup options
  const { data: categories } = useQuery({
    queryKey: ['reference', 'categories'],
    queryFn: referenceApi.getCategories,
    staleTime: 5 * 60 * 1000,
  });

  const { data: departments } = useQuery({
    queryKey: ['reference', 'departments'],
    queryFn: referenceApi.getDepartments,
    staleTime: 5 * 60 * 1000,
  });

  // Query manager municipal requests
  const { data, isLoading, error, refetch } = useQuery({
    queryKey: [
      'manager',
      'requests',
      { pageNumber: page + 1, pageSize, status, categoryId, departmentId, priority },
    ],
    queryFn: () =>
      managerApi.getMunicipalRequests({
        status,
        categoryId,
        departmentId,
        priority,
        pageNumber: page + 1,
        pageSize,
      }),
  });

  const handleClearFilters = () => {
    setStatus('');
    setCategoryId('');
    setDepartmentId('');
    setPriority('');
    setPage(0);
    setSearchParams({});
  };

  const hasActiveFilters = Boolean(status || categoryId || departmentId || priority);

  return (
    <Box>
      <PageHeader
        title="Belediye Hizmet Talepleri Yönetimi"
        subtitle="Vatandaşlardan gelen tüm arıza, talep ve başvuruların incelenmesi, birimlere atanması ve takibi."
      />

      {/* Filter Bar */}
      <Card sx={{ mb: 3, borderRadius: 2, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: 2 }}>
          <Grid container spacing={2} alignItems="center">
            <Grid item xs={12} sm={6} md={3}>
              <TextField
                select
                fullWidth
                size="small"
                id="filter-status"
                label="Durum"
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

            <Grid item xs={12} sm={6} md={3}>
              <TextField
                select
                fullWidth
                size="small"
                id="filter-category"
                label="Kategori"
                value={categoryId}
                onChange={(e) => {
                  setCategoryId(e.target.value);
                  setPage(0);
                }}
              >
                <MenuItem value="">Tüm Kategoriler</MenuItem>
                {categories?.map((cat) => (
                  <MenuItem key={cat.id} value={cat.id}>
                    {cat.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6} md={3}>
              <TextField
                select
                fullWidth
                size="small"
                id="filter-department"
                label="Birim / Müdürlük"
                value={departmentId}
                onChange={(e) => {
                  setDepartmentId(e.target.value);
                  setPage(0);
                }}
              >
                <MenuItem value="">Tüm Birimler</MenuItem>
                {departments?.map((dept) => (
                  <MenuItem key={dept.id} value={dept.id}>
                    {dept.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} sm={6} md={2}>
              <TextField
                select
                fullWidth
                size="small"
                id="filter-priority"
                label="Öncelik"
                value={priority}
                onChange={(e) => {
                  setPriority(e.target.value as Priority | '');
                  setPage(0);
                }}
              >
                {priorityFilterOptions.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12} md={1} sx={{ display: 'flex', justifyContent: { xs: 'flex-start', md: 'center' } }}>
              {hasActiveFilters && (
                <Button
                  size="small"
                  color="inherit"
                  variant="outlined"
                  startIcon={<FilterAltOffIcon />}
                  onClick={handleClearFilters}
                  sx={{ textTransform: 'none' }}
                >
                  Temizle
                </Button>
              )}
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {/* Requests Table */}
      <Card sx={{ borderRadius: 3, border: '1px solid #E2E8F0' }}>
        <CardContent sx={{ p: 0 }}>
          {isLoading && (
            <Box sx={{ p: 3 }}>
              <LoadingSkeleton rows={6} variant="table" />
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
                title="Talep Bulunamadı"
                description={
                  hasActiveFilters
                    ? 'Seçili filtre kriterlerine uyan hizmet talebi bulunmamaktadır.'
                    : 'Sistemde henüz kayıtlı bir hizmet talebi bulunmamaktadır.'
                }
                actionText={hasActiveFilters ? 'Filtreleri Temizle' : undefined}
                onAction={hasActiveFilters ? handleClearFilters : undefined}
              />
            </Box>
          )}

          {!isLoading && !error && data && data.items.length > 0 && (
            <>
              <TableContainer component={Paper} elevation={0}>
                <Table sx={{ minWidth: 900 }} aria-label="yönetici talepler tablosu">
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
                          {row.location?.addressText && (
                            <Typography variant="caption" color="text.secondary" noWrap sx={{ maxWidth: 260, display: 'block' }}>
                              {row.location.addressText}
                            </Typography>
                          )}
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

              <TablePagination
                rowsPerPageOptions={[5, 10, 25, 50]}
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
