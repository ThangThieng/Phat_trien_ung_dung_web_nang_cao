import { Badge } from '@/components/ui/badge';
import type { RecipeStatus } from '@/types/recipe';

const VARIANTS = { Draft: 'secondary', Published: 'default', Archived: 'outline' } as const;

export function RecipeStatusBadge({ status }: { status: RecipeStatus }) {
  return <Badge variant={VARIANTS[status]}>{status}</Badge>;
}
