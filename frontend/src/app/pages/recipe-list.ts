import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { COOKER_FUNCTIONS, RecipeFilter, RecipeSummary, functionLabel } from '../core/models';
import { RecipeService } from '../core/recipe.service';

@Component({
  selector: 'app-recipe-list',
  imports: [FormsModule, RouterLink],
  template: `
    <section class="card filters">
      <div class="row">
        <input type="search" placeholder="Cerca per titolo, ingrediente, tag…"
               [(ngModel)]="filter.search" (ngModelChange)="load()" />
        <select [(ngModel)]="filter.cookerFunction" (ngModelChange)="load()">
          <option value="">Tutte le funzioni</option>
          @for (f of functions; track f.value) { <option [value]="f.value">{{ f.label }}</option> }
        </select>
        <select [(ngModel)]="filter.tag" (ngModelChange)="load()">
          <option value="">Tutti i tag</option>
          @for (t of tags(); track t) { <option [value]="t">{{ t }}</option> }
        </select>
        <select [(ngModel)]="filter.servings" (ngModelChange)="load()">
          <option [ngValue]="null">Persone: tutte</option>
          @for (n of [1, 2, 3, 4]; track n) { <option [ngValue]="n">{{ n }} {{ n === 1 ? 'persona' : 'persone' }}</option> }
        </select>
        <select [(ngModel)]="filter.maxMinutes" (ngModelChange)="load()">
          <option [ngValue]="null">Qualsiasi durata</option>
          @for (m of [15, 30, 45, 60]; track m) { <option [ngValue]="m">Max {{ m }} min</option> }
        </select>
      </div>
    </section>

    @if (error()) { <p class="error">{{ error() }}</p> }
    @if (!loading() && recipes().length === 0 && !error()) {
      <p class="muted">Nessuna ricetta trovata. <a routerLink="/ricette/nuova">Aggiungine una!</a></p>
    }
    <div class="grid">
      @for (r of recipes(); track r.id) {
        <a class="card recipe" [routerLink]="['/ricette', r.id]">
          @if (r.coverUrl) { <img [src]="r.coverUrl" [alt]="r.title" /> } @else { <div class="placeholder">🍚</div> }
          <h3>{{ r.title }}</h3>
          <p class="muted">
            {{ label(r.cookerFunction) }} · {{ r.cookingTimeMinutes }} min · {{ r.servings }} {{ r.servings === 1 ? 'persona' : 'persone' }}
            @if (r.hasVideo) { · 🎬 }
          </p>
          @for (t of r.tags; track t) { <span class="chip">{{ t }}</span> }
        </a>
      }
    </div>
  `,
  styles: `
    .filters { margin-bottom: 1rem; }
    .recipe { display: block; color: inherit; text-decoration: none; }
    .recipe img, .placeholder { width: 100%; height: 160px; object-fit: cover; border-radius: 8px; }
    .placeholder { display: flex; align-items: center; justify-content: center; font-size: 3rem; background: #eef0e8; }
    h3 { margin: .6rem 0 .2rem; }
  `,
})
export class RecipeListPage implements OnInit {
  private readonly service = inject(RecipeService);
  protected readonly functions = COOKER_FUNCTIONS;
  protected readonly label = functionLabel;
  protected readonly recipes = signal<RecipeSummary[]>([]);
  protected readonly tags = signal<string[]>([]);
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected filter: RecipeFilter = { search: '', cookerFunction: '', tag: '', maxMinutes: null, servings: null };

  ngOnInit(): void {
    this.service.tags().subscribe({ next: (t) => this.tags.set(t) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.list(this.filter).subscribe({
      next: (r) => { this.recipes.set(r); this.loading.set(false); this.error.set(''); },
      error: () => { this.loading.set(false); this.error.set('Impossibile caricare le ricette.'); },
    });
  }
}
