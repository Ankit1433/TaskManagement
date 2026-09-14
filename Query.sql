-- ============================================================
-- TASK MANAGEMENT DATABASE
-- PostgreSQL / Neon
-- ============================================================


-- ============================================================
-- 1. USERS TABLE
-- ============================================================

CREATE TABLE users
(
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    name VARCHAR(100) NOT NULL,

    email VARCHAR(150) NOT NULL UNIQUE,

    password_hash VARCHAR(255) NOT NULL,

    role VARCHAR(30) NOT NULL DEFAULT 'Employee',

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);


-- ============================================================
-- 2. TASKS TABLE
-- ============================================================

CREATE TABLE tasks
(
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    title VARCHAR(200) NOT NULL,

    description TEXT,

    status VARCHAR(30) NOT NULL DEFAULT 'Pending',

    priority VARCHAR(30) NOT NULL DEFAULT 'Medium',

    assigned_to INTEGER NOT NULL,

    created_by INTEGER NOT NULL,

    due_date TIMESTAMP,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    updated_at TIMESTAMP,

    CONSTRAINT fk_tasks_assigned_user
        FOREIGN KEY (assigned_to)
        REFERENCES users(id),

    CONSTRAINT fk_tasks_created_user
        FOREIGN KEY (created_by)
        REFERENCES users(id)
);


-- ============================================================
-- 3. TASK STATUS HISTORY
-- ============================================================

CREATE TABLE task_status_history
(
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    task_id INTEGER NOT NULL,

    old_status VARCHAR(30),

    new_status VARCHAR(30) NOT NULL,

    changed_by INTEGER NOT NULL,

    changed_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_status_history_task
        FOREIGN KEY (task_id)
        REFERENCES tasks(id),

    CONSTRAINT fk_status_history_user
        FOREIGN KEY (changed_by)
        REFERENCES users(id)
);


-- ============================================================
-- 4. TASK COMMENTS
-- ============================================================

CREATE TABLE task_comments
(
    id INTEGER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,

    task_id INTEGER NOT NULL,

    user_id INTEGER NOT NULL,

    comment TEXT NOT NULL,

    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT fk_task_comments_task
        FOREIGN KEY (task_id)
        REFERENCES tasks(id),

    CONSTRAINT fk_task_comments_user
        FOREIGN KEY (user_id)
        REFERENCES users(id)
);


-- ============================================================
-- 5. SAMPLE USERS
-- ============================================================

INSERT INTO users
(
    name,
    email,
    password_hash,
    role
)
VALUES
(
    'Admin User',
    'admin@taskmanagement.com',
    'TEMP',
    'Admin'
),
(
    'John Employee',
    'john@taskmanagement.com',
    'TEMP',
    'Employee'
),
(
    'Jane Employee',
    'jane@taskmanagement.com',
    'TEMP',
    'Employee'
);


-- ============================================================
-- 6. SAMPLE TASKS
-- ============================================================

INSERT INTO tasks
(
    title,
    description,
    status,
    priority,
    assigned_to,
    created_by,
    due_date
)
VALUES
(
    'Create Login API',
    'Develop login API using ASP.NET Core',
    'Completed',
    'High',
    2,
    1,
    CURRENT_TIMESTAMP + INTERVAL '2 days'
),
(
    'Build Task CRUD',
    'Create task management CRUD APIs',
    'In Progress',
    'High',
    2,
    1,
    CURRENT_TIMESTAMP + INTERVAL '5 days'
),
(
    'Create MVC Dashboard',
    'Develop dashboard UI using ASP.NET Core MVC',
    'Pending',
    'Medium',
    3,
    1,
    CURRENT_TIMESTAMP + INTERVAL '7 days'
);


-- ============================================================
-- 7. SAMPLE STATUS HISTORY
-- ============================================================

INSERT INTO task_status_history
(
    task_id,
    old_status,
    new_status,
    changed_by
)
VALUES
(
    1,
    'In Progress',
    'Completed',
    2
),
(
    2,
    'Pending',
    'In Progress',
    2
);


-- ============================================================
-- 8. SAMPLE COMMENTS
-- ============================================================

INSERT INTO task_comments
(
    task_id,
    user_id,
    comment
)
VALUES
(
    1,
    2,
    'Login API has been completed.'
),
(
    2,
    2,
    'Started working on the CRUD APIs.'
),
(
    3,
    3,
    'Dashboard development will start tomorrow.'
);


-- ============================================================
-- 9. TEST QUERY
-- ============================================================

SELECT
    t.id,
    t.title,
    t.status,
    t.priority,
    u1.name AS assigned_employee,
    u2.name AS created_by
FROM tasks t
INNER JOIN users u1
    ON t.assigned_to = u1.id
INNER JOIN users u2
    ON t.created_by = u2.id
ORDER BY t.id;


select * from users