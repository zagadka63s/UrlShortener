import { Routes } from '@angular/router';
import { UrlsComponent } from './pages/urls/urls.component';

export const routes: Routes = [
  { path: '', redirectTo: 'urls', pathMatch: 'full' },
  { path: 'urls', component: UrlsComponent },
  {
    path: 'urls/:id',
    loadComponent: () =>
      import('./pages/urls/url-details/url-details.component')
        .then(m => m.UrlDetailsComponent)
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./pages/about/about.component')   
        .then(m => m.AboutComponent)
  },
  { path: '**', redirectTo: 'urls' }            
];
