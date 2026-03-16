import { NavLink } from 'react-router-dom';

/**
 * VIOLATION FILE
 *
 * VIOLATES:
 *   UI-001 — Uses a heavily saturated dark blue background (#1a237e)
 *            instead of the design system's light/neutral sidebar.
 *            The design system specifies: "DON'T: Solid colored background
 *            (no blue/purple/dark sidebars for light themes)"
 *   UI-004 — No hover states on navigation items.
 *            The design system requires: "Hover state: light gray
 *            background (#F1F3F5)" on nav items.
 */
export function Sidebar() {
  return (
    // VIOLATION UI-001: Saturated dark sidebar — should be #FFFFFF or #F8F9FA
    <nav style={{
      width: 240,
      backgroundColor: '#1a237e',   // VIOLATION: saturated dark blue
      color: '#ffffff',              // VIOLATION: white text on colored bg
      padding: '20px 0',
      display: 'flex',
      flexDirection: 'column',
    }}>
      <div style={{
        padding: '0 20px 20px',
        borderBottom: '1px solid rgba(255,255,255,0.2)',
        marginBottom: 20,
      }}>
        <h2 style={{ margin: 0, fontSize: 20, fontWeight: 700 }}>TaskBoard</h2>
      </div>

      <div style={{ flex: 1 }}>
        {/* VIOLATION UI-004: No hover states — just static links */}
        <NavLink
          to="/"
          style={{
            display: 'block',
            padding: '12px 20px',
            color: '#ffffff',
            textDecoration: 'none',
            fontSize: 14,
            // No hover state defined
          }}
        >
          Dashboard
        </NavLink>

        <NavLink
          to="/tasks"
          style={{
            display: 'block',
            padding: '12px 20px',
            color: '#ffffff',
            textDecoration: 'none',
            fontSize: 14,
            // No hover state defined
          }}
        >
          Tasks
        </NavLink>

        <a
          href="#"
          style={{
            display: 'block',
            padding: '12px 20px',
            color: '#ffffff',
            textDecoration: 'none',
            fontSize: 14,
          }}
        >
          Settings
        </a>
      </div>

      <div style={{
        padding: '20px',
        borderTop: '1px solid rgba(255,255,255,0.2)',
        fontSize: 12,
        opacity: 0.7,
      }}>
        v0.1.0
      </div>
    </nav>
  );
}
