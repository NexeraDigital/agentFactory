-- Seed data for demo
-- Uses the GUID PKs from 001 (which is itself a violation — see SQL-001)

INSERT INTO [dbo].[Tasks] ([Title], [Description], [Status], [Priority], [UserId], [AssignedTo], [DueDate])
VALUES
    ('Set up CI/CD pipeline', 'Configure GitHub Actions for automated deployment', 'Completed', 9, 'user-001', 'user-001', '2026-03-10'),
    ('Design database schema', 'Create initial SQL Server schema for TaskBoard', 'Completed', 8, 'user-001', 'user-001', '2026-03-08'),
    ('Implement auth middleware', 'Add JWT-based authentication to API endpoints', 'InProgress', 9, 'user-001', 'user-001', '2026-03-18'),
    ('Build dashboard page', 'Create React dashboard with key metrics', 'InProgress', 7, 'user-001', 'user-002', '2026-03-20'),
    ('Add task filtering', 'Implement status and priority filters on task list', 'Todo', 6, 'user-001', 'user-002', '2026-03-22'),
    ('Write unit tests', 'Add tests for PriorityEngine and TaskManager', 'Todo', 5, 'user-001', 'user-001', '2026-03-25'),
    ('Configure Key Vault', 'Move secrets from config to Azure Key Vault', 'Blocked', 10, 'user-001', 'user-001', '2026-03-15'),
    ('Performance optimization', 'Add AsNoTracking to read queries, implement keyset pagination', 'Todo', 4, 'user-001', 'user-002', NULL),
    ('Mobile responsive layout', 'Make dashboard and task list responsive', 'Todo', 3, 'user-001', 'user-002', '2026-04-01'),
    ('API documentation', 'Add Swagger annotations to all endpoints', 'Todo', 2, 'user-001', 'user-001', NULL);
