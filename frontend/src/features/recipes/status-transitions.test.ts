import { readFileSync } from 'node:fs';
import { join } from 'node:path';
import { allowedRecipeActions, canTransition, RECIPE_STATUS_TRANSITIONS } from './status-transitions';

/** Đọc thẳng bảng của Backend: đổi luật ở `RecipeStatusTransitions.cs` mà quên Frontend là test đỏ. */
function backendTransitions() {
  const source = readFileSync(
    join(process.cwd(), '../backend/src/CulinaryBlog.Domain/Entities/RecipeStatusTransitions.cs'),
    'utf8',
  );
  const pairs = [
    ...source.matchAll(/\[\(RecipeStatus\.(\w+), RecipeAction\.(\w+)\)\] = RecipeStatus\.(\w+)/g),
  ];
  return pairs.map(([, from, action, to]) => `${from}.${action.toLowerCase()}=${to}`).sort();
}

describe('Recipe status transition contract', () => {
  it('matches every transition declared in RecipeStatusTransitions.cs', () => {
    const frontend = Object.entries(RECIPE_STATUS_TRANSITIONS)
      .flatMap(([from, actions]) => Object.entries(actions).map(([action, to]) => `${from}.${action}=${to}`))
      .sort();
    expect(backendTransitions()).toHaveLength(5);
    expect(frontend).toEqual(backendTransitions());
  });

  it('only exposes valid state actions plus soft delete', () => {
    expect(allowedRecipeActions('Draft')).toEqual(['publish', 'archive', 'delete']);
    expect(canTransition('Archived', 'publish')).toBe(false);
    expect(canTransition('Archived', 'unarchive')).toBe(true);
  });
});
