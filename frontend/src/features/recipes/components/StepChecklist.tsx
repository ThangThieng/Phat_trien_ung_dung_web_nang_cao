'use client';

import { useState } from 'react';
import { Timer } from 'lucide-react';
import type { RecipeStep } from '@/types/api';

/** Các bước thực hiện dạng checklist – người đọc tick bước đã làm khi nấu. */
export default function StepChecklist({ steps }: { steps: RecipeStep[] }) {
  const [done, setDone] = useState<Set<string>>(new Set());

  const toggle = (id: string) =>
    setDone((current) => {
      const next = new Set(current);
      if (next.has(id)) next.delete(id);
      else next.add(id);
      return next;
    });

  return (
    <div>
      <p className="mb-3 text-sm text-gray-600" aria-live="polite">
        Đã hoàn thành {done.size}/{steps.length} bước
      </p>
      <ol className="flex flex-col gap-4">
        {steps.map((step) => {
          const checked = done.has(step.id);
          return (
            <li
              key={step.id}
              className={`rounded-xl border p-4 transition ${checked ? 'border-green-200 bg-green-50' : 'border-gray-200 bg-white'}`}
            >
              <label htmlFor={`step-${step.id}`} className="flex cursor-pointer items-start gap-3">
                <input
                  id={`step-${step.id}`}
                  type="checkbox"
                  checked={checked}
                  onChange={() => toggle(step.id)}
                  className="mt-1 h-5 w-5 shrink-0 accent-orange-600"
                />
                <span className="flex-1">
                  <span className="flex flex-wrap items-center gap-2">
                    <span className="text-sm font-semibold text-orange-700">
                      Bước {step.stepNumber}
                    </span>
                    <span
                      className={`font-semibold text-gray-900 ${checked ? 'line-through opacity-70' : ''}`}
                    >
                      {step.title}
                    </span>
                    {step.timerMinutes !== null && (
                      <span className="flex items-center gap-1 rounded-full bg-orange-100 px-2 py-0.5 text-xs text-orange-800">
                        <Timer aria-hidden className="h-3 w-3" /> {step.timerMinutes} phút
                      </span>
                    )}
                  </span>
                  <span
                    className={`mt-1 block text-sm leading-relaxed text-gray-700 ${checked ? 'opacity-70' : ''}`}
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
