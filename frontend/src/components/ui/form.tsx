'use client';

/* eslint-disable react/jsx-no-constructed-context-values, @typescript-eslint/no-use-before-define, react/destructuring-assignment */

import * as React from 'react';
import { Controller, FormProvider, useFormContext } from 'react-hook-form';
import type { ControllerProps, FieldPath, FieldValues } from 'react-hook-form';
import { Label } from '@/components/ui/label';
import { cn } from '@/lib/utils';

const Form = FormProvider;
const FormField = Controller;
const FormItemContext = React.createContext<{ id: string } | undefined>(undefined);

function FormItem({ className, ...props }: React.ComponentProps<'div'>) {
  const id = React.useId();
  return <FormItemContext.Provider value={{ id }}><div className={cn('space-y-2', className)} {...props} /></FormItemContext.Provider>;
}

function useFormField() {
  const context = React.useContext(FormItemContext);
  const { getFieldState, formState } = useFormContext();
  if (!context) throw new Error('useFormField must be used within FormItem');
  const fieldState = getFieldState((React.useContext(FormFieldNameContext) ?? '') as never, formState);
  return { ...context, error: fieldState.error as { message?: string } | undefined };
}

const FormFieldNameContext = React.createContext<string | undefined>(undefined);

function FormLabel({ className, ...props }: React.ComponentProps<typeof Label>) {
  const { id, error } = useFormField();
  return <Label className={cn(error && 'text-destructive', className)} htmlFor={`${id}-form-item`} {...props} />;
}

function FormControl({ children }: { children: React.ReactElement<Record<string, unknown>> }) {
  const { id, error } = useFormField();
  return React.cloneElement(children, {
    id: `${id}-form-item`,
    'aria-invalid': Boolean(error),
    'aria-describedby': error ? `${id}-form-item-message` : undefined,
  });
}

function FormMessage({ className, children, ...props }: React.ComponentProps<'p'>) {
  const { id, error } = useFormField();
  const body = error?.message ?? children;
  return body ? <p id={`${id}-form-item-message`} className={cn('text-sm text-destructive', className)} {...props}>{body}</p> : null;
}

function NamedFormField<TFieldValues extends FieldValues, TName extends FieldPath<TFieldValues>>(
  props: ControllerProps<TFieldValues, TName>,
) {
  return <FormFieldNameContext.Provider value={String(props.name)}><FormField {...props} /></FormFieldNameContext.Provider>;
}

export { Form, NamedFormField as FormField, FormItem, FormLabel, FormControl, FormMessage };
