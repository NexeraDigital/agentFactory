export interface TaskDto {
  id: number;
  title: string;
  description: string;
  status: string;
  priority: number;
  assignedTo: string;
  createdAt: string;
  dueDate: string | null;
}

export interface DashboardDto {
  totalTasks: number;
  completedTasks: number;
  overdueTasks: number;
  highPriorityCount: number;
  mediumPriorityCount: number;
  lowPriorityCount: number;
  completionRate: number;
  averageCompletionDays: number;
  tasksCreatedThisWeek: number;
  tasksCompletedThisWeek: number;
  tasksDueToday: number;
  tasksDueThisWeek: number;
  unassignedTasks: number;
  blockedTasks: number;
  inProgressTasks: number;
}

const API_BASE = '/api/task';

export async function fetchTasks(): Promise<TaskDto[]> {
  const response = await fetch(API_BASE);
  if (!response.ok) throw new Error(`Failed to fetch tasks: ${response.status}`);
  return response.json();
}

export async function fetchDashboard(): Promise<DashboardDto> {
  const response = await fetch(`${API_BASE}/dashboard`);
  if (!response.ok) throw new Error(`Failed to fetch dashboard: ${response.status}`);
  return response.json();
}

export async function createTask(request: {
  title: string;
  description: string;
  priority: number;
  dueDate: string | null;
}): Promise<TaskDto> {
  const response = await fetch(API_BASE, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error(`Failed to create task: ${response.status}`);
  return response.json();
}

export async function updateTask(
  taskId: number,
  request: { title: string; description: string; status: string; priority: number; dueDate: string | null }
): Promise<TaskDto> {
  const response = await fetch(`${API_BASE}/${taskId}`, {
    method: 'PUT',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(request),
  });
  if (!response.ok) throw new Error(`Failed to update task: ${response.status}`);
  return response.json();
}

export async function deleteTask(taskId: number): Promise<void> {
  const response = await fetch(`${API_BASE}/${taskId}`, { method: 'DELETE' });
  if (!response.ok) throw new Error(`Failed to delete task: ${response.status}`);
}
