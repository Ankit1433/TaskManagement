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

ALTER TABLE users
ADD COLUMN password_hash VARCHAR(255) NOT NULL DEFAULT 'TEMP';
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