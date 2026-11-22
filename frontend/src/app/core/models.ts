export type CookerFunction =
  | 'WhiteRice' | 'BrownRice' | 'Porridge' | 'Soup' | 'Steam' | 'Cake' | 'KeepWarm' | 'Other';

export type CookingAppliance = 'RiceCooker' | 'AirFryer' | 'Stovetop';

export const COOKING_APPLIANCES: { value: CookingAppliance; label: string }[] = [
  { value: 'RiceCooker', label: 'Cuociriso' },
  { value: 'AirFryer', label: 'Friggitrice ad aria' },
  { value: 'Stovetop', label: 'Fornello / pentola / padella' },
];

export const applianceLabel = (v: CookingAppliance): string =>
  COOKING_APPLIANCES.find((a) => a.value === v)?.label ?? v;

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

export const functionLabel = (v: CookerFunction | null): string | null =>
  v ? COOKER_FUNCTIONS.find((f) => f.value === v)?.label ?? v : null;

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
  cookerFunction: CookerFunction | null;
  cookingAppliances?: CookingAppliance[];
  /** Campo singolo mantenuto dal backend per compatibilità con i client precedenti. */
  cookingAppliance?: CookingAppliance;
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
  cookerFunction: CookerFunction | null;
  cookingAppliances?: CookingAppliance[];
  /** Campo singolo mantenuto dal backend per compatibilità con i client precedenti. */
  cookingAppliance?: CookingAppliance;
  usesSteamer: boolean;
  ingredients: Ingredient[];
  steps: string[];
  tags: string[];
  media: Media[];
  createdAt: string;
  updatedAt: string;
}

export interface RecipeRequest {
  title: string;
  description: string | null;
  servings: number;
  riceCups: number;
  cookingTimeMinutes: number;
  cookerFunction: CookerFunction | null;
  cookingAppliances: CookingAppliance[];
  usesSteamer: boolean;
  ingredients: Ingredient[];
  steps: string[];
  tags: string[];
}

export interface RecipeFilter {
  search?: string;
  cookingAppliance?: CookingAppliance | '';
  cookerFunction?: string;
  tag?: string;
  maxMinutes?: number | null;
  servings?: number | null;
}
