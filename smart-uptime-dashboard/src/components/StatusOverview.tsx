import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
  Chip,
} from '@mui/material';
import {
  CheckCircle as CheckCircleIcon,
  Error as ErrorIcon,
  Warning as WarningIcon,
  Speed as SpeedIcon,
} from '@mui/icons-material';
import { Site, PingResult } from '../types';

interface StatusOverviewProps {
  sites: Site[];
  pingResults: PingResult[];
}

const StatusOverview: React.FC<StatusOverviewProps> = ({ sites, pingResults }) => {
  const totalSites = sites.length;
  const activeSites = sites.filter(site => site.isActive).length;
  
  // Calculate status statistics
  const upSites = sites.filter(site => {
    const latestPing = pingResults.find(ping => ping.siteId === site.id);
    return latestPing && latestPing.statusCode >= 200 && latestPing.statusCode < 300;
  }).length;
  
  const downSites = sites.filter(site => {
    const latestPing = pingResults.find(ping => ping.siteId === site.id);
    return latestPing && (latestPing.statusCode < 200 || latestPing.statusCode >= 300);
  }).length;
  
  const anomalySites = sites.filter(site => {
    const latestPing = pingResults.find(ping => ping.siteId === site.id);
    return latestPing && latestPing.isAnomaly;
  }).length;

  // Calculate average latency
  const validLatencies = pingResults
    .filter(ping => ping.latencyMs !== null && ping.latencyMs !== undefined)
    .map(ping => ping.latencyMs!);
  
  const averageLatency = validLatencies.length > 0 
    ? Math.round(validLatencies.reduce((sum, latency) => sum + latency, 0) / validLatencies.length)
    : 0;

  const stats = [
    {
      title: 'Total Sites',
      value: totalSites,
      icon: <SpeedIcon sx={{ fontSize: 40, color: '#00d4ff' }} />,
      color: '#00d4ff',
    },
    {
      title: 'Up',
      value: upSites,
      icon: <CheckCircleIcon sx={{ fontSize: 40, color: '#4caf50' }} />,
      color: '#4caf50',
    },
    {
      title: 'Down',
      value: downSites,
      icon: <ErrorIcon sx={{ fontSize: 40, color: '#f44336' }} />,
      color: '#f44336',
    },
    {
      title: 'Anomalies',
      value: anomalySites,
      icon: <WarningIcon sx={{ fontSize: 40, color: '#ff9800' }} />,
      color: '#ff9800',
    },
    {
      title: 'Avg Latency',
      value: `${averageLatency}ms`,
      icon: <SpeedIcon sx={{ fontSize: 40, color: '#9c27b0' }} />,
      color: '#9c27b0',
    },
  ];

  return (
    <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
      {stats.map((stat, index) => (
        <Box key={index} sx={{ flex: '1 1 200px', minWidth: 0 }}>
          <Card 
            sx={{ 
              height: '100%',
              background: `linear-gradient(135deg, ${stat.color}15 0%, ${stat.color}05 100%)`,
              border: `1px solid ${stat.color}30`,
              transition: 'transform 0.2s ease-in-out',
              '&:hover': {
                transform: 'translateY(-4px)',
                boxShadow: `0 8px 25px ${stat.color}20`,
              },
            }}
          >
            <CardContent sx={{ textAlign: 'center', p: 3 }}>
              <Box sx={{ mb: 2 }}>
                {stat.icon}
              </Box>
              <Typography variant="h3" component="div" sx={{ fontWeight: 700, mb: 1 }}>
                {stat.value}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {stat.title}
              </Typography>
            </CardContent>
          </Card>
        </Box>
      ))}
    </Box>
  );
};

export default StatusOverview; 