import { getImageProps } from 'next/image';
import { createHash } from 'node:crypto';
import { readFileSync } from 'node:fs';
import { buildContentSecurityPolicy, getCspMode, IMAGE_STYLE_ATTRIBUTE_HASHES, LIBRARY_STYLE_HASHES } from './csp';

const findDirective = (policy: string, name: string) =>
  policy
    .split(';')
    .map((part) => part.trim())
    .find((part) => part.startsWith(`${name} `))!;

const sha256 = (text: string) =>
  `'sha256-${createHash('sha256').update(text, 'utf8').digest('base64')}'`;

/** Khối CSS Sonner chèn bằng `__insertCSS("…")` — đọc từ chính bản cài trong node_modules. */
function sonnerInjectedCss(): string {
  const source = readFileSync(require.resolve('sonner'), 'utf8');
  const start = source.indexOf('__insertCSS("') + '__insertCSS('.length;
  let end = start + 1;
  while (source[end] !== '"') end += source[end] === '\\' ? 2 : 1;
  return JSON.parse(source.slice(start, end + 1)) as string;
}

describe('content security policy', () => {
  it('uses nonce and strict-dynamic without unsafe script directives in production', () => {
    const policy = buildContentSecurityPolicy('request-nonce', false);
    const scriptSource = policy
      .split(';')
      .find((directive) => directive.trim().startsWith('script-src'))!;
    expect(scriptSource.trim()).toBe(
      "script-src 'self' 'nonce-request-nonce' 'strict-dynamic' https://accounts.google.com/gsi/client",
    );
    expect(scriptSource).not.toContain("'unsafe-inline'");
    expect(scriptSource).not.toContain("'unsafe-eval'");
    expect(policy).toContain("style-src 'self'");
    expect(policy).not.toContain("style-src 'self' 'unsafe-inline'");
    expect(policy).toContain("form-action 'self'");
    expect(policy).toContain("frame-ancestors 'none'");
    expect(policy).toContain('upgrade-insecure-requests');
    expect(policy).toContain("style-src 'self' 'sha256-StEaX+se6YS7pqjzrzMIA0KaX9zF/8zAhvQXZAe5epY=' 'sha256-47DEQpj8HBSa+/TImW+5JCeuQeRkm5NMpJWZG3hSuFU='");
    expect(policy).not.toContain("style-src 'self' 'unsafe-inline'");
    expect(policy).not.toContain('fonts.googleapis.com');
    expect(policy).not.toContain('fonts.gstatic.com');
  });

  it('production style-src: nonce of the request + hashes of library styles, never unsafe-inline', () => {
    const styleSource = findDirective(
      buildContentSecurityPolicy('request-nonce', false),
      'style-src',
    );
    expect(styleSource).toBe(
      `style-src 'self' 'nonce-request-nonce' ${[...LIBRARY_STYLE_HASHES, ...IMAGE_STYLE_ATTRIBUTE_HASHES].join(' ')}`,
    );
    expect(styleSource).not.toContain("'unsafe-inline'");
  });

  it('the Sonner hashes match the CSS of the installed sonner version (bump sonner → update LIBRARY_STYLE_HASHES)', () => {
    const css = sonnerInjectedCss();
    expect(css).toContain('[data-sonner-toaster]');
    expect(LIBRARY_STYLE_HASHES).toEqual([sha256(''), sha256(css)]);
  });

  it('allows exactly the style attributes next/image renders, by hash', () => {
    const css = (style: object) =>
      Object.entries(style)
        .filter(([, value]) => value !== undefined)
        .map(([key, value]) => `${key.replace(/[A-Z]/g, (c) => `-${c.toLowerCase()}`)}:${value}`)
        .join(';');
    const hash = (text: string) => `'sha256-${createHash('sha256').update(text).digest('base64')}'`;
    const fill = getImageProps({ src: '/a.jpg', alt: '', fill: true, unoptimized: true }).props.style;
    const sized = getImageProps({ src: '/a.jpg', alt: '', width: 1, height: 1, unoptimized: true }).props.style;

    expect(IMAGE_STYLE_ATTRIBUTE_HASHES).toContain(hash(css(fill!)));
    expect(IMAGE_STYLE_ATTRIBUTE_HASHES).toContain(hash(css(sized!)));
    expect(buildContentSecurityPolicy('nonce', false)).not.toContain("'unsafe-inline'");
  });

  it('development style-src stays unsafe-inline without a nonce (a nonce would make browsers ignore unsafe-inline)', () => {
    expect(findDirective(buildContentSecurityPolicy('nonce', true), 'style-src')).toBe(
      "style-src 'self' 'unsafe-inline'",
    );
  });

  it('only permits unsafe-eval for development and supports report-only rollout mode', () => {
    expect(buildContentSecurityPolicy('nonce', true)).toContain("'unsafe-eval'");
    expect(buildContentSecurityPolicy('nonce', true)).toContain("style-src 'self' 'unsafe-inline'");
    expect(buildContentSecurityPolicy('nonce', true)).not.toContain('upgrade-insecure-requests');
    expect(getCspMode('report-only')).toBe('report-only');
    expect(getCspMode('other')).toBe('enforce');
  });
});
