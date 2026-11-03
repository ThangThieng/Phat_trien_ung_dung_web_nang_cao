import { allowedRecipeActions, canTransition, RECIPE_STATUS_TRANSITIONS } from './status-transitions';

describe('Recipe status transition contract', () => {
  it('mirrors the five transitions in RecipeStatusTransitions.cs', () => {
    expect(RECIPE_STATUS_TRANSITIONS).toEqual({
      Draft: { publish: 'Published', archive: 'Archived' },
      Published: { unpublish: 'Draft', archive: 'Archived' },
      Archived: { unarchive: 'Draft' },
    });
  });

  it('only exposes valid state actions plus soft delete', () => {
    expect(allowedRecipeActions('Draft')).toEqual(['publish', 'archive', 'delete']);
    expect(canTransition('Archived', 'publish')).toBe(false);
    expect(canTransition('Archived', 'unarchive')).toBe(true);
  });
});
