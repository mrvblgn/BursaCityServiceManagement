import React, { useState } from 'react';
import {
  Box,
  Card,
  CardContent,
  Typography,
  TextField,
  Button,
  Grid,
  MenuItem,
  CircularProgress,
  Alert,
  Divider,
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import SendIcon from '@mui/icons-material/Send';
import { useNavigate } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { referenceApi } from '../../api/referenceApi';
import { citizenApi } from '../../api/citizenApi';
import { PageHeader } from '../../components/common/PageHeader';
import { getErrorMessage } from '../../utils/errorUtils';

export const CreateRequestPage: React.FC = () => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();

  const [title, setTitle] = useState('');
  const [categoryId, setCategoryId] = useState('');
  const [description, setDescription] = useState('');
  const [addressText, setAddressText] = useState('');
  const [latitude, setLatitude] = useState<string>('40.1885'); // Bursa default
  const [longitude, setLongitude] = useState<string>('29.0610');

  const [error, setError] = useState<string | null>(null);

  // Load categories from reference API
  const { data: categories, isLoading: categoriesLoading } = useQuery({
    queryKey: ['reference', 'categories'],
    queryFn: referenceApi.getCategories,
    staleTime: 5 * 60 * 1000,
  });

  const createMutation = useMutation({
    mutationFn: citizenApi.createRequest,
    onSuccess: (data) => {
      queryClient.invalidateQueries({ queryKey: ['citizen', 'requests'] });
      navigate(`/citizen/requests/${data.id}`, { replace: true });
    },
    onError: (err) => {
      setError(getErrorMessage(err));
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!title.trim()) {
      setError('Lütfen başvuru başlığını giriniz.');
      return;
    }
    if (!categoryId) {
      setError('Lütfen bir hizmet kategorisi seçiniz.');
      return;
    }

    setError(null);

    const latNum = latitude ? parseFloat(latitude) : null;
    const lngNum = longitude ? parseFloat(longitude) : null;

    createMutation.mutate({
      title: title.trim(),
      categoryId,
      description: description.trim() || undefined,
      addressText: addressText.trim() || undefined,
      latitude: isNaN(latNum as number) ? null : latNum,
      longitude: isNaN(lngNum as number) ? null : lngNum,
    });
  };

  return (
    <Box>
      <PageHeader
        title="Yeni Belediye Hizmet Başvurusu"
        subtitle="Yol, aydınlatma, çevre temizliği ve park gibi alanlardaki taleplerinizi iletebilirsiniz."
        action={
          <Button startIcon={<ArrowBackIcon />} onClick={() => navigate('/citizen')}>
            Geri Dön
          </Button>
        }
      />

      <Card sx={{ maxWidth: 800, borderRadius: 3, border: '1px solid #E2E8F0', mx: 'auto' }}>
        <CardContent sx={{ p: { xs: 3, sm: 4 } }}>
          {error && (
            <Alert severity="error" sx={{ mb: 3 }}>
              {error}
            </Alert>
          )}

          <Box component="form" onSubmit={handleSubmit} noValidate>
            <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#1E293B', mb: 2 }}>
              1. Başvuru Bilgileri
            </Typography>

            <Grid container spacing={2.5}>
              <Grid item xs={12}>
                <TextField
                  required
                  fullWidth
                  id="title"
                  label="Başvuru Başlığı / Konusu"
                  placeholder="Örn: FSM Bulvarında Asfalt Çökmesi"
                  value={title}
                  onChange={(e) => setTitle(e.target.value)}
                  disabled={createMutation.isPending}
                  helperText="Sorunu kısaca özetleyen bir başlık yazınız."
                />
              </Grid>

              <Grid item xs={12}>
                <TextField
                  select
                  required
                  fullWidth
                  id="categoryId"
                  label="Hizmet Kategorisi"
                  value={categoryId}
                  onChange={(e) => setCategoryId(e.target.value)}
                  disabled={categoriesLoading || createMutation.isPending}
                  helperText={categoriesLoading ? 'Kategoriler yükleniyor...' : 'Talebinizin ilgili olduğu kategoriyi seçiniz.'}
                >
                  {categories?.map((cat) => (
                    <MenuItem key={cat.id} value={cat.id}>
                      {cat.name}
                    </MenuItem>
                  ))}
                </TextField>
              </Grid>

              <Grid item xs={12}>
                <TextField
                  fullWidth
                  multiline
                  rows={4}
                  id="description"
                  label="Açıklama ve Detaylar"
                  placeholder="Sorunun tam yeri, durumu ve detaylarını açıklayınız..."
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  disabled={createMutation.isPending}
                />
              </Grid>
            </Grid>

            <Divider sx={{ my: 3 }} />

            <Typography variant="subtitle1" sx={{ fontWeight: 600, color: '#1E293B', mb: 2 }}>
              2. Konum ve Adres Bilgileri
            </Typography>

            <Grid container spacing={2.5}>
              <Grid item xs={12}>
                <TextField
                  fullWidth
                  id="addressText"
                  label="Açık Adres / Tarif"
                  placeholder="Örn: Ataevler Mah. Nenehatun Cad. No: 12 önü Nilüfer / BURSA"
                  value={addressText}
                  onChange={(e) => setAddressText(e.target.value)}
                  disabled={createMutation.isPending}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  id="latitude"
                  label="Enlem (Latitude)"
                  placeholder="40.1885"
                  value={latitude}
                  onChange={(e) => setLatitude(e.target.value)}
                  disabled={createMutation.isPending}
                />
              </Grid>

              <Grid item xs={12} sm={6}>
                <TextField
                  fullWidth
                  id="longitude"
                  label="Boylam (Longitude)"
                  placeholder="29.0610"
                  value={longitude}
                  onChange={(e) => setLongitude(e.target.value)}
                  disabled={createMutation.isPending}
                />
              </Grid>
            </Grid>

            <Box sx={{ mt: 4, display: 'flex', justifyContent: 'flex-end', gap: 2 }}>
              <Button variant="outlined" color="inherit" onClick={() => navigate('/citizen')} disabled={createMutation.isPending}>
                İptal
              </Button>
              <Button
                type="submit"
                variant="contained"
                color="primary"
                size="large"
                startIcon={createMutation.isPending ? <CircularProgress size={20} color="inherit" /> : <SendIcon />}
                disabled={createMutation.isPending}
                sx={{ px: 4, fontWeight: 600 }}
              >
                {createMutation.isPending ? 'Gönderiliyor...' : 'Başvuruyu Gönder'}
              </Button>
            </Box>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
};
