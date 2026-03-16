import { Routes, Route } from 'react-router-dom';
import { Sidebar } from './components/Layout/Sidebar';
import { Header } from './components/Layout/Header';
import { DashboardPage } from './components/Dashboard/DashboardPage';
import { TaskListPage } from './components/TaskList/TaskListPage';

export function App() {
  return (
    <div style={{ display: 'flex', height: '100vh' }}>
      <Sidebar />
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        <Header />
        <main style={{ flex: 1, overflow: 'auto', padding: '24px' }}>
          <Routes>
            <Route path="/" element={<DashboardPage />} />
            <Route path="/tasks" element={<TaskListPage />} />
          </Routes>
        </main>
      </div>
    </div>
  );
}
