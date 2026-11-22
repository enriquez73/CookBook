import { ChangeDetectionStrategy, Component, inject, input, OnInit, signal } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { Router, RouterLink } from '@angular/router';
import { Media, RecipeDetail, applianceLabel, functionLabel } from '../../core/models';
import { RecipeService } from '../../core/recipe.service';
import { ButtonModule } from 'primeng/button';
import { ScrollTopModule } from 'primeng/scrolltop';

@Component({
  imports: [RouterLink, ButtonModule, ScrollTopModule],
  selector: 'app-recipe-detail',
  styleUrl: './recipe-detail.css',
  templateUrl: './recipe-detail.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
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

  protected photos(): Media[] {
    return this.recipe()?.media.filter((m) => m.kind === 'Photo') ?? [];
  }
  protected videos(): Media[] {
    return this.recipe()?.media.filter((m) => m.kind !== 'Photo') ?? [];
  }

  protected applianceNames(recipe: RecipeDetail): string {
    const values = recipe.cookingAppliances?.length
      ? recipe.cookingAppliances
      : recipe.cookingAppliance
        ? [recipe.cookingAppliance]
        : [];
    return values.map(applianceLabel).join(' + ');
  }

  protected embed(url: string): SafeResourceUrl | null {
    const yt = /(?:youtube\.com\/watch\?v=|youtu\.be\/|youtube\.com\/embed\/)([\w-]{6,})/.exec(url);
    if (yt)
      return this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://www.youtube.com/embed/${yt[1]}`,
      );
    const vimeo = /vimeo\.com\/(\d+)/.exec(url);
    if (vimeo)
      return this.sanitizer.bypassSecurityTrustResourceUrl(
        `https://player.vimeo.com/video/${vimeo[1]}`,
      );
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
