import { Routes } from '@angular/router';
import { RecipeFormPage } from './pages/recipe-form';
import { RecipeDetailPage } from './pages/recipe-detail';
import { RecipeListPage } from './pages/recipe-list';

export const routes: Routes = [
  { path: '', component: RecipeListPage, title: 'Ricette' },
  { path: 'ricette/nuova', component: RecipeFormPage, title: 'Nuova ricetta' },
  { path: 'ricette/:id/modifica', component: RecipeFormPage, title: 'Modifica ricetta' },
  { path: 'ricette/:id', component: RecipeDetailPage, title: 'Ricetta' },
  { path: '**', redirectTo: '' },
];
