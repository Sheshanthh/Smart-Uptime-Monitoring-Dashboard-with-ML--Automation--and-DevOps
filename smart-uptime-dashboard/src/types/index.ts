export interface Site {
  id: number;
  url: string;
  name?: string;
  createdAt: string;
  isActive: boolean;
}

export interface PingResult {
  id: number;
  siteId: number;
  timestamp: string;
  latencyMs?: number;
  statusCode: number;
  isAnomaly: boolean;
  errorMessage?: string;
}

export interface SiteStatus {
  siteId: number;
  status: string;
  checkedAt: string;
}

export interface SiteStatusHistory {
  checkedAt: string;
  status: string;
  isAnomaly: boolean;
} 