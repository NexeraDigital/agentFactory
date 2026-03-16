import { useState, useEffect } from 'react';

/**
 * VIOLATION FILE
 *
 * VIOLATES:
 *   RC-003 — Uses useEffect to compute derived state that should
 *            be calculated directly during render (useMemo or inline).
 *   RC-004 — Unnecessary useEffect for state synchronization.
 *            The activeFilterCount can be computed from props directly.
 */

interface TaskFiltersProps {
  statusFilter: string;
  priorityFilter: string;
  searchQuery: string;
  onStatusChange: (status: string) => void;
  onPriorityChange: (priority: string) => void;
  onSearchChange: (query: string) => void;
}

export function TaskFilters({
  statusFilter,
  priorityFilter,
  searchQuery,
  onStatusChange,
  onPriorityChange,
  onSearchChange,
}: TaskFiltersProps) {
  // VIOLATION RC-003/RC-004: Derived state via useEffect
  // This should be: const activeFilterCount = [statusFilter !== 'all', ...].filter(Boolean).length
  const [activeFilterCount, setActiveFilterCount] = useState(0);

  // VIOLATION: useEffect to sync derived state — unnecessary and can cause extra renders
  useEffect(() => {
    let count = 0;
    if (statusFilter !== 'all') count++;
    if (priorityFilter !== 'all') count++;
    if (searchQuery.length > 0) count++;
    setActiveFilterCount(count);
  }, [statusFilter, priorityFilter, searchQuery]);

  // VIOLATION RC-003: Another derived state via useEffect
  const [filterSummary, setFilterSummary] = useState('');

  useEffect(() => {
    const parts: string[] = [];
    if (statusFilter !== 'all') parts.push(`Status: ${statusFilter}`);
    if (priorityFilter !== 'all') parts.push(`Priority: ${priorityFilter}`);
    if (searchQuery) parts.push(`Search: "${searchQuery}"`);
    setFilterSummary(parts.length > 0 ? `Filtering by ${parts.join(', ')}` : 'No filters applied');
  }, [statusFilter, priorityFilter, searchQuery]);

  return (
    <div className="task-filters">
      <div className="filter-bar">
        <div className="filter-group">
          <label>Search</label>
          <input
            type="text"
            placeholder="Search tasks..."
            value={searchQuery}
            onChange={(e) => onSearchChange(e.target.value)}
          />
        </div>

        <div className="filter-group">
          <label>Status</label>
          <select value={statusFilter} onChange={(e) => onStatusChange(e.target.value)}>
            <option value="all">All Statuses</option>
            <option value="Todo">To Do</option>
            <option value="InProgress">In Progress</option>
            <option value="Blocked">Blocked</option>
            <option value="Completed">Completed</option>
          </select>
        </div>

        <div className="filter-group">
          <label>Priority</label>
          <select value={priorityFilter} onChange={(e) => onPriorityChange(e.target.value)}>
            <option value="all">All Priorities</option>
            <option value="8">High</option>
            <option value="4">Medium</option>
            <option value="1">Low</option>
          </select>
        </div>

        {activeFilterCount > 0 && (
          <button
            className="clear-filters"
            onClick={() => {
              onStatusChange('all');
              onPriorityChange('all');
              onSearchChange('');
            }}
          >
            Clear ({activeFilterCount})
          </button>
        )}
      </div>

      <div className="filter-summary">{filterSummary}</div>
    </div>
  );
}
