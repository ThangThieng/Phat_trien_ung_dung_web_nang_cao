import { forwardRef, useId } from 'react';
import type { InputHTMLAttributes } from 'react';

interface FormFieldProps extends InputHTMLAttributes<HTMLInputElement> {
  label: string;
  error?: string;
  hint?: string;
}

/** Input có label + thông báo lỗi inline (NFR-USE-003), liên kết aria-describedby cho screen reader. */
const FormField = forwardRef<HTMLInputElement, FormFieldProps>(
  ({ label, error, hint, id, className, ...props }, ref) => {
    const generatedId = useId();
    const inputId = id ?? generatedId;
    let describedBy: string | undefined;
    if (error) describedBy = `${inputId}-error`;
    else if (hint) describedBy = `${inputId}-hint`;

    return (
      <div className="flex flex-col gap-1">
        <label htmlFor={inputId} className="text-sm font-medium text-gray-800">
          {label}
        </label>
        <input
          ref={ref}
          id={inputId}
          aria-invalid={error ? true : undefined}
          aria-describedby={describedBy}
          className={`rounded-lg border px-3 py-2 text-sm shadow-sm transition outline-none focus:ring-2 focus:ring-orange-500 ${
            error ? 'border-red-500' : 'border-gray-300'
          } ${className ?? ''}`}
          {...props}
        />
        {error && (
          <p id={`${inputId}-error`} role="alert" className="text-sm text-red-600">
            {error}
          </p>
        )}
        {!error && hint && (
          <p id={`${inputId}-hint`} className="text-xs text-gray-500">
            {hint}
          </p>
        )}
      </div>
    );
  },
);

FormField.displayName = 'FormField';

export default FormField;
