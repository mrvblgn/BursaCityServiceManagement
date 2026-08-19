import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Grid,
  Alert,
  CircularProgress,
  Box,
} from '@mui/material';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { referenceApi } from '../../../api/referenceApi';
import { managerApi } from '../../../api/managerApi';
import { Priority } from '../../../types/serviceRequest.types';
import { getErrorMessage } from '../../../utils/errorUtils';

interface AssignModalProps {
  open: boolean;
  requestId: string;
  onClose: () => void;
  onSuccess: () => void;
}

const priorityOptions: { label: string; value: Priority }[] = [
  { label: 'Düşük', value: 'Low' },
  { label: 'Orta', value: 'Medium' },
  { label: 'Yüksek', value: 'High' },
  { label: 'Kritik', value: 'Critical' },
];

export const AssignModal: React.FC<AssignModalProps> = ({
  open,
  requestId,
  onClose,
  onSuccess,
}) => {
  const queryClient = useQueryClient();

  const [departmentId, setDepartmentId] = useState('');
  const [employeeId, setEmployeeId] = useState('');
  const [priority, setPriority] = useState<Priority>('Medium');
  const [error, setError] = useState<string | null>(null);

  // Fetch departments
  const { data: departments, isLoading: departmentsLoading } = useQuery({
    queryKey: ['reference', 'departments'],
    queryFn: referenceApi.getDepartments,
    enabled: open,
    staleTime: 5 * 60 * 1000,
  });

  // Fetch employees for selected department
  const { data: employees, isLoading: employeesLoading } = useQuery({
    queryKey: ['reference', 'employees', departmentId],
    queryFn: () => referenceApi.getDepartmentEmployees(departmentId),
    enabled: open && !!departmentId,
  });

  // Reset employee when department changes
  const handleDepartmentChange = (newDeptId: string) => {
    setDepartmentId(newDeptId);
    setEmployeeId('');
  };

  useEffect(() => {
    if (!open) {
      setDepartmentId('');
      setEmployeeId('');
      setPriority('Medium');
      setError(null);
    }
  }, [open]);

  const assignMutation = useMutation({
    mutationFn: () =>
      managerApi.assignRequest(requestId, {
        departmentId,
        employeeId,
        priority,
      }),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['manager', 'requests'] });
      queryClient.invalidateQueries({ queryKey: ['manager', 'requests', requestId] });
      onSuccess();
      onClose();
    },
    onError: (err) => {
      setError(getErrorMessage(err));
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!departmentId) {
      setError('Lütfen bir müdürlük / birim seçiniz.');
      return;
    }
    if (!employeeId) {
      setError('Lütfen atanacak saha personelini seçiniz.');
      return;
    }

    setError(null);
    assignMutation.mutate();
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth aria-labelledby="assign-modal-title">
      <DialogTitle id="assign-modal-title" sx={{ fontWeight: 600 }}>
        Görevi Birim ve Personele Ata
      </DialogTitle>
      <Box component="form" onSubmit={handleSubmit}>
        <DialogContent dividers>
          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
              {error}
            </Alert>
          )}

          <Grid container spacing={2.5}>
            <Grid item xs={12}>
              <TextField
                select
                required
                fullWidth
                id="assign-department"
                label="Görevlendirilecek Birim / Müdürlük"
                value={departmentId}
                onChange={(e) => handleDepartmentChange(e.target.value)}
                disabled={departmentsLoading || assignMutation.isPending}
                helperText={departmentsLoading ? 'Birimler yükleniyor...' : 'Talebi yürütecek belediye birimini seçiniz.'}
              >
                {departments?.map((dept) => (
                  <MenuItem key={dept.id} value={dept.id}>
                    {dept.name}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12}>
              <TextField
                select
                required
                fullWidth
                id="assign-employee"
                label="Saha Personeli"
                value={employeeId}
                onChange={(e) => setEmployeeId(e.target.value)}
                disabled={!departmentId || employeesLoading || assignMutation.isPending}
                helperText={
                  !departmentId
                    ? 'Önce birim seçiniz.'
                    : employeesLoading
                    ? 'Personeller yükleniyor...'
                    : employees && employees.length === 0
                    ? 'Bu birimde aktif personel bulunamadı.'
                    : 'Görevi yürütecek personeli seçiniz.'
                }
              >
                {employees?.map((emp) => (
                  <MenuItem key={emp.id} value={emp.id}>
                    {emp.fullName} ({emp.email})
                  </MenuItem>
                ))}
              </TextField>
            </Grid>

            <Grid item xs={12}>
              <TextField
                select
                required
                fullWidth
                id="assign-priority"
                label="Öncelik Derecesi"
                value={priority}
                onChange={(e) => setPriority(e.target.value as Priority)}
                disabled={assignMutation.isPending}
              >
                {priorityOptions.map((opt) => (
                  <MenuItem key={opt.value} value={opt.value}>
                    {opt.label}
                  </MenuItem>
                ))}
              </TextField>
            </Grid>
          </Grid>
        </DialogContent>
        <DialogActions sx={{ px: 3, py: 2 }}>
          <Button onClick={onClose} disabled={assignMutation.isPending} color="inherit">
            İptal
          </Button>
          <Button
            type="submit"
            variant="contained"
            color="primary"
            disabled={assignMutation.isPending}
            startIcon={assignMutation.isPending ? <CircularProgress size={18} color="inherit" /> : null}
          >
            {assignMutation.isPending ? 'Atanıyor...' : 'Atamayı Tamamla'}
          </Button>
        </DialogActions>
      </Box>
    </Dialog>
  );
};
