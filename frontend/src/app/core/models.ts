export type CookerFunction =
  | 'WhiteRice' | 'BrownRice' | 'Porridge' | 'Soup' | 'Steam' | 'Cake' | 'KeepWarm' | 'Other';

export const COOKER_FUNCTIONS: { value: CookerFunction; label: string }[] = [
  { value: 'WhiteRice', label: 'Riso bianco' },
  { value: 'BrownRice', label: 'Riso integrale' },
  { value: 'Porridge', label: 'Porridge / Congee' },
  { value: 'Soup', label: 'Zuppa' },
  { value: 'Steam', label: 'Vapore (cestello)' },
  { value: 'Cake', label: 'Dolce' },
  { value: 'KeepWarm', label: 'Mantenimento caldo' },
  { value: 'Other', label: 'Altro' },
];

export const functionLabel = (v: CookerFunction): string =>
  COOKER_FUNCTIONS.find((f) => f.value === v)?.label ?? v;

export interface Ingredient { name: string; quantity: string | null; }

export interface Media {
  id: string;
  kind: 'Photo' | 'VideoFile' | 'VideoUrl';
  url: string;
  caption: string | null;
}

export interface RecipeSummary {
  id: string;
  title: string;
  description: string | null;
  servings: number;
  cookingTimeMinutes: number;
  cookerFunction: CookerFunction;
  usesSteamer: boolean;
  tags: string[];
  coverUrl: string | null;
  hasVideo: boolean;
}

export interface RecipeDetail {
  id: string;
  title: string;
  description: string | null;
  servings: number;
  riceCups: number;
  cookingTimeMinutes: number;
  cookerFunction: CookerFunction;
  usesSteamer: boolean;
  ingredients: Ingredient[];
  steps: string[];
  tags: string[];
  media: Media[];
  createdAt: string;
  updatedAt: string;
}

export type RecipeRequest = Omit<RecipeDetail, 'id' | 'media' | 'createdAt' | 'updatedAt'>;

export interface RecipeFilter {
  search?: string;
  cookerFunction?: string;
  tag?: string;
  maxMinutes?: number | null;
  servings?: number | null;
}
