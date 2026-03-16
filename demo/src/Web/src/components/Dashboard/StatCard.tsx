/**
 * CORRECT FILE — clean, focused component.
 * Single responsibility, props-driven, no side effects.
 */

interface StatCardProps {
  title: string;
  value: string | number;
  trend?: 'up' | 'down' | 'stable';
  subtitle?: string;
}

export function StatCard({ title, value, trend, subtitle }: StatCardProps) {
  return (
    <div className="stat-card">
      <div className="stat-card-title">{title}</div>
      <div className="stat-card-value">{value}</div>
      {trend && (
        <div className={`stat-card-trend trend-${trend}`}>
          {trend === 'up' ? '↑' : trend === 'down' ? '↓' : '→'}
        </div>
      )}
      {subtitle && <div className="stat-card-subtitle">{subtitle}</div>}
    </div>
  );
}
