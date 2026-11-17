import { Badge } from '@/components/ui/badge';
import { RECIPE_STATUS_LABEL } from '@/features/recipes/format';
import type { RecipeStatus } from '@/types/recipe';

const VARIANTS = { Draft: 'secondary', Published: 'default', Archived: 'outline' } as const;

export function RecipeStatusBadge({ status }: { status: RecipeStatus }) {
  return <Badge variant={VARIANTS[status]}>{RECIPE_STATUS_LABEL[status]}</Badge>;
}
