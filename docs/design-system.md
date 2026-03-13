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

## Typography Scale

Use a **systematic type scale** with clear hierarchy:

```
Display:    text-2xl  (24-28px) — Page titles only, semibold 600
Heading:    text-lg   (18-20px) — Section headings, medium 500
Subheading: text-base (15-16px) — Card titles, subsections, medium 500
Body:       text-sm   (13-14px) — Primary content, regular 400
Caption:    text-xs   (11-12px) — Metadata, timestamps, labels, medium 500
Overline:   text-xs   (10-11px) — Category labels, ALL-CAPS with letter-spacing 0.05em
```

**Font choices** (in order of preference for enterprise SaaS):
1. **Inter** — Excellent clarity at small sizes, ideal for data-heavy UIs
2. **Geist** (Vercel) — Modern, geometric, excellent for tech products
3. **Plus Jakarta Sans** — Slightly warmer than Inter, good for HR/people products
4. **DM Sans** — Clean geometric with personality

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

## Shadow System

Replace heavy borders with **layered shadows** for depth:

```css
--shadow-xs:  0 1px 2px rgba(0,0,0,0.04);                          /* Subtle lift */
--shadow-sm:  0 1px 3px rgba(0,0,0,0.06), 0 1px 2px rgba(0,0,0,0.04);  /* Cards */
--shadow-md:  0 4px 6px rgba(0,0,0,0.04), 0 2px 4px rgba(0,0,0,0.03);  /* Hover state */
--shadow-lg:  0 10px 15px rgba(0,0,0,0.05), 0 4px 6px rgba(0,0,0,0.03); /* Modals, dropdowns */
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

## Interaction & Motion

```css
/* Base transition for all interactive elements */
transition: all 150ms cubic-bezier(0.4, 0, 0.2, 1);

/* Specific overrides */
--transition-fast: 100ms ease;     /* Hover states, focus rings */
--transition-base: 150ms ease;     /* Color changes, shadows */
--transition-slow: 250ms ease;     /* Layout shifts, reveals */
--transition-spring: 300ms cubic-bezier(0.34, 1.56, 0.64, 1); /* Playful bounces */
```

## Loading States

```
Skeleton screens > spinners > progress bars
├── Skeleton: pulse animation on gray rectangles matching content shape
├── Spinner: only for isolated actions (button submit, inline load)
└── Progress: only when you can estimate completion %
```

## Accessibility Baseline

- Color contrast: 4.5:1 for body text, 3:1 for large text and UI components
- Focus indicators: visible ring on all interactive elements (don't remove outlines)
- Touch targets: minimum 44x44px for mobile, 32x32px for desktop
- Keyboard navigation: logical tab order, visible focus states
- Status communication: never rely on color alone (add icons, text, patterns)
- Reduced motion: respect `prefers-reduced-motion` media query
