import React from 'react';
import {
  Card,
  CardContent,
  Typography,
  Box,
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
      icon: <SpeedIcon sx={{ fontSize: 32, color: '#00b8d4' }} />,
      color: '#00b8d4',
      gradient: 'linear-gradient(135deg, #00b8d4 0%, #5ddef4 100%)',
    },
    {
      title: 'Up',
      value: upSites,
      icon: <CheckCircleIcon sx={{ fontSize: 32, color: '#48bb78' }} />,
      color: '#48bb78',
      gradient: 'linear-gradient(135deg, #48bb78 0%, #68d391 100%)',
    },
    {
      title: 'Down',
      value: downSites,
      icon: <ErrorIcon sx={{ fontSize: 32, color: '#f56565' }} />,
      color: '#f56565',
      gradient: 'linear-gradient(135deg, #f56565 0%, #fc8181 100%)',
    },
    {
      title: 'Anomalies',
      value: anomalySites,
      icon: <WarningIcon sx={{ fontSize: 32, color: '#ed8936' }} />,
      color: '#ed8936',
      gradient: 'linear-gradient(135deg, #ed8936 0%, #f6ad55 100%)',
    },
    {
      title: 'Avg Latency',
      value: `${averageLatency}ms`,
      icon: <SpeedIcon sx={{ fontSize: 32, color: '#4299e1' }} />,
      color: '#4299e1',
      gradient: 'linear-gradient(135deg, #4299e1 0%, #63b3ed 100%)',
    },
  ];

  return (
    <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
      {stats.map((stat, index) => (
        <Box key={index} sx={{ flex: '1 1 200px', minWidth: 0 }}>
          <Card 
            sx={{ 
              height: '100%',
              background: `linear-gradient(135deg, rgba(26, 31, 46, 0.8) 0%, rgba(45, 55, 72, 0.8) 100%)`,
              border: `1px solid rgba(255, 255, 255, 0.1)`,
              transition: 'all 0.3s ease-in-out',
              position: 'relative',
              overflow: 'hidden',
              '&:hover': {
                transform: 'translateY(-4px)',
                boxShadow: `0 8px 25px rgba(0, 0, 0, 0.3)`,
                '& .gradient-overlay': {
                  opacity: 1,
                },
              },
              '&::before': {
                content: '""',
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                height: '3px',
                background: stat.gradient,
              },
            }}
          >
            <Box 
              className="gradient-overlay"
              sx={{
                position: 'absolute',
                top: 0,
                left: 0,
                right: 0,
                bottom: 0,
                background: stat.gradient,
                opacity: 0.05,
                transition: 'opacity 0.3s ease-in-out',
              }}
            />
            <CardContent sx={{ textAlign: 'center', p: 3, position: 'relative', zIndex: 1 }}>
              <Box sx={{ 
                mb: 2, 
                display: 'flex', 
                justifyContent: 'center',
                alignItems: 'center',
                width: 60,
                height: 60,
                borderRadius: '50%',
                background: `linear-gradient(135deg, ${stat.color}20 0%, ${stat.color}10 100%)`,
                mx: 'auto',
              }}>
                {stat.icon}
              </Box>
              <Typography variant="h3" component="div" sx={{ fontWeight: 700, mb: 1, color: stat.color }}>
                {stat.value}
              </Typography>
              <Typography variant="body2" color="text.secondary" sx={{ fontWeight: 500 }}>
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