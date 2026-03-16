import { useState, useEffect } from 'react';
import { TaskDto, fetchTasks } from '../services/taskApi';

/**
 * VIOLATION FILE
 *
 * VIOLATES:
 *   SEC-002 — userId is read from URL search params (user-supplied input).
 *             UserId must come from the authenticated session/token,
 *             never from request body, query string, or route parameters.
 */
export function useTasks() {
  const [tasks, setTasks] = useState<TaskDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // VIOLATION SEC-002: userId from URL — any user can manipulate this
  // to view another user's tasks by changing the query parameter.
  // Should come from auth context/session, not user-controlled input.
  const params = new URLSearchParams(window.location.search);
  const userId = params.get('userId');

  useEffect(() => {
    async function loadTasks() {
      try {
        setLoading(true);
        const data = await fetchTasks();
        setTasks(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Failed to load tasks');
      } finally {
        setLoading(false);
      }
    }

    loadTasks();
  }, [userId]);

  return { tasks, loading, error, userId };
}
