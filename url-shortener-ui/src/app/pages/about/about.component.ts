import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService, AboutDto } from '../../core/api.service';

// component to view and edit the "About" page content
@Component({
  standalone: true,
  selector: 'app-about',
  imports: [CommonModule, FormsModule],
  templateUrl: './about.component.html'
})
export class AboutComponent implements OnInit {
  loading = true;
  error = '';
  content = '';
  updatedAt: string | null = null;

  get canEdit() { return !!localStorage.getItem('accessToken'); }

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getAbout().subscribe({
      next: (x: AboutDto) => {
        this.content = x?.content ?? '';
        this.updatedAt = x?.updatedAt ?? null;
        this.loading = false;
      },
      error: (_: any) => {
        this.error = 'Failed to load About';
        this.loading = false;
      }
    });
  }

  // save changes (Admin only)
  save(): void {
    if (!this.content.trim()) {
      this.error = 'Content is required';
      return;
    }
    this.error = '';
    this.api.updateAbout(this.content).subscribe({
      next: (_: void) => {
        this.updatedAt = new Date().toISOString();
      },
      error: (e: any) => {
        this.error = e?.status === 403 ? 'Forbidden (Admin only)' : 'Save failed';
      }
    });
  }
}
