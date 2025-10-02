// src/app/core/api.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface LoginRequest { email: string; password: string; }
export interface LoginResponse { accessToken: string; }

// DTOs interfaces 
export interface ShortUrlDto {
  id: number;
  originalUrl: string;
  shortCode: string;
  createdAt: string;          
  createdByUserId: string;    
}

// Generic paged result
export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
}
// { items: T[]; page: number; pageSize: number; total: number
export interface AboutDto {
  content: string;
  updatedAt: string;
}
// apiService with methods for backend interaction
@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  login(body: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>('/api/auth/login', body);
  }

  // Public list with search and pagination //
  getUrls(q = '', page = 1, pageSize = 20): Observable<PagedResult<ShortUrlDto>> {
    const params: any = { page, pageSize };
    if (q && q.trim()) params.q = q.trim();
    return this.http.get<PagedResult<ShortUrlDto>>('/api/urls', { params });
  }

  // details view (info view) — requires authorization
  getUrl(id: number): Observable<ShortUrlDto> {
    return this.http.get<ShortUrlDto>(`/api/urls/${id}`);
  }

  createUrl(originalUrl: string): Observable<ShortUrlDto> {
    return this.http.post<ShortUrlDto>('/api/urls', { originalUrl });
  }

  deleteUrl(id: number): Observable<void> {
    return this.http.delete<void>(`/api/urls/${id}`);
  }

  // public GET About
  getAbout(): Observable<AboutDto> {
    return this.http.get<AboutDto>('/api/about');
  }

  // put About (admin only) 
  updateAbout(content: string): Observable<void> {
    return this.http.put<void>('/api/about', { content });
  }
}

/* ===== JWT helpers (for role-based UI rules) ===== */

export function getAccessToken(): string | null {
  return localStorage.getItem('accessToken');
}

export function getCurrentUserId(): string | null {
  const t = getAccessToken();
  if (!t) return null;
  try {
    const payload = JSON.parse(atob(t.split('.')[1]));
    return (payload?.sub as string) ?? null;
  } catch {
    return null;
  }
}

export function isAdminFromToken(): boolean {
  const t = getAccessToken();
  if (!t) return false;
  try {
    const payload = JSON.parse(atob(t.split('.')[1]));
    const role = payload?.role;
    return Array.isArray(role) ? role.includes('Admin') : role === 'Admin';
  } catch {
    return false;
  }
}
