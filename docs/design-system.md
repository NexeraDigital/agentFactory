# Design System Reference

<!-- ADAPT: Replace color values, font choices, and component patterns with your project's design tokens -->

This document defines the visual design system foundation. It is referenced by the modern-ui-agent and UI design rules to ensure consistency.

## Color Architecture

Build every interface on a **neutral-first palette** with strategic accent usage:

```
Foundation (90% of the UI):
├── Background:    #FAFAFA or #F8F9FA (not pure white)
├── Surface:       #FFFFFF (cards, panels)
├── Surface-alt:   #F1F3F5 (secondary surfaces, table headers)
├── Border:        #E9ECEF at 60-80% opacity (barely visible)
├── Border-strong: #DEE2E6 (section dividers only)
├── Text-primary:  #1A1A2E or #0F172A (near-black, not pure black)
├── Text-secondary:#64748B (descriptions, metadata)
└── Text-tertiary: #94A3B8 (timestamps, placeholders)

Accent (5-8% of the UI):
├── Primary:       <!-- ADAPT: One brand color, used SPARINGLY -->
├── Primary-subtle: Primary at 8-12% opacity (backgrounds, highlights)
├── Success:       #10B981 (confirmations, active states)
├── Warning:       #F59E0B (alerts, urgency)
├── Danger:        #EF4444 (errors, destructive actions)
└── Info:          #3B82F6 (informational states)
```

**Brand color usage** (define per project, then apply consistently):
- Active navigation indicator (left border or background tint, NOT the entire sidebar)
- Primary action buttons (ONE per view)
- Status accents for "in progress" states
- Logo/brand mark

### Design Tokens

Define all color values as CSS custom properties (e.g., `--color-text-primary`, `--color-surface`, `--color-border`) rather than hard-coding hex values. This enables runtime theming, dark mode switching, and white-labeling without touching component code. Map every value in the palettes above to a semantic token.

### Dark Mode Palette

```
Foundation (dark):
├── Background:    #0F1117 (deep near-black, not pure #000)
├── Surface:       #1A1D27 (cards, panels)
├── Surface-alt:   #242736 (secondary surfaces, table headers)
├── Border:        #2E3241 at 60-80% opacity
├── Border-strong: #3A3F50 (section dividers only)
├── Text-primary:  #E2E8F0 (off-white, not pure white)
├── Text-secondary:#94A3B8
└── Text-tertiary: #64748B

Accent (dark — same hues, adjusted lightness):
├── Primary:       <!-- ADAPT: Lighten brand color ~10-15% for dark backgrounds -->
├── Primary-subtle: Primary at 12-16% opacity
├── Success:       #34D399
├── Warning:       #FBBF24
├── Danger:        #F87171
└── Info:          #60A5FA
```

**Implementation**: Use `prefers-color-scheme: dark` media query with CSS custom properties. Toggle by swapping a `data-theme="dark"` attribute on `<html>` for user-overridable preference. Store the user's choice in `localStorage`.

### Surface Elevation

Layer surfaces to create depth hierarchy — especially important in dark mode where shadows are less visible:

```
Tier 0 — Base:      var(--color-background)     Page canvas
Tier 1 — Raised:    var(--color-surface)         Cards, sidebar
Tier 2 — Overlay:   var(--color-surface-alt)     Dropdowns, popovers
Tier 3 — Floating:  var(--color-surface-alt)     Modals, command palette, toasts
```

In dark mode, each tier should be slightly lighter than the previous. In light mode, use shadows to distinguish tiers.

## Typography Scale

Use a **systematic type scale** with clear hierarchy:

```
Display:    text-2xl  (24-28px) — Page titles only, semibold 600,  line-height 1.2
Heading:    text-lg   (18-20px) — Section headings, medium 500,   line-height 1.25
Subheading: text-base (15-16px) — Card titles, subsections, medium 500, line-height 1.3
Body:       text-sm   (13-14px) — Primary content, regular 400,   line-height 1.5
Caption:    text-xs   (11-12px) — Metadata, timestamps, labels, medium 500, line-height 1.4
Overline:   text-xs   (10-11px) — Category labels, ALL-CAPS with letter-spacing 0.05em, line-height 1.4
```

**Tabular numbers**: Use `font-variant-numeric: tabular-nums` on any element displaying numeric data (tables, metrics, dashboards, stat cards). This ensures digits are monospaced for clean column alignment.

**Font choices** (in order of preference for enterprise SaaS):
1. **Inter** — Excellent clarity at small sizes, ideal for data-heavy UIs
2. **Satoshi** — Contemporary geometric sans, balances warmth with precision
3. **Geist** (Vercel) — Modern, geometric, excellent for tech products
4. **Plus Jakarta Sans** — Slightly warmer than Inter, good for HR/people products
5. **DM Sans** — Clean geometric with personality

**Variable fonts**: Prefer variable font files (`.woff2` with `font-weight: 100 900`) over static weight files. Reduces total payload and enables fine-grained weight tuning (e.g., `450` for slightly heavier body text).

Pair with a **monospace** for data: `JetBrains Mono`, `Fira Code`, or `SF Mono`.

## Spacing System

Use an **8px base grid** with consistent application:

```
4px   — Tight: between label and value, icon and text
8px   — Compact: within components, between related items
12px  — Default: standard padding within cards
16px  — Comfortable: card padding, section gaps
24px  — Spacious: between cards, major sections
32px  — Generous: page-level section separation
48px  — Dramatic: hero areas, major visual breaks
```

### Responsive Breakpoints

```
sm:   640px   — container max-width: 640px
md:   768px   — container max-width: 768px
lg:   1024px  — container max-width: 1024px
xl:   1280px  — container max-width: 1280px
2xl:  1536px  — container max-width: 1536px
```

**Container queries**: Prefer `@container` over `@media` for component-level responsive behavior. This decouples components from viewport size, making them reusable across layouts (sidebars, modals, dashboards). Define `container-type: inline-size` on wrapper elements.

## Shadow System

Replace heavy borders with **layered shadows** for depth:

```css
--shadow-xs:  0 1px 2px rgba(0,0,0,0.04);                          /* Subtle lift */
--shadow-sm:  0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);  /* Cards */
--shadow-md:  0 4px 6px rgba(0,0,0,0.04), 0 2px 4px rgba(0,0,0,0.03);  /* Hover state */
--shadow-lg:  0 10px 15px rgba(0,0,0,0.05), 0 4px 6px rgba(0,0,0,0.03); /* Modals, dropdowns */

/* Frosted glass overlay */
--blur-overlay: backdrop-filter: blur(12px);   /* Modals, command palette, dropdowns */
               /* Pair with semi-transparent bg: rgba(255,255,255,0.7) light / rgba(15,17,23,0.7) dark */
```

## Component Patterns

### Navigation (Sidebar)

```
DO:
├── Light/neutral background (#FFFFFF or #F8F9FA)
├── Subtle border-right (1px, low opacity)
├── Icon + label for each item (icons at 18-20px, consistent stroke weight)
├── Active state: subtle primary-tinted background + left accent border (2-3px)
├── Hover state: light gray background (#F1F3F5)
├── Grouped sections with subtle overline labels
├── User avatar + name at bottom or top
└── Collapsed state support for responsive

DON'T:
├── Solid colored background (no blue/purple/dark sidebars for light themes)
├── White text on colored background
├── Icons without labels (unless collapsed mode)
├── Active state that's just bold text
└── More than 8-10 top-level items
```

### Cards & Containers

```
Structure:
├── Background: white (#FFFFFF)
├── Border: 1px solid rgba(0,0,0,0.06) OR shadow-sm (not both)
├── Border-radius: 8-12px (consistent across all cards)
├── Padding: 16-24px
├── Hover: shadow-md + translateY(-1px) for interactive cards
└── Header pattern: Overline label → Value → Trend/subtitle
```

### Tables

```
Structure:
├── Header: text-xs, uppercase, letter-spaced, text-secondary, bg-surface-alt
├── Rows: comfortable padding (12-16px vertical), border-bottom at 40% opacity
├── Hover: subtle background change (#F8F9FA)
├── Selected: primary-subtle background
├── Status badges: pill-shaped, small, color-coded with subtle backgrounds
├── Actions: text buttons or icon buttons, right-aligned
└── Search/filter bar: above table, clean input with icon prefix

Status Badge Pattern:
├── Active/Success: bg-emerald-50, text-emerald-700, border-emerald-200
├── Pending/Warning: bg-amber-50, text-amber-700, border-amber-200
├── Inactive/Error: bg-red-50, text-red-700, border-red-200
└── Default/Info: bg-slate-50, text-slate-600, border-slate-200
```

### Buttons

```
Hierarchy (most to least prominent):
├── Primary: Solid brand color, white text (ONE per view/section)
├── Secondary: White bg, subtle border, dark text
├── Ghost: No bg, no border, text + icon only (hover shows bg)
└── Danger: Red variant, used only for destructive actions

Sizing:
├── Default: h-9 (36px), px-4, text-sm, rounded-md (6px)
├── Small: h-8 (32px), px-3, text-xs, rounded-md
└── Icon-only: square, same height, rounded-md
```

### Forms & Inputs

```
Input fields:
├── Height: 36-40px
├── Border: 1px solid #E2E8F0
├── Border-radius: 6px
├── Focus: border-color transition to primary, subtle ring (0 0 0 3px primary/15%)
├── Label: text-sm, font-medium, mb-1.5
├── Helper text: text-xs, text-secondary, mt-1
└── Error state: border-red-300, ring-red-100, helper text in red
```

### Modals & Dialogs

```
Size tiers:
├── sm:   max-width 400px  — Confirmations, simple forms
├── md:   max-width 560px  — Standard forms, detail views
├── lg:   max-width 720px  — Complex forms, multi-step flows
└── full: max-width 960px  — Data-heavy views, editors (use sparingly)

Overlay: rgba(0,0,0,0.4) + backdrop-filter: blur(12px)
Animation: fade-in overlay (150ms) + scale modal from 0.95→1.0 (200ms ease-out)
Close: ESC key, overlay click, and explicit close button (top-right)
Focus: trap focus within modal; return focus to trigger on close
Border-radius: 12px
Padding: 24px body, 16-20px header/footer
```

### Dropdowns & Popovers

```
Structure:
├── Background: var(--color-surface)
├── Border: 1px solid var(--color-border) OR shadow-lg (not both)
├── Border-radius: 8px
├── Padding: 4px (container), 8-10px 12px (items)
├── Max-height: 320px with overflow-y: auto
├── Placement: bottom-start default, auto-flip when clipped
├── Animation: scale from 0.95 + fade, 150ms ease-out
└── Keyboard: arrow keys navigate, Enter selects, ESC closes
```

### Toasts & Notifications

```
Structure:
├── Position: top-right, 16px from edges
├── Width: 360-400px
├── Shadow: shadow-lg
├── Border-radius: 8px
├── Auto-dismiss: 5s default (errors persist until dismissed)
├── Stacking: newest on top, max 3 visible, older ones collapse
├── Animation: slide-in from right (200ms) + fade-out on dismiss
├── Action button: optional inline text button (e.g., "Undo")
└── Types: success (green left border), error (red), warning (amber), info (blue)
```

### Empty States

```
Structure:
├── Illustration: light, line-art style, muted colors (not cartoonish)
│   <!-- ADAPT: Use project illustration library or a minimal SVG set -->
├── Heading: text-lg, medium 500, text-primary — what happened
├── Description: text-sm, text-secondary — why and what to do next
├── CTA: primary button centered below description
└── Placement: centered vertically and horizontally in the content area

Examples: "No results found", "Get started by creating your first...", "Nothing here yet"
```

### Command Palette (Cmd+K)

```
Structure:
├── Overlay: rgba(0,0,0,0.4) + backdrop-filter: blur(12px)
├── Container: max-width 640px, centered, top-third of viewport
├── Search input: large (text-lg), no border, full-width, autofocus
├── Result groups: labeled sections (Pages, Actions, Recent)
├── Result items: icon + label + optional description + shortcut badge
├── Keyboard: ↑↓ navigate, Enter selects, ESC closes
├── Animation: fade-in + slide-down (150ms)
├── Max-height: 480px with scroll on results area
└── Empty state: "No results" with suggestion text
```

## Interaction & Motion

```css
/* Base transition for all interactive elements */
transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);

/* Specific overrides */
--transition-fast: 100ms ease;     /* Hover states, focus rings */
--transition-base: 150ms ease;     /* Color changes, shadows */
--transition-slow: 250ms ease;     /* Layout shifts, reveals */
--transition-spring: 300ms cubic-bezier(0.34, 1.56, 0.64, 1); /* Playful bounces */

/* Page/route transitions */
--transition-page-fade: opacity 150ms ease;                     /* Preferred: simple crossfade */
--transition-page-slide: transform 200ms ease, opacity 150ms ease; /* Directional slide for wizards/steps */

/* List reorder: use layout animations (e.g., Framer Motion layoutId, FLIP technique) */
/* Keep reorder animations ≤ 200ms to feel snappy, not sluggish */
```

All animations should respect `prefers-reduced-motion: reduce` — disable transforms and use instant opacity changes only.

## Loading States

```
Skeleton screens > spinners > progress bars
├── Skeleton: pulse animation on gray rectangles matching content shape
├── Spinner: only for isolated actions (button submit, inline load)
└── Progress: only when you can estimate completion %
```

## Density Modes

<!-- ADAPT: Enable density switching if building data-heavy enterprise tools -->

Optional density presets for interfaces that need to display varying amounts of information. Apply by toggling a `data-density` attribute on a container or `<html>`.

```
Compact:
├── Table row padding:   6-8px vertical
├── Card padding:        12px
├── Section gap:         16px
├── Font size:           text-xs (12px) body
└── Use case:            Power users, dense dashboards, data grids

Comfortable (default):
├── Table row padding:   12-16px vertical
├── Card padding:        16-24px
├── Section gap:         24px
├── Font size:           text-sm (14px) body
└── Use case:            Standard workflows, forms, detail views

Spacious:
├── Table row padding:   16-20px vertical
├── Card padding:        24-32px
├── Section gap:         32px
├── Font size:           text-sm (14px) body
└── Use case:            Onboarding, marketing-adjacent pages, low-data views
```

Implement via CSS custom properties scoped to `[data-density]` selectors. Let users persist their preference in `localStorage`.

## AI UI Patterns

<!-- ADAPT: This area is evolving rapidly — adjust patterns to match your AI integration approach -->

Patterns for interfaces that incorporate AI-generated content or AI-assisted interactions.

### Streaming Text

```
├── Typewriter effect: render tokens as they arrive, ~30-50ms per token visually
├── Cursor: blinking block or line cursor at insertion point during streaming
├── Container: auto-scroll to bottom, but pause if user scrolls up
├── Loading: show a subtle pulse or skeleton before first token arrives
└── Complete state: cursor disappears, content becomes selectable
```

### Inline Suggestions

```
├── Ghost text: render suggestion in text-tertiary (40-50% opacity) inline with cursor
├── Accept: Tab key or → arrow to accept full suggestion
├── Partial accept: Cmd+→ to accept word-by-word
├── Reject: continue typing or ESC to dismiss
└── Attribution: subtle "AI" micro-badge near suggestion origin (optional)
```

### AI-Attributed Content

```
├── Border treatment: subtle left border (2px) in a distinct muted color (e.g., violet-300/purple-300)
├── Badge: small "AI" pill badge — bg-violet-50, text-violet-600, text-xs
├── Distinguish from user content: slight background tint or border, never a heavy visual break
└── Editable: AI content should always be editable by the user with clear "edited" state
```

## Accessibility Baseline

- Color contrast: 4.5:1 for body text, 3:1 for large text and UI components
- Focus indicators: visible ring on all interactive elements (don't remove outlines)
- Touch targets: minimum 44x44px for mobile, 32x32px for desktop
- Keyboard navigation: logical tab order, visible focus states
- Status communication: never rely on color alone (add icons, text, patterns)
- Reduced motion: respect `prefers-reduced-motion` media query
