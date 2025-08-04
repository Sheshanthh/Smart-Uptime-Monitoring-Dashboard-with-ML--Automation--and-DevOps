import React from 'react';
import {
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  IconButton,
  Chip,
  Typography,
  Box,
  Tooltip,
} from '@mui/material';
import {
  Delete as DeleteIcon,
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Help as HelpIcon,
} from '@mui/icons-material';
import { Site } from '../types';

interface SiteListProps {
  sites: Site[];
  onDelete: (siteId: number) => void;
  getSiteStatus: (siteId: number) => string;
  getSiteLatency: (siteId: number) => number | null;
  getSiteAnomaly: (siteId: number) => boolean;
}

const SiteList: React.FC<SiteListProps> = ({
  sites,
  onDelete,
  getSiteStatus,
  getSiteLatency,
  getSiteAnomaly,
}) => {
  const getStatusIcon = (status: string) => {
    switch (status) {
      case 'up':
        return <CheckCircleIcon sx={{ color: '#4caf50' }} />;
      case 'down':
        return <ErrorIcon sx={{ color: '#f44336' }} />;
      default:
        return <HelpIcon sx={{ color: '#9e9e9e' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'up':
        return '#4caf50';
      case 'down':
        return '#f44336';
      default:
        return '#9e9e9e';
    }
  };

  const getLatencyColor = (latency: number | null) => {
    if (!latency) return '#9e9e9e';
    if (latency < 200) return '#4caf50';
    if (latency < 500) return '#ff9800';
    return '#f44336';
  };

  if (sites.length === 0) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="body1" color="text.secondary">
          No sites monitored yet. Add your first site!
        </Typography>
      </Box>
    );
  }

  return (
    <List>
      {sites.map((site) => {
        const status = getSiteStatus(site.id);
        const latency = getSiteLatency(site.id);
        const isAnomaly = getSiteAnomaly(site.id);

        return (
          <ListItem
            key={site.id}
            sx={{
              mb: 1,
              borderRadius: 2,
              background: 'rgba(255, 255, 255, 0.02)',
              border: '1px solid rgba(255, 255, 255, 0.1)',
              transition: 'all 0.2s ease-in-out',
              '&:hover': {
                background: 'rgba(255, 255, 255, 0.05)',
                transform: 'translateX(4px)',
              },
            }}
          >
            <Box sx={{ mr: 2 }}>
              {getStatusIcon(status)}
            </Box>
            
            <ListItemText
              primary={
                <Box display="flex" alignItems="center" gap={1}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                    {site.name || site.url}
                  </Typography>
                  {isAnomaly && (
                    <Tooltip title="ML Anomaly Detected">
                      <WarningIcon sx={{ color: '#ff9800', fontSize: 16 }} />
                    </Tooltip>
                  )}
                </Box>
              }
              secondary={
                <Box>
                  <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                    {site.url}
                  </Typography>
                  <Box display="flex" gap={1} alignItems="center">
                    <Chip
                      label={status.toUpperCase()}
                      size="small"
                      sx={{
                        backgroundColor: `${getStatusColor(status)}20`,
                        color: getStatusColor(status),
                        border: `1px solid ${getStatusColor(status)}40`,
                      }}
                    />
                    {latency && (
                      <Chip
                        label={`${latency}ms`}
                        size="small"
                        sx={{
                          backgroundColor: `${getLatencyColor(latency)}20`,
                          color: getLatencyColor(latency),
                          border: `1px solid ${getLatencyColor(latency)}40`,
                        }}
                      />
                    )}
                  </Box>
                </Box>
              }
            />
            
            <ListItemSecondaryAction>
              <Tooltip title="Delete Site">
                <IconButton
                  edge="end"
                  onClick={() => onDelete(site.id)}
                  sx={{ color: '#f44336' }}
                >
                  <DeleteIcon />
                </IconButton>
              </Tooltip>
            </ListItemSecondaryAction>
          </ListItem>
        );
      })}
    </List>
  );
};

export default SiteList; 