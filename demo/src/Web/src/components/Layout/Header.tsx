/**
 * CORRECT FILE — clean header component.
 * Follows design system: neutral background, proper typography, subtle borders.
 */
export function Header() {
  return (
    <header className="app-header">
      <div className="header-left">
        <h1 className="header-title">TaskBoard</h1>
      </div>
      <div className="header-right">
        <div className="header-search">
          <input type="text" placeholder="Search..." className="search-input" />
        </div>
        <div className="header-user">
          <div className="user-avatar">JD</div>
          <span className="user-name">Jane Doe</span>
        </div>
      </div>
    </header>
  );
}
