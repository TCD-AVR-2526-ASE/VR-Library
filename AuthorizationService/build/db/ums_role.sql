create table ums_role
(
    id          bigserial
        primary key,
    name        varchar(100),
    description varchar(500),
    status      integer,
    sort        integer,
    code        varchar(10),
    create_by   varchar(50),
    create_time timestamp,
    update_by   varchar(50),
    update_time timestamp
);

alter table ums_role
    owner to root;

INSERT INTO public.ums_role (id, name, description, status, sort, code, create_by, create_time, update_by, update_time) VALUES (1, 'admin', 'he is the man', 1, 0, 'admin', null, null, null, null);
INSERT INTO public.ums_role (id, name, description, status, sort, code, create_by, create_time, update_by, update_time) VALUES (3, 'librarian', 'test', 0, null, 'LR', 'admin', '2026-01-29 16:25:08.498748', 'admin', '2026-01-29 16:25:08.498000');
INSERT INTO public.ums_role (id, name, description, status, sort, code, create_by, create_time, update_by, update_time) VALUES (4, 'member', 'member in the system', 1, 0, 'Member', 'admin', '2026-02-09 13:19:57.417786', 'admin', '2026-02-09 13:19:57.417786');

select setval('ums_admin_id_seq',5)
