import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { ApiService, ShortUrlDto, getCurrentUserId, isAdminFromToken } from '../../core/api.service';
import { RealtimeService } from '../../core/realtime.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-urls',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './urls.component.html',
  styleUrls: ['./urls.component.scss']
})
export class UrlsComponent implements OnInit, OnDestroy {
  // auth
  email = 'admin@example.com';
  password = 'Admin#12345';
  loggedIn = signal<boolean>(!!localStorage.getItem('accessToken'));
  currentUserId: string | null = getCurrentUserId();
  admin = isAdminFromToken();

  // ------create------
  newUrl = 'https://example.com/';

  // --------list + search + pagination-------
  urls: ShortUrlDto[] = [];
  q = '';
  page = 1;
  pageSize = 10;
  total = 0;

  busy = false;
  error = '';

  
  Math = Math;

  private sub?: Subscription;

  constructor(private api: ApiService, private rt: RealtimeService) {}

  ngOnInit(): void {
    this.rt.start();
    this.sub = this.rt.urlsChanged$.subscribe(() => this.load());
    this.load();
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
    this.rt.stop();
  }

  async login() {
    this.error = '';
    this.busy = true;
    this.api.login({ email: this.email, password: this.password }).subscribe({
      next: r => {
        localStorage.setItem('accessToken', r.accessToken);
        this.loggedIn.set(true);
        this.currentUserId = getCurrentUserId();
        this.admin = isAdminFromToken();
        this.busy = false;
      },
      error: _ => {
        this.error = 'Login failed';
        this.busy = false;
      }
    });
  }

  logout() {
    localStorage.removeItem('accessToken');
    this.loggedIn.set(false);
    this.currentUserId = null;
    this.admin = false;
  }

  // ------- search & pagination -------
  load() {
    this.error = '';
    this.busy = true;
    this.api.getUrls(this.q, this.page, this.pageSize).subscribe({
      next: r => {
        this.urls = r.items;
        this.page = r.page;
        this.pageSize = r.pageSize;
        this.total = r.total;
        this.busy = false;
      },
      error: _ => { this.error = 'Load failed'; this.busy = false; }
    });
  }

  onSearch() {
    this.page = 1;
    this.load();
  }

  totalPages(): number {
    return Math.max(1, Math.ceil(this.total / this.pageSize));
  }

  canPrev(): boolean { return this.page > 1; }
  canNext(): boolean { return this.page < this.totalPages(); }

  prevPage() { if (this.canPrev()) { this.page--; this.load(); } }
  nextPage() { if (this.canNext()) { this.page++; this.load(); } }

  // ------- create with validation -------
  isUrlInvalid(): boolean {
    return !!this.urlValidationError(this.newUrl);
  }

  private urlValidationError(value: string | null | undefined): string | null {
    const raw = (value ?? '').trim();
    if (!raw) return 'URL is required';
    try {
      const u = new URL(raw);
      if (u.protocol !== 'http:' && u.protocol !== 'https:') return 'Only http/https protocols are allowed';
      if (!u.hostname) return 'Host is required';
      return null;
    } catch {
      return 'Invalid URL format';
    }
  }

  create() {
    const err = this.urlValidationError(this.newUrl);
    if (err) { this.error = err; return; }

    this.error = '';
    this.busy = true;
    this.api.createUrl(this.newUrl.trim()).subscribe({
      next: _ => {
        this.newUrl = '';
        this.busy = false;
        this.load();
      },
      error: e => {
        this.error = e?.error?.message ?? 'Create failed';
        this.busy = false;
      }
    });
  }

  // ------- delete with role/UI rules -------
  isOwner(u: ShortUrlDto) {
    return !!this.currentUserId && u.createdByUserId === this.currentUserId;
  }
  canDelete(u: ShortUrlDto) {
    return this.admin || this.isOwner(u);
  }

  remove(id: number) {
    if (!confirm('Delete this short URL?')) return;
    this.error = '';
    this.busy = true;
    this.api.deleteUrl(id).subscribe({
      next: _ => { this.busy = false; },
      error: _ => { this.error = 'Delete failed'; this.busy = false; }
    });
  }

  // ------- UI helpers -------
  shortLink(code: string) { return `/r/${code}`; }

  async copyShort(code: string) {
    try {
      await navigator.clipboard.writeText(this.shortLink(code));
    } catch {
      this.error = 'Cannot copy to clipboard';
      setTimeout(() => (this.error = ''), 2000);
    }
  }
}
