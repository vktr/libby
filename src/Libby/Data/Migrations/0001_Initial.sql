create table libraries (
    id uuid not null primary key,
    name text not null,
    path text not null unique
);

create table library_items (
    id uuid not null primary key,
    library_id uuid not null references libraries(id),
    file_name text not null,
    file_size numeric not null,
    ffprobe_json jsonb,
    ffprobe_error text,
    ffprobe_date timestamptz
);

create table library_scans (
    correlation_id uuid not null primary key,
    current_state text not null,
    items_importing uuid[],
    items_imported uuid[],
    status_date timestamptz
);
