import { TaskDto } from '../../services/taskApi';

/**
 * CORRECT FILE — clean, focused row component.
 */

interface TaskRowProps {
  task: TaskDto;
  onClick: () => void;
}

export function TaskRow({ task, onClick }: TaskRowProps) {
  const priorityLabel = task.priority >= 8 ? 'High' : task.priority >= 4 ? 'Medium' : 'Low';
  const priorityClass = `priority-${priorityLabel.toLowerCase()}`;

  const formatDate = (dateStr: string | null) => {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString();
  };

  return (
    <div className="task-row" onClick={onClick}>
      <span className="task-title">{task.title}</span>
      <span className={`task-status status-${task.status.toLowerCase()}`}>
        {task.status}
      </span>
      <span className={`task-priority ${priorityClass}`}>
        {priorityLabel}
      </span>
      <span className="task-due-date">{formatDate(task.dueDate)}</span>
      <span className="task-assigned">{task.assignedTo || 'Unassigned'}</span>
    </div>
  );
}
