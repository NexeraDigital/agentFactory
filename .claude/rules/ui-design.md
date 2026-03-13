---
description: "UI/UX design rules for modern enterprise interfaces. Enforces design system consistency, visual hierarchy, and accessibility standards."
paths: # ADAPT: adjust to your frontend/styling file patterns
  - "**/*.tsx"
  - "**/*.jsx"
  - "**/*.css"
  - "**/*.scss"
---

# UI Design Rules

| ID | Severity | Rule | Details |
|----|----------|------|---------|
| UI-001 | **High** | No saturated primary sidebars | Solid blue/purple navigation backgrounds are a hallmark of generic UIs. Use neutral backgrounds (#FFFFFF or #F8F9FA) with subtle active state indicators. |
| UI-002 | **High** | Maximum 3 colors beyond neutrals | Establish a strict 2-3 color system. Reserve the brand/primary color for the ONE primary action per view. Use semantic colors (success, warning, danger) consistently. |
| UI-003 | **High** | Clear typographic hierarchy | At least 3-4 distinct type levels must be visible. No same-size text doing different jobs. Use display → heading → subheading → body → caption scale. |
| UI-004 | **High** | Every interactive element has hover state | Cards: shadow elevation. Table rows: background shift. Buttons: darken/lighten. Links: color shift. Nav items: background tint. No exceptions. |
| UI-005 | **Medium** | 8px spacing grid | Use consistent spacing from the 8px system: 4px (tight), 8px (compact), 12px (default), 16px (comfortable), 24px (spacious), 32px (generous). |
| UI-006 | **Medium** | Shadows over borders | Replace heavy borders with layered shadows for depth. Use 1px borders at reduced opacity only for subtle separation. Never combine heavy borders AND shadows on the same element. |
| UI-007 | **Medium** | Accessibility baseline | 4.5:1 contrast for body text, 3:1 for large text/UI components. Visible focus indicators on all interactive elements. Minimum 44x44px touch targets (mobile) / 32x32px (desktop). Respect `prefers-reduced-motion`. |
| UI-008 | **Low** | No "template smell" | The UI should not look like default framework output or generic AI-generated design. Evaluate: would a user mistake this for a custom-designed product? |
