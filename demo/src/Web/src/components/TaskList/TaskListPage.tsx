import { useState } from 'react';
import { useTasks } from '../../hooks/useTasks';
import { TaskRow } from './TaskRow';
import { TaskFilters } from './TaskFilters';
import { TaskDetailModal } from '../TaskDetail/TaskDetailModal';
import { TaskDto } from '../../services/taskApi';

/**
 * CORRECT FILE — clean page component.
 * Delegates data fetching to hook, rendering to child components.
 */
export function TaskListPage() {
  const { tasks, loading, error } = useTasks();
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [priorityFilter, setPriorityFilter] = useState<string>('all');
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTask, setSelectedTask] = useState<TaskDto | null>(null);

  if (loading) return <div className="loading">Loading tasks...</div>;
  if (error) return <div className="error">{error}</div>;

  const filteredTasks = tasks.filter(task => {
    if (statusFilter !== 'all' && task.status !== statusFilter) return false;
    if (priorityFilter !== 'all') {
      const p = parseInt(priorityFilter);
      if (task.priority !== p) return false;
    }
    if (searchQuery && !task.title.toLowerCase().includes(searchQuery.toLowerCase())) return false;
    return true;
  });

  return (
    <div className="task-list-page">
      <div className="task-list-header">
        <h1>Tasks</h1>
        <button className="btn-primary">New Task</button>
      </div>

      <TaskFilters
        statusFilter={statusFilter}
        priorityFilter={priorityFilter}
        searchQuery={searchQuery}
        onStatusChange={setStatusFilter}
        onPriorityChange={setPriorityFilter}
        onSearchChange={setSearchQuery}
      />

      <div className="task-table">
        <div className="task-table-header">
          <span>Title</span>
          <span>Status</span>
          <span>Priority</span>
          <span>Due Date</span>
          <span>Assigned To</span>
        </div>
        {filteredTasks.map(task => (
          <TaskRow key={task.id} task={task} onClick={() => setSelectedTask(task)} />
        ))}
        {filteredTasks.length === 0 && (
          <div className="task-table-empty">No tasks match your filters.</div>
        )}
      </div>

      {selectedTask && (
        <TaskDetailModal
          task={selectedTask}
          onClose={() => setSelectedTask(null)}
          onSave={() => setSelectedTask(null)}
          onDelete={() => setSelectedTask(null)}
          userName="Current User"
          userRole="admin"
          projectName="TaskBoard"
          teamMembers={['Alice', 'Bob', 'Charlie']}
        />
      )}
    </div>
  );
}
