import { useState, useEffect } from 'react';
import { DashboardDto, fetchDashboard } from '../../services/taskApi';
import { StatCard } from './StatCard';

/**
 * VIOLATION FILE — "god component"
 *
 * VIOLATES:
 *   CC-003  — Component far exceeds 200 lines.
 *   RC-002  — Business logic mixed with presentation (calculations inline).
 *   RC-005  — No separation of concerns; fetching, computing, and rendering
 *             all in one component. Should extract hooks and sub-components.
 *
 * Also triggers data-clarifier: 15+ metrics displayed with no visual
 * hierarchy, progressive disclosure, or cognitive load management.
 */
export function DashboardPage() {
  const [dashboard, setDashboard] = useState<DashboardDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [timeRange, setTimeRange] = useState<'week' | 'month' | 'quarter'>('week');
  const [showDetails, setShowDetails] = useState(false);
  const [selectedMetric, setSelectedMetric] = useState<string | null>(null);
  const [refreshCount, setRefreshCount] = useState(0);

  // VIOLATION RC-002: Data fetching mixed directly in component
  useEffect(() => {
    async function loadDashboard() {
      try {
        setLoading(true);
        const data = await fetchDashboard();
        setDashboard(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load dashboard');
      } finally {
        setLoading(false);
      }
    }

    loadDashboard();
  }, [refreshCount]);

  // VIOLATION RC-002: Business logic inline — should be in a custom hook
  const getCompletionTrend = () => {
    if (!dashboard) return 'neutral';
    if (dashboard.completionRate > 75) return 'up';
    if (dashboard.completionRate > 50) return 'stable';
    return 'down';
  };

  const getPriorityDistribution = () => {
    if (!dashboard) return [];
    return [
      { label: 'High', value: dashboard.highPriorityCount, color: '#EF4444' },
      { label: 'Medium', value: dashboard.mediumPriorityCount, color: '#F59E0B' },
      { label: 'Low', value: dashboard.lowPriorityCount, color: '#10B981' },
    ];
  };

  const getStatusBreakdown = () => {
    if (!dashboard) return [];
    const total = dashboard.totalTasks || 1;
    return [
      { label: 'Completed', value: dashboard.completedTasks, pct: (dashboard.completedTasks / total * 100).toFixed(1) },
      { label: 'In Progress', value: dashboard.inProgressTasks, pct: (dashboard.inProgressTasks / total * 100).toFixed(1) },
      { label: 'Blocked', value: dashboard.blockedTasks, pct: (dashboard.blockedTasks / total * 100).toFixed(1) },
      { label: 'Overdue', value: dashboard.overdueTasks, pct: (dashboard.overdueTasks / total * 100).toFixed(1) },
    ];
  };

  const getProductivityScore = () => {
    if (!dashboard) return 0;
    const completionWeight = dashboard.completionRate * 0.4;
    const overdueWeight = (1 - (dashboard.overdueTasks / Math.max(dashboard.totalTasks, 1))) * 100 * 0.3;
    const velocityWeight = (dashboard.tasksCompletedThisWeek / Math.max(dashboard.tasksCreatedThisWeek, 1)) * 100 * 0.3;
    return Math.min(Math.round(completionWeight + overdueWeight + velocityWeight), 100);
  };

  const getHealthIndicator = () => {
    const score = getProductivityScore();
    if (score > 80) return { label: 'Excellent', color: '#10B981' };
    if (score > 60) return { label: 'Good', color: '#3B82F6' };
    if (score > 40) return { label: 'Needs Attention', color: '#F59E0B' };
    return { label: 'Critical', color: '#EF4444' };
  };

  if (loading) {
    return <div className="dashboard-loading">Loading dashboard...</div>;
  }

  if (error) {
    return <div className="dashboard-error">{error}</div>;
  }

  if (!dashboard) {
    return <div>No data available</div>;
  }

  const trend = getCompletionTrend();
  const priorities = getPriorityDistribution();
  const statuses = getStatusBreakdown();
  const productivity = getProductivityScore();
  const health = getHealthIndicator();

  // DATA-CLARIFIER TRIGGER: 15+ metrics dumped with no hierarchy
  // All metrics presented at same visual weight — wall of numbers
  return (
    <div className="dashboard">
      <div className="dashboard-header">
        <h1>Dashboard</h1>
        <div className="dashboard-controls">
          <select value={timeRange} onChange={(e) => setTimeRange(e.target.value as 'week' | 'month' | 'quarter')}>
            <option value="week">This Week</option>
            <option value="month">This Month</option>
            <option value="quarter">This Quarter</option>
          </select>
          <button onClick={() => setRefreshCount(c => c + 1)}>Refresh</button>
          <button onClick={() => setShowDetails(!showDetails)}>
            {showDetails ? 'Hide Details' : 'Show Details'}
          </button>
        </div>
      </div>

      {/* Row 1: Primary stats — no visual hierarchy, all same size */}
      <div className="stat-grid">
        <StatCard title="Total Tasks" value={dashboard.totalTasks} />
        <StatCard title="Completed" value={dashboard.completedTasks} />
        <StatCard title="Overdue" value={dashboard.overdueTasks} />
        <StatCard title="Completion Rate" value={`${dashboard.completionRate.toFixed(1)}%`} />
        <StatCard title="Avg Completion Days" value={dashboard.averageCompletionDays.toFixed(1)} />
      </div>

      {/* Row 2: More stats at the same visual level */}
      <div className="stat-grid">
        <StatCard title="Created This Week" value={dashboard.tasksCreatedThisWeek} />
        <StatCard title="Completed This Week" value={dashboard.tasksCompletedThisWeek} />
        <StatCard title="Due Today" value={dashboard.tasksDueToday} />
        <StatCard title="Due This Week" value={dashboard.tasksDueThisWeek} />
        <StatCard title="Unassigned" value={dashboard.unassignedTasks} />
      </div>

      {/* Row 3: Even more metrics at the same level */}
      <div className="stat-grid">
        <StatCard title="High Priority" value={dashboard.highPriorityCount} />
        <StatCard title="Medium Priority" value={dashboard.mediumPriorityCount} />
        <StatCard title="Low Priority" value={dashboard.lowPriorityCount} />
        <StatCard title="Blocked" value={dashboard.blockedTasks} />
        <StatCard title="In Progress" value={dashboard.inProgressTasks} />
      </div>

      {/* Productivity Score */}
      <div className="productivity-section">
        <h2>Productivity Score</h2>
        <div className="score-display">
          <span className="score-value" style={{ color: health.color }}>{productivity}</span>
          <span className="score-label" style={{ color: health.color }}>{health.label}</span>
        </div>
        <div className="score-bar">
          <div className="score-fill" style={{ width: `${productivity}%`, backgroundColor: health.color }} />
        </div>
      </div>

      {/* Priority Distribution */}
      <div className="priority-section">
        <h2>Priority Distribution</h2>
        <div className="priority-bars">
          {priorities.map(p => (
            <div key={p.label} className="priority-bar-row">
              <span className="priority-label">{p.label}</span>
              <div className="priority-bar-track">
                <div
                  className="priority-bar-fill"
                  style={{
                    width: `${(p.value / Math.max(dashboard.totalTasks, 1)) * 100}%`,
                    backgroundColor: p.color,
                  }}
                />
              </div>
              <span className="priority-count">{p.value}</span>
            </div>
          ))}
        </div>
      </div>

      {/* Status Breakdown */}
      <div className="status-section">
        <h2>Status Breakdown</h2>
        <div className="status-cards">
          {statuses.map(s => (
            <div
              key={s.label}
              className={`status-card ${selectedMetric === s.label ? 'selected' : ''}`}
              onClick={() => setSelectedMetric(selectedMetric === s.label ? null : s.label)}
            >
              <div className="status-card-value">{s.value}</div>
              <div className="status-card-label">{s.label}</div>
              <div className="status-card-pct">{s.pct}%</div>
            </div>
          ))}
        </div>
      </div>

      {/* Trend indicator */}
      <div className="trend-section">
        <h2>Completion Trend</h2>
        <div className={`trend-indicator trend-${trend}`}>
          {trend === 'up' ? '↑ Improving' : trend === 'stable' ? '→ Stable' : '↓ Declining'}
        </div>
      </div>

      {/* Conditional details section */}
      {showDetails && (
        <div className="details-section">
          <h2>Detailed Metrics</h2>
          <table className="details-table">
            <thead>
              <tr>
                <th>Metric</th>
                <th>Value</th>
                <th>Trend</th>
              </tr>
            </thead>
            <tbody>
              <tr><td>Total Tasks</td><td>{dashboard.totalTasks}</td><td>-</td></tr>
              <tr><td>Completed</td><td>{dashboard.completedTasks}</td><td>-</td></tr>
              <tr><td>Overdue</td><td>{dashboard.overdueTasks}</td><td>-</td></tr>
              <tr><td>High Priority</td><td>{dashboard.highPriorityCount}</td><td>-</td></tr>
              <tr><td>Medium Priority</td><td>{dashboard.mediumPriorityCount}</td><td>-</td></tr>
              <tr><td>Low Priority</td><td>{dashboard.lowPriorityCount}</td><td>-</td></tr>
              <tr><td>Completion Rate</td><td>{dashboard.completionRate.toFixed(1)}%</td><td>{trend}</td></tr>
              <tr><td>Avg Days to Complete</td><td>{dashboard.averageCompletionDays.toFixed(1)}</td><td>-</td></tr>
              <tr><td>Created This Week</td><td>{dashboard.tasksCreatedThisWeek}</td><td>-</td></tr>
              <tr><td>Completed This Week</td><td>{dashboard.tasksCompletedThisWeek}</td><td>-</td></tr>
              <tr><td>Due Today</td><td>{dashboard.tasksDueToday}</td><td>-</td></tr>
              <tr><td>Due This Week</td><td>{dashboard.tasksDueThisWeek}</td><td>-</td></tr>
              <tr><td>Unassigned</td><td>{dashboard.unassignedTasks}</td><td>-</td></tr>
              <tr><td>Blocked</td><td>{dashboard.blockedTasks}</td><td>-</td></tr>
              <tr><td>In Progress</td><td>{dashboard.inProgressTasks}</td><td>-</td></tr>
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}
