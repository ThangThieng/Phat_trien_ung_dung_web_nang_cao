'use client';

import { useEffect, useState } from 'react';
import type { RecipeStep } from '@/types/recipe';

/** Các bước thực hiện dạng checklist – người đọc tick bước đã làm khi nấu. */
export default function StepChecklist({ steps }: { steps: RecipeStep[] }) {
  const [done, setDone] = useState<Set<string>>(new Set());
  const [activeTimer, setActiveTimer] = useState<{ id: string; remaining: number } | null>(null);

  useEffect(() => {
    if (!activeTimer) return undefined;
    const interval = window.setInterval(
      () =>
        setActiveTimer((current) => {
          if (!current) return null;
          if (current.remaining <= 1) {
            import('sonner').then(({ toast }) => toast.success('Hẹn giờ đã kết thúc.'));
            return null;
          }
          return { ...current, remaining: current.remaining - 1 };
        }),
      1000,
    );
    return () => window.clearInterval(interval);
  }, [activeTimer]);

  const toggle = (id: string) =>
    setDone((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  return (
    <div>
      <p className="mb-3 text-sm text-ink-soft" aria-live="polite">
        Đã hoàn thành {done.size}/{steps.length} bước
      </p>
      <ol className="flex flex-col gap-4">
        {steps.map((step) => {
          const checked = done.has(step.id);
          return (
            <li
              id={`buoc-${step.stepNumber}`}
              key={step.id}
              className={`rounded-xl border p-4 transition ${checked ? 'border-green-200 bg-green-50' : 'border-line bg-paper'}`}
            >
              <label htmlFor={`step-${step.id}`} className="flex cursor-pointer items-start gap-3">
                <input
                  id={`step-${step.id}`}
                  type="checkbox"
                  checked={checked}
                  onChange={() => toggle(step.id)}
                  className="mt-1 h-5 w-5 shrink-0 accent-navy"
                />
                <span className="flex-1">
                  <span className="flex flex-wrap items-center gap-2">
                    <span className="text-sm font-semibold text-gold-ink">
                      Bước {step.stepNumber}
                    </span>
                    <span
                      className={`font-semibold text-ink ${checked ? 'line-through opacity-70' : ''}`}
                    >
                      {step.title}
                    </span>
                    {step.timerMinutes !== null && (
                      <button
                        type="button"
                        onClick={() =>
                          setActiveTimer({ id: step.id, remaining: (step.timerMinutes ?? 0) * 60 })
                        }
                        className="flex items-center gap-1 rounded-full bg-gold-soft px-2 py-0.5 text-xs text-gold-ink hover:bg-line"
                        aria-label={`Hẹn giờ ${step.timerMinutes} phút cho bước ${step.stepNumber}`}
                      >
                        <span aria-hidden>⏱</span>{' '}
                        {activeTimer?.id === step.id
                          ? `${Math.floor(activeTimer.remaining / 60)}:${String(activeTimer.remaining % 60).padStart(2, '0')}`
                          : `${step.timerMinutes} phút`}
                      </button>
                    )}
                  </span>
                  <span
                    className={`mt-1 block text-sm leading-relaxed text-ink ${checked ? 'opacity-70' : ''}`}
                  >
                    {step.description}
                  </span>
                </span>
              </label>
              {step.imageUrl && (
                // eslint-disable-next-line @next/next/no-img-element -- ảnh minh họa bước từ MinIO
                <img
                  src={step.imageUrl}
                  alt={`Minh họa bước ${step.stepNumber}`}
                  className="mt-3 max-h-64 rounded-lg object-cover"
                />
              )}
            </li>
          );
        })}
      </ol>
    </div>
  );
}
