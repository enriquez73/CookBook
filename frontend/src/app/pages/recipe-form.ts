import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import { COOKER_FUNCTIONS, CookerFunction, Media } from '../core/models';
import { RecipeService } from '../core/recipe.service';

interface FormModel {
  title: string;
  description: string;
  servings: number;
  riceCups: number;
  cookingTimeMinutes: number;
  cookerFunction: CookerFunction;
  usesSteamer: boolean;
  ingredients: { name: string; quantity: string }[];
  steps: string[];
  tags: string;
}

@Component({
  selector: 'app-recipe-form',
  imports: [FormsModule, RouterLink],
  template: `
    <h1>{{ id() ? 'Modifica ricetta' : 'Nuova ricetta' }}</h1>
    <form class="card" (ngSubmit)="save()" #f="ngForm">
      <label for="title">Titolo *</label>
      <input id="title" name="title" [(ngModel)]="m.title" required maxlength="200" />

      <label for="desc">Descrizione</label>
      <textarea id="desc" name="desc" rows="3" [(ngModel)]="m.description"></textarea>

      <div class="row">
        <div>
          <label for="fn">Funzione del cuociriso</label>
          <select id="fn" name="fn" [(ngModel)]="m.cookerFunction">
            @for (c of functions; track c.value) { <option [value]="c.value">{{ c.label }}</option> }
          </select>
        </div>
        <div>
          <label for="serv">Persone (1-4)</label>
          <input id="serv" name="serv" type="number" min="1" max="4" [(ngModel)]="m.servings" required />
        </div>
        <div>
          <label for="cups">Tazze di riso crudo (0-4)</label>
          <input id="cups" name="cups" type="number" min="0" max="4" step="0.5" [(ngModel)]="m.riceCups" />
        </div>
        <div>
          <label for="min">Tempo di cottura (min)</label>
          <input id="min" name="min" type="number" min="0" max="1440" [(ngModel)]="m.cookingTimeMinutes" />
        </div>
      </div>
      <label><input type="checkbox" name="steam" [(ngModel)]="m.usesSteamer" style="width:auto" /> Usa la vaporiera</label>

      <label for="tags">Tag (separati da virgola)</label>
      <input id="tags" name="tags" [(ngModel)]="m.tags" placeholder="veloce, vegetariano" />

      <h3>Ingredienti</h3>
      @for (i of m.ingredients; track $index; let n = $index) {
        <div class="row">
          <input [name]="'iq' + n" [(ngModel)]="i.quantity" placeholder="Quantità (es. 1 tazza)" />
          <input [name]="'in' + n" [(ngModel)]="i.name" placeholder="Ingrediente" />
          <button type="button" class="btn small" (click)="m.ingredients.splice(n, 1)" aria-label="Rimuovi">✕</button>
        </div>
      }
      <button type="button" class="btn small" (click)="m.ingredients.push({ name: '', quantity: '' })">+ Ingrediente</button>

      <h3>Preparazione</h3>
      @for (s of m.steps; track $index; let n = $index) {
        <div class="row">
          <textarea [name]="'s' + n" rows="2" [ngModel]="s" (ngModelChange)="m.steps[n] = $event" [placeholder]="'Passo ' + (n + 1)"></textarea>
          <button type="button" class="btn small" (click)="m.steps.splice(n, 1)" aria-label="Rimuovi">✕</button>
        </div>
      }
      <button type="button" class="btn small" (click)="m.steps.push('')">+ Passo</button>

      <h3>Foto e video</h3>
      @for (x of existing(); track x.id) {
        <div class="row">
          @if (x.kind === 'Photo') { <img [src]="x.url" alt="" height="60" /> } @else { <span>🎬 {{ x.url }}</span> }
          <button type="button" class="btn small danger" (click)="removeMedia(x)">Rimuovi</button>
        </div>
      }
      <label for="files">Aggiungi foto o video (file)</label>
      <input id="files" type="file" multiple accept="image/*,video/mp4,video/webm,video/quicktime" (change)="pick($event)" />
      <label for="vurl">Oppure link a un video (es. YouTube)</label>
      <input id="vurl" name="vurl" type="url" [(ngModel)]="videoUrl" placeholder="https://www.youtube.com/watch?v=…" />

      @if (error()) { <p class="error">{{ error() }}</p> }
      <div class="row" style="margin-top:1rem">
        <button class="btn primary" type="submit" [disabled]="saving() || f.invalid">{{ saving() ? 'Salvataggio…' : 'Salva' }}</button>
        <a class="btn" [routerLink]="id() ? ['/ricette', id()] : ['/']">Annulla</a>
      </div>
    </form>
  `,
})
export class RecipeFormPage implements OnInit {
  readonly id = input<string>();
  private readonly service = inject(RecipeService);
  private readonly router = inject(Router);
  protected readonly functions = COOKER_FUNCTIONS;
  protected readonly existing = signal<Media[]>([]);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected files: File[] = [];
  protected videoUrl = '';
  protected m: FormModel = {
    title: '', description: '', servings: 2, riceCups: 1, cookingTimeMinutes: 20,
    cookerFunction: 'WhiteRice', usesSteamer: false,
    ingredients: [{ name: '', quantity: '' }], steps: [''], tags: '',
  };

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;
    this.service.get(id).subscribe({
      next: (r) => {
        this.m = {
          title: r.title, description: r.description ?? '', servings: r.servings, riceCups: r.riceCups,
          cookingTimeMinutes: r.cookingTimeMinutes, cookerFunction: r.cookerFunction, usesSteamer: r.usesSteamer,
          ingredients: r.ingredients.map((i) => ({ name: i.name, quantity: i.quantity ?? '' })),
          steps: [...r.steps], tags: r.tags.join(', '),
        };
        this.existing.set(r.media);
      },
      error: () => this.error.set('Ricetta non trovata.'),
    });
  }

  protected pick(event: Event): void {
    this.files = Array.from((event.target as HTMLInputElement).files ?? []);
  }

  protected removeMedia(x: Media): void {
    const id = this.id();
    if (!id) return;
    this.service.deleteMedia(id, x.id).subscribe(() => this.existing.update((l) => l.filter((y) => y.id !== x.id)));
  }

  protected save(): void {
    this.saving.set(true);
    this.error.set('');
    const body = {
      title: this.m.title, description: this.m.description || null, servings: this.m.servings,
      riceCups: this.m.riceCups, cookingTimeMinutes: this.m.cookingTimeMinutes,
      cookerFunction: this.m.cookerFunction, usesSteamer: this.m.usesSteamer,
      ingredients: this.m.ingredients.filter((i) => i.name.trim()).map((i) => ({ name: i.name, quantity: i.quantity || null })),
      steps: this.m.steps.filter((s) => s.trim()),
      tags: this.m.tags.split(',').map((t) => t.trim()).filter(Boolean),
    };
    const id = this.id();
    (id ? this.service.update(id, body) : this.service.create(body)).subscribe({
      next: (r) => {
        const uploads: Observable<unknown>[] = this.files.map((f) => this.service.uploadMedia(r.id, f));
        if (this.videoUrl.trim()) uploads.push(this.service.addVideoUrl(r.id, this.videoUrl.trim()));
        (uploads.length ? forkJoin(uploads) : of<unknown>(null)).subscribe({
          next: () => this.router.navigate(['/ricette', r.id]),
          error: () => {
            this.saving.set(false);
            this.error.set('Ricetta salvata, ma il caricamento di alcuni media non è riuscito.');
            this.router.navigate(['/ricette', r.id, 'modifica']);
          },
        });
      },
      error: () => { this.saving.set(false); this.error.set('Salvataggio non riuscito: controlla i campi.'); },
    });
  }
}
