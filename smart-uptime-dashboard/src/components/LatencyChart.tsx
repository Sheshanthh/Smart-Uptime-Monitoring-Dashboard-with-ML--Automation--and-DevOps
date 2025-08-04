import React from 'react';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Scatter,
  ScatterChart,
} from 'recharts';
import { Box, Typography, useTheme } from '@mui/material';
import { Site, PingResult } from '../types';

interface LatencyChartProps {
  pingResults: PingResult[];
  sites: Site[];
}

const LatencyChart: React.FC<LatencyChartProps> = ({ pingResults, sites }) => {
  const theme = useTheme();

  // Process data for the chart
  const chartData = pingResults
    .slice(-20) // Show last 20 results
    .map(ping => {
      const site = sites.find(s => s.id === ping.siteId);
      return {
        timestamp: new Date(ping.timestamp).toLocaleTimeString(),
        latency: ping.latencyMs || 0,
        siteName: site?.name || site?.url || 'Unknown',
        isAnomaly: ping.isAnomaly,
        statusCode: ping.statusCode,
      };
    })
    .reverse(); // Show newest on the right

  const CustomTooltip = ({ active, payload, label }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0].payload;
      return (
        <Box
          sx={{
            background: 'rgba(26, 26, 26, 0.95)',
            border: '1px solid #333',
            borderRadius: 2,
            p: 2,
            boxShadow: '0 4px 20px rgba(0,0,0,0.3)',
          }}
        >
          <Typography variant="body2" sx={{ color: '#fff', mb: 1 }}>
            <strong>Time:</strong> {label}
          </Typography>
          <Typography variant="body2" sx={{ color: '#fff', mb: 1 }}>
            <strong>Site:</strong> {data.siteName}
          </Typography>
          <Typography variant="body2" sx={{ color: '#fff', mb: 1 }}>
            <strong>Latency:</strong> {data.latency}ms
          </Typography>
          <Typography variant="body2" sx={{ color: '#fff', mb: 1 }}>
            <strong>Status:</strong> {data.statusCode}
          </Typography>
          {data.isAnomaly && (
            <Typography variant="body2" sx={{ color: '#ff9800' }}>
              ⚠️ ML Anomaly Detected
            </Typography>
          )}
        </Box>
      );
    }
    return null;
  };

  if (pingResults.length === 0) {
    return (
      <Box textAlign="center" py={4}>
        <Typography variant="body1" color="text.secondary">
          No latency data available yet. Sites will appear here after monitoring begins.
        </Typography>
      </Box>
    );
  }

  return (
    <Box sx={{ height: 400 }}>
      <ResponsiveContainer width="100%" height="100%">
        <LineChart data={chartData} margin={{ top: 20, right: 30, left: 20, bottom: 20 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="#333" />
          <XAxis
            dataKey="timestamp"
            stroke="#b0b0b0"
            fontSize={12}
            tick={{ fill: '#b0b0b0' }}
          />
          <YAxis
            stroke="#b0b0b0"
            fontSize={12}
            tick={{ fill: '#b0b0b0' }}
            label={{ value: 'Latency (ms)', angle: -90, position: 'insideLeft', fill: '#b0b0b0' }}
          />
          <Tooltip content={<CustomTooltip />} />
          
          {/* Main latency line */}
          <Line
            type="monotone"
            dataKey="latency"
            stroke="#00d4ff"
            strokeWidth={3}
            dot={{ fill: '#00d4ff', strokeWidth: 2, r: 4 }}
            activeDot={{ r: 6, stroke: '#00d4ff', strokeWidth: 2 }}
          />
          
          {/* Anomaly points */}
          <Scatter
            data={chartData.filter(d => d.isAnomaly)}
            fill="#ff6b6b"
            shape="circle"
            dataKey="latency"
            name="Anomaly"
          />
        </LineChart>
      </ResponsiveContainer>
      
      {/* Legend */}
      <Box display="flex" justifyContent="center" gap={3} mt={2}>
        <Box display="flex" alignItems="center" gap={1}>
          <Box
            sx={{
              width: 12,
              height: 12,
              backgroundColor: '#00d4ff',
              borderRadius: '50%',
            }}
          />
          <Typography variant="body2" color="text.secondary">
            Latency
          </Typography>
        </Box>
        <Box display="flex" alignItems="center" gap={1}>
          <Box
            sx={{
              width: 12,
              height: 12,
              backgroundColor: '#ff6b6b',
              borderRadius: '50%',
            }}
          />
          <Typography variant="body2" color="text.secondary">
            ML Anomaly
          </Typography>
        </Box>
      </Box>
    </Box>
  );
};

export default LatencyChart; 