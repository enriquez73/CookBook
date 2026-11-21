import { Component, inject, input, OnInit, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { Media, RecipeDetail, functionLabel } from '../core/models';
import { RecipeService } from '../core/recipe.service';

@Component({
  selector: 'app-recipe-detail',
  imports: [RouterLink],
  template: `
    @if (recipe(); as r) {
      <div class="row head">
        <h1>{{ r.title }}</h1>
        <a class="btn" [routerLink]="['/ricette', r.id, 'modifica']">Modifica</a>
        <button class="btn danger" (click)="remove()">Elimina</button>
      </div>
      <p class="muted">
        {{ label(r.cookerFunction) }} · {{ r.cookingTimeMinutes }} min · {{ r.servings }} {{ r.servings === 1 ? 'persona' : 'persone' }}
        @if (r.riceCups > 0) { · {{ r.riceCups }} tazze di riso crudo }
        @if (r.usesSteamer) { · con vaporiera }
      </p>
      @for (t of r.tags; track t) { <span class="chip">{{ t }}</span> }
      @if (r.description) { <p>{{ r.description }}</p> }

      <div class="gallery">
        @for (m of photos(); track m.id) { <img [src]="m.url" [alt]="m.caption ?? r.title" /> }
      </div>
      @for (m of videos(); track m.id) {
        <figure class="card">
          @if (m.kind === 'VideoFile') {
            <video [src]="m.url" controls preload="metadata"></video>
          } @else if (embed(m.url); as e) {
            <iframe [src]="e" allowfullscreen title="Video ricetta"></iframe>
          } @else {
            <a [href]="m.url" target="_blank" rel="noopener noreferrer">Guarda il video ↗</a>
          }
          @if (m.caption) { <figcaption class="muted">{{ m.caption }}</figcaption> }
        </figure>
      }

      <div class="cols">
        <section class="card">
          <h2>Ingredienti</h2>
          <ul>
            @for (i of r.ingredients; track $index) {
              <li><strong>{{ i.quantity }}</strong> {{ i.name }}</li>
            }
          </ul>
        </section>
        <section class="card">
          <h2>Preparazione</h2>
          <ol>
            @for (s of r.steps; track $index) { <li>{{ s }}</li> }
          </ol>
        </section>
      </div>
    } @else if (error()) {
      <p class="error">{{ error() }}</p>
    }
  `,
  styles: `
    .head h1 { flex: 1; margin: 0; }
    .gallery { display: flex; gap: .5rem; flex-wrap: wrap; margin: 1rem 0; }
    .gallery img { max-height: 260px; border-radius: 8px; }
    video, iframe { width: 100%; aspect-ratio: 16/9; border: 0; }
    figure { margin: 1rem 0; }
    .cols { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 1rem; margin-top: 1rem; }
    li { margin-bottom: .4rem; }
  `,
})
export class RecipeDetailPage implements OnInit {
  readonly id = input.required<string>();
  private readonly service = inject(RecipeService);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  protected readonly label = functionLabel;
  protected readonly recipe = signal<RecipeDetail | null>(null);
  protected readonly error = signal('');

  ngOnInit(): void {
    this.service.get(this.id()).subscribe({
      next: (r) => this.recipe.set(r),
      error: () => this.error.set('Ricetta non trovata.'),
    });
  }

  protected photos(): Media[] { return this.recipe()?.media.filter((m) => m.kind === 'Photo') ?? []; }
  protected videos(): Media[] { return this.recipe()?.media.filter((m) => m.kind !== 'Photo') ?? []; }

  protected embed(url: string): SafeResourceUrl | null {
    const yt = /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([\w-]{6,})/.exec(url);
    if (yt) return this.sanitizer.bypassSecurityTrustResourceUrl(`https://www.youtube.com/embed/${yt[1]}`);
    const vimeo = /vimeo\.com\/(\d+)/.exec(url);
    if (vimeo) return this.sanitizer.bypassSecurityTrustResourceUrl(`https://player.vimeo.com/video/${vimeo[1]}`);
    return null;
  }

  protected remove(): void {
    if (!confirm('Eliminare definitivamente questa ricetta?')) return;
    this.service.delete(this.id()).subscribe({
      next: () => this.router.navigate(['/']),
      error: () => this.error.set('Eliminazione non riuscita.'),
    });
  }
}
