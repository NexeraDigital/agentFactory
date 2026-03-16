export type TaskStatus = 'Todo' | 'InProgress' | 'Blocked' | 'Completed';

export const TASK_STATUS_LABELS: Record<TaskStatus, string> = {
  Todo: 'To Do',
  InProgress: 'In Progress',
  Blocked: 'Blocked',
  Completed: 'Completed',
};

export const TASK_STATUS_COLORS: Record<TaskStatus, string> = {
  Todo: '#64748B',
  InProgress: '#3B82F6',
  Blocked: '#EF4444',
  Completed: '#10B981',
};
