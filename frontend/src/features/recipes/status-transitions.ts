import type { RecipeStatus, RecipeStatusAction } from '@/types/recipe';

/** Mirrors `RecipeStatusTransitions.cs`; lifecycle controls must be derived from this table. */
export const RECIPE_STATUS_TRANSITIONS = {
  Draft: { publish: 'Published', archive: 'Archived' },
  Published: { unpublish: 'Draft', archive: 'Archived' },
  Archived: { unarchive: 'Draft' },
} as const satisfies Record<RecipeStatus, Partial<Record<Exclude<RecipeStatusAction, 'delete'>, RecipeStatus>>>;

export function allowedRecipeActions(status: RecipeStatus): RecipeStatusAction[] {
  return [...Object.keys(RECIPE_STATUS_TRANSITIONS[status]), 'delete'] as RecipeStatusAction[];
}

export function canTransition(status: RecipeStatus, action: RecipeStatusAction): boolean {
  return action === 'delete' || action in RECIPE_STATUS_TRANSITIONS[status];
}
