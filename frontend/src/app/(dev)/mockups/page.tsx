import { redirect } from 'next/navigation';
import { assertDevOnly } from './dev-only';

export default function MockupsIndex() {
  assertDevOnly();
  redirect('/mockups/home');
}
