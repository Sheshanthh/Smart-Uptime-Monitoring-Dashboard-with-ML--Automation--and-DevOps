import React, { useState, useEffect } from 'react';
import {
  Box,
  Container,
  Typography,
  Card,
  CardContent,
  AppBar,
  Toolbar,
  IconButton,
  Fab,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Button,
  Chip,
  Alert,
  CircularProgress,
  Paper,
} from '@mui/material';
import {
  Add as AddIcon,
  Close as CloseIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Refresh as RefreshIcon,
} from '@mui/icons-material';
import SiteList from './SiteList';
import LatencyChart from './LatencyChart';
import StatusOverview from './StatusOverview';
import { Site, PingResult } from '../types';
import { apiService } from '../services/apiService';

const Dashboard: React.FC = () => {
  const [sites, setSites] = useState<Site[]>([]);
  const [pingResults, setPingResults] = useState<PingResult[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [openAddDialog, setOpenAddDialog] = useState(false);
  const [newSite, setNewSite] = useState({ url: '', name: '' });

  useEffect(() => {
    fetchData();
    const interval = setInterval(fetchData, 30000); // Refresh every 30 seconds
    return () => clearInterval(interval);
  }, []);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [sitesData, pingData] = await Promise.all([
        apiService.getSites(),
        apiService.getRecentPingResults(),
      ]);
      setSites(sitesData);
      setPingResults(pingData);
      setError(null);
    } catch (err) {
      setError('Failed to fetch data. Please check your connection.');
      console.error('Error fetching data:', err);
    } finally {
      setLoading(false);
    }
  };

  const handleAddSite = async () => {
    try {
      const addedSite = await apiService.addSite(newSite);
      setSites([...sites, addedSite]);
      setNewSite({ url: '', name: '' });
      setOpenAddDialog(false);
    } catch (err) {
      setError('Failed to add site. Please try again.');
    }
  };

  const handleDeleteSite = async (siteId: number) => {
    try {
      await apiService.deleteSite(siteId);
      setSites(sites.filter(site => site.id !== siteId));
    } catch (err) {
      setError('Failed to delete site. Please try again.');
    }
  };

  const getSiteStatus = (siteId: number) => {
    const latestPing = pingResults.find(ping => ping.siteId === siteId);
    if (!latestPing) return 'unknown';
    if (latestPing.statusCode >= 200 && latestPing.statusCode < 300) return 'up';
    return 'down';
  };

  const getSiteLatency = (siteId: number) => {
    const latestPing = pingResults.find(ping => ping.siteId === siteId);
    return latestPing?.latencyMs || null;
  };

  const getSiteAnomaly = (siteId: number) => {
    const latestPing = pingResults.find(ping => ping.siteId === siteId);
    return latestPing?.isAnomaly || false;
  };

  if (loading && sites.length === 0) {
    return (
      <Box display="flex" justifyContent="center" alignItems="center" minHeight="100vh">
        <CircularProgress size={60} />
      </Box>
    );
  }

  return (
    <Box>
      <AppBar position="static" elevation={0} sx={{ background: 'rgba(26, 26, 26, 0.8)', backdropFilter: 'blur(10px)' }}>
        <Toolbar>
          <Typography variant="h4" component="h1" sx={{ flexGrow: 1, fontWeight: 700 }}>
            Smart Uptime Monitor
          </Typography>
          <IconButton onClick={fetchData} color="inherit">
            <RefreshIcon />
          </IconButton>
        </Toolbar>
      </AppBar>

      <Container maxWidth="xl" sx={{ mt: 4, mb: 4 }}>
        {error && (
          <Alert severity="error" sx={{ mb: 3 }}>
            {error}
          </Alert>
        )}

        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
          {/* Status Overview */}
          <Box>
            <StatusOverview sites={sites} pingResults={pingResults} />
          </Box>

          {/* Sites List and Chart */}
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            {/* Sites List */}
            <Box sx={{ flex: '1 1 500px', minWidth: 0 }}>
              <Card>
                <CardContent>
                  <Typography variant="h5" gutterBottom>
                    Monitored Sites
                  </Typography>
                  <SiteList
                    sites={sites}
                    onDelete={handleDeleteSite}
                    getSiteStatus={getSiteStatus}
                    getSiteLatency={getSiteLatency}
                    getSiteAnomaly={getSiteAnomaly}
                  />
                </CardContent>
              </Card>
            </Box>

            {/* Latency Chart */}
            <Box sx={{ flex: '1 1 500px', minWidth: 0 }}>
              <Card>
                <CardContent>
                  <Typography variant="h5" gutterBottom>
                    Latency Trends
                  </Typography>
                  <LatencyChart pingResults={pingResults} sites={sites} />
                </CardContent>
              </Card>
            </Box>
          </Box>
        </Box>
      </Container>

      {/* Add Site FAB */}
      <Fab
        color="primary"
        aria-label="add site"
        sx={{ position: 'fixed', bottom: 24, right: 24 }}
        onClick={() => setOpenAddDialog(true)}
      >
        <AddIcon />
      </Fab>

      {/* Add Site Dialog */}
      <Dialog open={openAddDialog} onClose={() => setOpenAddDialog(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          Add New Site
          <IconButton
            aria-label="close"
            onClick={() => setOpenAddDialog(false)}
            sx={{ position: 'absolute', right: 8, top: 8 }}
          >
            <CloseIcon />
          </IconButton>
        </DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="Site Name"
            fullWidth
            variant="outlined"
            value={newSite.name}
            onChange={(e) => setNewSite({ ...newSite, name: e.target.value })}
            sx={{ mb: 2 }}
          />
          <TextField
            margin="dense"
            label="URL"
            fullWidth
            variant="outlined"
            value={newSite.url}
            onChange={(e) => setNewSite({ ...newSite, url: e.target.value })}
            placeholder="https://example.com"
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenAddDialog(false)}>Cancel</Button>
          <Button onClick={handleAddSite} variant="contained" disabled={!newSite.url}>
            Add Site
          </Button>
        </DialogActions>
      </Dialog>
    </Box>
  );
};

export default Dashboard; 