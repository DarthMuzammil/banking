/**
 * Design tokens — Notion-inspired banking workspace
 *
 * Typography scale: 11 / 12 / 13 / 14 / 16 / 20 / 24 / 32 / 40 / 48
 * Spacing scale:    4 / 8 / 12 / 16 / 20 / 24 / 32 / 40 / 48 / 64
 * Motion:           150ms–250ms ease
 */

export const typography = {
  caption: "text-[11px] leading-4 tracking-wide",
  small: "text-xs leading-4",
  body: "text-sm leading-5",
  bodyLarge: "text-base leading-6",
  subtitle: "text-lg leading-7 font-medium",
  title: "text-xl leading-7 font-semibold tracking-tight",
  heading: "text-2xl leading-8 font-semibold tracking-tight",
  display: "text-[32px] leading-9 font-semibold tracking-tight",
  balance: "text-[40px] leading-none font-semibold tracking-tight tabular-nums",
} as const;

export const spacing = {
  page: "px-6 py-8 md:px-10 md:py-10",
  section: "space-y-6",
  stack: "space-y-4",
  inline: "gap-3",
} as const;

export const motion = {
  fast: "duration-150 ease-out",
  base: "duration-200 ease-out",
  slow: "duration-250 ease-out",
} as const;
