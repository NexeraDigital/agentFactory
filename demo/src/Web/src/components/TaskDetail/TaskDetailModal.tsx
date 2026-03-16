import { TaskDto } from '../../services/taskApi';

/**
 * VIOLATION FILE
 *
 * VIOLATES:
 *   RC-001 — Excessive prop drilling. This component receives props
 *            (userName, userRole, projectName, teamMembers) that should
 *            come from context providers instead of being threaded through
 *            every layer of the component tree.
 */

interface TaskDetailModalProps {
  task: TaskDto;
  onClose: () => void;
  onSave: (task: TaskDto) => void;
  onDelete: (taskId: number) => void;
  // VIOLATION RC-001: These props are drilled from App → TaskListPage → here
  // Should use React Context (UserContext, ProjectContext)
  userName: string;
  userRole: string;
  projectName: string;
  teamMembers: string[];
}

export function TaskDetailModal({
  task,
  onClose,
  onSave,
  onDelete,
  userName,
  userRole,
  projectName,
  teamMembers,
}: TaskDetailModalProps) {
  const canEdit = userRole === 'admin' || task.assignedTo === userName;
  const canDelete = userRole === 'admin';

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-content" onClick={(e) => e.stopPropagation()}>
        <div className="modal-header">
          <h2>{task.title}</h2>
          <span className="modal-project">{projectName}</span>
          <button className="modal-close" onClick={onClose}>×</button>
        </div>

        <div className="modal-body">
          <div className="field-group">
            <label>Description</label>
            <p>{task.description}</p>
          </div>

          <div className="field-row">
            <div className="field-group">
              <label>Status</label>
              <span className={`status-badge status-${task.status.toLowerCase()}`}>
                {task.status}
              </span>
            </div>
            <div className="field-group">
              <label>Priority</label>
              <span>{task.priority}</span>
            </div>
          </div>

          <div className="field-row">
            <div className="field-group">
              <label>Assigned To</label>
              {/* VIOLATION RC-001: teamMembers drilled through just for this dropdown */}
              <select defaultValue={task.assignedTo}>
                {teamMembers.map(member => (
                  <option key={member} value={member}>{member}</option>
                ))}
              </select>
            </div>
            <div className="field-group">
              <label>Due Date</label>
              <span>{task.dueDate ? new Date(task.dueDate).toLocaleDateString() : 'No due date'}</span>
            </div>
          </div>

          <div className="field-group">
            <label>Created</label>
            <span>{new Date(task.createdAt).toLocaleString()}</span>
          </div>

          {/* VIOLATION RC-001: userName drilled through for display */}
          <div className="field-group">
            <label>Viewing as</label>
            <span>{userName} ({userRole})</span>
          </div>
        </div>

        <div className="modal-footer">
          {canEdit && (
            <button className="btn-primary" onClick={() => onSave(task)}>
              Save Changes
            </button>
          )}
          {canDelete && (
            <button className="btn-danger" onClick={() => onDelete(task.id)}>
              Delete Task
            </button>
          )}
          <button className="btn-secondary" onClick={onClose}>
            Cancel
          </button>
        </div>
      </div>
    </div>
  );
}
