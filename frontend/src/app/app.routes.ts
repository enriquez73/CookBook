import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/app-recipe-list/app-recipe-list').then((m) => m.AppRecipeList),
    title: 'Ricette',
  },
  {
    path: 'ricette/nuova',
    loadComponent: () => import('./pages/recipe-form/recipe-form').then((m) => m.RecipeForm),
    title: 'Nuova ricetta',
  },
  {
    path: 'ricette/:id/modifica',
    loadComponent: () => import('./pages/recipe-form/recipe-form').then((m) => m.RecipeForm),
    title: 'Modifica ricetta',
  },
  {
    path: 'ricette/:id',
    loadComponent: () => import('./pages/recipe-detail/recipe-detail').then((m) => m.RecipeDetailPage),
    title: 'Ricetta',
  },
  { path: '**', redirectTo: '' },
];
