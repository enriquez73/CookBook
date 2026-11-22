import { ChangeDetectionStrategy, ChangeDetectorRef, Component, OnInit, inject, input, signal } from '@angular/core';
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
  steps: StepDraft[];
  tags: string;
}

interface StepDraft {
  id: string | null;
  text: string;
  savedImageUrl: string | null;
  imageUrlDraft: string;
  imageFile: File | null;
  imagePreviewUrl: string | null;
  removeImage: boolean;
}

const emptyStep = (): StepDraft => ({
  id: null,
  text: '',
  savedImageUrl: null,
  imageUrlDraft: '',
  imageFile: null,
  imagePreviewUrl: null,
  removeImage: false,
});

@Component({
  imports: [FormsModule, RouterLink, ButtonModule, InputText, Select, Textarea],
  selector: 'app-recipe-form',
  styleUrl: './recipe-form.css',
  templateUrl: './recipe-form.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class RecipeForm implements OnInit {
  readonly id = input<string>();
  private readonly service = inject(RecipeService);
  private readonly router = inject(Router);
  private readonly changeDetector = inject(ChangeDetectorRef);
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
    steps: [emptyStep()],
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
          steps: r.stepDetails.map((step) => ({
            id: step.id,
            text: step.text,
            savedImageUrl: step.imageUrl,
            imageUrlDraft: '',
            imageFile: null,
            imagePreviewUrl: null,
            removeImage: false,
          })),
          tags: r.tags.join(', '),
        };
        this.existing.set(r.media);
        this.changeDetector.markForCheck();
      },
      error: () => this.error.set('Ricetta non trovata.'),
    });
  }

  protected pick(event: Event): void {
    this.files = Array.from((event.target as HTMLInputElement).files ?? []);
  }

  protected pickStepImage(step: StepDraft, event: Event): void {
    const file = (event.target as HTMLInputElement).files?.[0] ?? null;
    if (!file) return;
    this.revokePreview(step);
    step.imageFile = file;
    step.imageUrlDraft = '';
    step.imagePreviewUrl = URL.createObjectURL(file);
    step.removeImage = false;
  }

  protected clearStepFile(step: StepDraft): void {
    if (!step.imageUrlDraft.trim()) return;
    this.revokePreview(step);
    step.imageFile = null;
    step.removeImage = false;
  }

  protected removeStepImage(step: StepDraft): void {
    this.revokePreview(step);
    step.imageFile = null;
    step.imageUrlDraft = '';
    step.removeImage = !!step.id && !!step.savedImageUrl;
    if (!step.removeImage) step.savedImageUrl = null;
  }

  private revokePreview(step: StepDraft): void {
    if (step.imagePreviewUrl) URL.revokeObjectURL(step.imagePreviewUrl);
    step.imagePreviewUrl = null;
  }

  protected removeStep(index: number): void {
    this.revokePreview(this.m.steps[index]);
    this.m.steps.splice(index, 1);
  }

  protected addStep(): void {
    this.m.steps.push(emptyStep());
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
    const activeSteps = this.m.steps.filter((step) => step.text.trim());
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
      steps: activeSteps.map((step) => step.text.trim()),
      stepIds: activeSteps.map((step) => step.id),
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
        activeSteps.forEach((step, index) => {
          const savedStep = r.stepDetails[index];
          if (!savedStep) return;
          if (step.removeImage && savedStep.imageUrl) {
            uploads.push(this.service.deleteStepImage(r.id, savedStep.id));
          } else if (step.imageFile) {
            uploads.push(this.service.uploadStepImage(r.id, savedStep.id, step.imageFile));
          } else if (step.imageUrlDraft.trim()) {
            uploads.push(this.service.addStepImageUrl(r.id, savedStep.id, step.imageUrlDraft.trim()));
          }
        });
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
