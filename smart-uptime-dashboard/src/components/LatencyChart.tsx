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
} from 'recharts';
import { Box, Typography } from '@mui/material';
import { Site, PingResult } from '../types';

interface LatencyChartProps {
  pingResults: PingResult[];
  sites: Site[];
}

const LatencyChart: React.FC<LatencyChartProps> = ({ pingResults, sites }) => {

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
            background: 'linear-gradient(135deg, rgba(26, 31, 46, 0.95) 0%, rgba(45, 55, 72, 0.95) 100%)',
            border: '1px solid rgba(255, 255, 255, 0.1)',
            borderRadius: 3,
            p: 3,
            boxShadow: '0 8px 32px rgba(0,0,0,0.3)',
            backdropFilter: 'blur(10px)',
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
    <Box sx={{ width: '100%', height: '100%' }}>
      <ResponsiveContainer width="100%" height="100%">
                 <LineChart data={chartData} margin={{ top: 20, right: 30, left: 40, bottom: 20 }}>
          <CartesianGrid strokeDasharray="3 3" stroke="rgba(255, 255, 255, 0.1)" />
          <XAxis
            dataKey="timestamp"
            stroke="#a0aec0"
            fontSize={12}
            tick={{ fill: '#a0aec0' }}
          />
                     <YAxis
             stroke="#a0aec0"
             fontSize={12}
             tick={{ fill: '#a0aec0' }}
             label={{ value: 'Latency (ms)', angle: -90, position: 'insideLeft', fill: '#a0aec0', fontSize: 12 }}
           />
          <Tooltip content={<CustomTooltip />} />
          
          {/* Main latency line */}
          <Line
            type="monotone"
            dataKey="latency"
            stroke="#00b8d4"
            strokeWidth={3}
            dot={{ fill: '#00b8d4', strokeWidth: 2, r: 4 }}
            activeDot={{ r: 6, stroke: '#00b8d4', strokeWidth: 2 }}
          />
          
          {/* Anomaly points */}
          <Scatter
            data={chartData.filter(d => d.isAnomaly)}
            fill="#ed8936"
            shape="circle"
            dataKey="latency"
            name="Anomaly"
          />
        </LineChart>
      </ResponsiveContainer>
      
             {/* Legend */}
       <Box display="flex" justifyContent="center" gap={4} mt={3} sx={{ 
         background: 'rgba(255, 255, 255, 0.05)', 
         borderRadius: 2, 
         p: 2,
         border: '1px solid rgba(255, 255, 255, 0.1)'
       }}>
         <Box display="flex" alignItems="center" gap={2}>
           <Box
             sx={{
               width: 16,
               height: 16,
               background: 'linear-gradient(135deg, #00b8d4 0%, #5ddef4 100%)',
               borderRadius: '50%',
               boxShadow: '0 2px 4px rgba(0, 184, 212, 0.3)',
             }}
           />
           <Typography variant="body1" color="text.primary" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>
             Latency
           </Typography>
         </Box>
         <Box display="flex" alignItems="center" gap={2}>
           <Box
             sx={{
               width: 16,
               height: 16,
               background: 'linear-gradient(135deg, #ed8936 0%, #f6ad55 100%)',
               borderRadius: '50%',
               boxShadow: '0 2px 4px rgba(237, 137, 54, 0.3)',
             }}
           />
           <Typography variant="body1" color="text.primary" sx={{ fontWeight: 600, fontSize: '0.9rem' }}>
             ML Anomaly
           </Typography>
         </Box>
       </Box>
    </Box>
  );
};

export default LatencyChart; 