import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ApiService, ShortUrlDto } from '../../../core/api.service';

@Component({
  standalone: true,
  selector: 'app-url-details',
  imports: [CommonModule, RouterLink],
  templateUrl: './url-details.component.html'
})
export class UrlDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private api = inject(ApiService);

  loading = true;
  error: string | null = null;
  item: ShortUrlDto | null = null;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = 'Invalid id';
      this.loading = false;
      return;
    }

    this.api.getUrl(id).subscribe({
      next: (x: ShortUrlDto) => {
        this.item = x;
        this.loading = false;
      },
      error: (e: any) => {
        this.error = e?.error?.message ?? 'Failed to load details';
        this.loading = false;
      }
    });
  }
}
