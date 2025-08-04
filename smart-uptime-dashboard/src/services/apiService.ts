import axios from 'axios';
import { Site, PingResult } from '../types';

const API_BASE_URL = 'http://localhost:5198/api';

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
});

export const apiService = {
  // Sites
  async getSites(): Promise<Site[]> {
    const response = await api.get('/sites');
    return response.data;
  },

  async getSite(id: number): Promise<Site> {
    const response = await api.get(`/sites/${id}`);
    return response.data;
  },

  async addSite(site: { url: string; name?: string }): Promise<Site> {
    const response = await api.post('/sites', site);
    return response.data;
  },

  async updateSite(id: number, site: { url: string; name?: string }): Promise<Site> {
    const response = await api.put(`/sites/${id}`, site);
    return response.data;
  },

  async deleteSite(id: number): Promise<void> {
    await api.delete(`/sites/${id}`);
  },

  // Ping Results
  async getRecentPingResults(count: number = 100): Promise<PingResult[]> {
    const response = await api.get(`/status/pingresults/recent?count=${count}`);
    return response.data;
  },

  // Status
  async getAllStatuses(): Promise<any[]> {
    const response = await api.get('/status');
    return response.data;
  },

  async getSiteStatus(siteId: number): Promise<any> {
    const response = await api.get(`/status/${siteId}`);
    return response.data;
  },

  async getSiteHistory(siteId: number): Promise<any[]> {
    const response = await api.get(`/status/${siteId}/history`);
    return response.data;
  },
}; 