import { Component, OnInit, inject, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { forkJoin, Observable, of } from 'rxjs';
import {
  COOKER_FUNCTIONS,
  COOKING_APPLIANCES,
  CookingAppliance,
  CookerFunction,
  Media,
  RecipeRequest,
} from '../../core/models';
import { RecipeService } from '../../core/recipe.service';
import { ButtonModule } from 'primeng/button';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Textarea } from 'primeng/textarea';

interface FormModel {
  title: string;
  description: string;
  servings: number;
  riceCups: number;
  cookingTimeMinutes: number;
  cookerFunction: CookerFunction | null;
  cookingAppliances: CookingAppliance[];
  usesSteamer: boolean;
  ingredients: { name: string; quantity: string }[];
  steps: string[];
  tags: string;
}

@Component({
  imports: [FormsModule, RouterLink, ButtonModule, InputText, Select, Textarea],
  selector: 'app-recipe-form',
  styleUrl: './recipe-form.css',
  templateUrl: './recipe-form.html',
})
export class RecipeForm implements OnInit {
  readonly id = input<string>();
  private readonly service = inject(RecipeService);
  private readonly router = inject(Router);
  protected readonly functions = COOKER_FUNCTIONS;
  protected readonly appliances = COOKING_APPLIANCES;
  protected readonly existing = signal<Media[]>([]);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected files: File[] = [];
  protected imageUrl = '';
  protected videoUrl = '';
  protected m: FormModel = {
    title: '',
    description: '',
    servings: 2,
    riceCups: 1,
    cookingTimeMinutes: 20,
    cookerFunction: 'WhiteRice',
    cookingAppliances: ['RiceCooker'],
    usesSteamer: false,
    ingredients: [{ name: '', quantity: '' }],
    steps: [''],
    tags: '',
  };

  ngOnInit(): void {
    const id = this.id();
    if (!id) return;
    this.service.get(id).subscribe({
      next: (r) => {
        this.m = {
          title: r.title,
          description: r.description ?? '',
          servings: r.servings,
          riceCups: r.riceCups,
          cookingTimeMinutes: r.cookingTimeMinutes,
          cookerFunction: r.cookerFunction,
          cookingAppliances: r.cookingAppliances?.length
            ? [...r.cookingAppliances]
            : [r.cookingAppliance ?? 'RiceCooker'],
          usesSteamer: r.usesSteamer,
          ingredients: r.ingredients.map((i) => ({ name: i.name, quantity: i.quantity ?? '' })),
          steps: [...r.steps],
          tags: r.tags.join(', '),
        };
        this.existing.set(r.media);
      },
      error: () => this.error.set('Ricetta non trovata.'),
    });
  }

  protected pick(event: Event): void {
    this.files = Array.from((event.target as HTMLInputElement).files ?? []);
  }

  protected usesRiceCooker(): boolean {
    return this.m.cookingAppliances.includes('RiceCooker');
  }

  protected toggleAppliance(appliance: CookingAppliance, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    this.m.cookingAppliances = checked
      ? [...new Set([...this.m.cookingAppliances, appliance])]
      : this.m.cookingAppliances.filter((a) => a !== appliance);
    if (this.usesRiceCooker()) this.m.cookerFunction ??= 'WhiteRice';
    else {
      this.m.cookerFunction = null;
      this.m.usesSteamer = false;
    }
  }

  protected removeMedia(x: Media): void {
    const id = this.id();
    if (!id) return;
    this.service
      .deleteMedia(id, x.id)
      .subscribe(() => this.existing.update((l) => l.filter((y) => y.id !== x.id)));
  }

  protected save(): void {
    this.saving.set(true);
    this.error.set('');
    const body: RecipeRequest = {
      title: this.m.title,
      description: this.m.description || null,
      servings: this.m.servings,
      riceCups: this.m.riceCups,
      cookingTimeMinutes: this.m.cookingTimeMinutes,
      cookerFunction: this.usesRiceCooker() ? (this.m.cookerFunction ?? 'WhiteRice') : null,
      cookingAppliances: this.m.cookingAppliances,
      usesSteamer: this.usesRiceCooker() && this.m.usesSteamer,
      ingredients: this.m.ingredients
        .filter((i) => i.name.trim())
        .map((i) => ({ name: i.name, quantity: i.quantity || null })),
      steps: this.m.steps.filter((s) => s.trim()),
      tags: this.m.tags
        .split(',')
        .map((t) => t.trim())
        .filter(Boolean),
    };
    const id = this.id();
    (id ? this.service.update(id, body) : this.service.create(body)).subscribe({
      next: (r) => {
        const uploads: Observable<unknown>[] = this.files.map((f) =>
          this.service.uploadMedia(r.id, f),
        );
        if (this.imageUrl.trim())
          uploads.push(this.service.addImageUrl(r.id, this.imageUrl.trim()));
        if (this.videoUrl.trim())
          uploads.push(this.service.addVideoUrl(r.id, this.videoUrl.trim()));
        (uploads.length ? forkJoin(uploads) : of<unknown>(null)).subscribe({
          next: () => this.router.navigate(['/ricette', r.id]),
          error: () => {
            this.saving.set(false);
            this.error.set('Ricetta salvata, ma il caricamento di alcuni media non è riuscito.');
            this.router.navigate(['/ricette', r.id, 'modifica']);
          },
        });
      },
      error: () => {
        this.saving.set(false);
        this.error.set('Salvataggio non riuscito: controlla i campi.');
      },
    });
  }
}
