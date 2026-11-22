import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  COOKER_FUNCTIONS,
  COOKING_APPLIANCES,
  RecipeFilter,
  RecipeSummary,
  applianceLabel,
  functionLabel,
} from '../../core/models';
import { RecipeService } from '../../core/recipe.service';
import { Card } from 'primeng/card';
import { InputText } from 'primeng/inputtext';
import { Select } from 'primeng/select';
import { Tag } from 'primeng/tag';
import { ButtonModule } from 'primeng/button';

@Component({
  imports: [FormsModule, RouterLink, Card, InputText, Select, Tag, ButtonModule],
  selector: 'app-recipe-list',
  styleUrl: './app-recipe-list.css',
  templateUrl: './app-recipe-list.html',
})
export class AppRecipeList implements OnInit {
  private readonly service = inject(RecipeService);
  protected readonly functions = COOKER_FUNCTIONS;
  protected readonly appliances = COOKING_APPLIANCES;
  protected readonly label = functionLabel;
  protected readonly recipes = signal<RecipeSummary[]>([]);
  protected readonly tags = signal<string[]>([]);
  protected readonly tagOptions = computed(() => [
    { label: 'Tutti i tag', value: '' },
    ...this.tags().map((tag) => ({ label: tag, value: tag })),
  ]);
  protected readonly durationOptions = [
    { label: 'Qualsiasi durata', value: null },
    ...[15, 30, 45, 60].map((minutes) => ({ label: `Max ${minutes} min`, value: minutes })),
  ];
  protected readonly loading = signal(false);
  protected readonly error = signal('');
  protected filter: RecipeFilter = {
    search: '',
    cookingAppliance: '',
    cookerFunction: '',
    tag: '',
    maxMinutes: null,
    servings: null,
  };

  protected applianceNames(recipe: RecipeSummary): string {
    const values = recipe.cookingAppliances?.length
      ? recipe.cookingAppliances
      : recipe.cookingAppliance
        ? [recipe.cookingAppliance]
        : [];
    return values.map(applianceLabel).join(' + ');
  }

  ngOnInit(): void {
    this.service.tags().subscribe({ next: (t) => this.tags.set(t) });
    this.load();
  }

  load(): void {
    this.loading.set(true);
    this.service.list(this.filter).subscribe({
      next: (r) => {
        this.recipes.set(r);
        this.loading.set(false);
        this.error.set('');
      },
      error: () => {
        this.loading.set(false);
        this.error.set('Impossibile caricare le ricette.');
      },
    });
  }
}
