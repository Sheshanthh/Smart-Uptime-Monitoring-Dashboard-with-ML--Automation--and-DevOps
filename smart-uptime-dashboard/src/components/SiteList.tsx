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
        return <CheckCircleIcon sx={{ color: '#48bb78' }} />;
      case 'down':
        return <ErrorIcon sx={{ color: '#f56565' }} />;
      default:
        return <HelpIcon sx={{ color: '#a0aec0' }} />;
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'up':
        return '#48bb78';
      case 'down':
        return '#f56565';
      default:
        return '#a0aec0';
    }
  };

  const getLatencyColor = (latency: number | null) => {
    if (!latency) return '#a0aec0';
    if (latency < 200) return '#48bb78';
    if (latency < 500) return '#ed8936';
    return '#f56565';
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
              mb: 2,
              borderRadius: 3,
              background: 'linear-gradient(135deg, rgba(26, 31, 46, 0.8) 0%, rgba(45, 55, 72, 0.8) 100%)',
              border: '1px solid rgba(255, 255, 255, 0.1)',
              transition: 'all 0.3s ease-in-out',
              position: 'relative',
              overflow: 'hidden',
              '&:hover': {
                background: 'linear-gradient(135deg, rgba(26, 31, 46, 0.9) 0%, rgba(45, 55, 72, 0.9) 100%)',
                transform: 'translateX(4px)',
                boxShadow: '0 4px 12px rgba(0, 0, 0, 0.3)',
                '& .status-indicator': {
                  transform: 'scale(1.1)',
                },
              },
              '&::before': {
                content: '""',
                position: 'absolute',
                left: 0,
                top: 0,
                bottom: 0,
                width: '4px',
                background: getStatusColor(status) === '#48bb78' ? 'linear-gradient(135deg, #48bb78 0%, #68d391 100%)' : 
                           getStatusColor(status) === '#f56565' ? 'linear-gradient(135deg, #f56565 0%, #fc8181 100%)' :
                           'linear-gradient(135deg, #a0aec0 0%, #cbd5e0 100%)',
              },
            }}
          >
            <Box sx={{ mr: 2 }} className="status-indicator">
              <Box sx={{
                width: 40,
                height: 40,
                borderRadius: '50%',
                background: getStatusColor(status) === '#48bb78' ? 'linear-gradient(135deg, #48bb78 0%, #68d391 100%)' : 
                           getStatusColor(status) === '#f56565' ? 'linear-gradient(135deg, #f56565 0%, #fc8181 100%)' :
                           'linear-gradient(135deg, #a0aec0 0%, #cbd5e0 100%)',
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                transition: 'transform 0.3s ease-in-out',
              }}>
                {getStatusIcon(status)}
              </Box>
            </Box>
            
            <ListItemText
              primary={
                <Box display="flex" alignItems="center" gap={1}>
                  <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                    {site.name || site.url}
                  </Typography>
                                     {isAnomaly && (
                     <Tooltip title="ML Anomaly Detected">
                       <WarningIcon sx={{ color: '#ed8936', fontSize: 16 }} />
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
                   sx={{ color: '#f56565' }}
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