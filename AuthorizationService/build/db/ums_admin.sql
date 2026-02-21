create table ums_admin
(
    id          bigserial
        primary key,
    username    varchar(64),
    password    varchar(64),
    icon        varchar(500),
    email       varchar(100),
    nick_name   varchar(200),
    note        varchar(500),
    create_time timestamp(6),
    login_time  timestamp(6),
    status      integer,
    id_card     varchar(50),
    phone       varchar(20),
    position    varchar(50),
    contact     varchar(500),
    sort        integer,
    create_by   bigint,
    update_by   bigint,
    update_time timestamp
);

comment on table ums_admin is 'user table';

comment on column ums_admin.status is 'account status';

alter table ums_admin
    owner to root;

INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (6, 'gongxu2', '$2a$10$u5AJ72S/oZt9E7g8h.nO8.6yiPH3ObHKUeU.nsWcDfJ5twUJNU2a2', null, null, null, null, '2026-02-09 13:33:15.981657', null, 1, null, null, null, null, null, null, null, '2026-02-09 13:33:15.981657');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (7, 'daoqi2', '$2a$10$3JtDjUvdDsRXb0uk/ZDIKu7PLu1OdcxYpKYreaJBwaiDw78LsCHmC', null, null, null, null, '2026-02-10 11:07:26.402969', null, 1, null, null, null, null, null, null, null, '2026-02-11 10:29:51.451166');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (8, 'test', '$2a$10$t4Og52LjZg6maMuxyM6ixuG.3aqroUy7oHLP4yiOJYSKty9uDWu2a', null, null, null, null, '2026-02-12 15:54:47.514225', null, 1, null, null, null, null, null, null, null, '2026-02-12 15:54:47.514225');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (10, 'yoki', '$2a$10$WI.4zUAQ88wHtqxDo6Iw3eEhOFe6p4GZKLNOx9fvTHdeyZLQVNlLu', null, null, null, null, '2026-02-16 13:19:01.573460', null, 1, null, null, null, null, null, null, null, '2026-02-16 13:19:01.573460');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (11, 'test11', '$2a$10$09ZsLo6t3Mce8MJysVrcP.ZgJLJxECWo38fzX4cD4UWjXvrwPmojC', null, null, null, null, '2026-02-16 14:57:24.378552', null, 1, null, null, null, null, null, null, null, '2026-02-16 14:57:24.378552');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (9, 'pri', '$2a$10$rBB6ek3nNuwbp0ehf.DhA.9N/91OwBEg7xe13/HsGgtKki4AjeFoC', null, null, null, null, '2026-02-12 16:00:07.750924', null, 1, null, null, null, null, null, null, null, '2026-02-17 09:59:59.203857');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (12, 'peyman', '$2a$10$jYehOXmm3w2TL7yFP/zEPuQ3ANw89IxXxIc/.s8fQEQYnHWGGkiDm', null, null, null, null, '2026-02-17 11:12:11.660728', null, 1, null, null, null, null, null, null, null, '2026-02-19 13:01:52.459904');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (13, 'updated_name', '$2a$10$e7ZyBwCDhpleCfe//YPSKe71MsSpI5vEZ/IzLE6x168wUYykVe6Ee', null, null, null, null, '2026-02-19 13:19:43.812791', null, 1, null, null, null, null, null, null, null, '2026-02-19 13:27:19.189146');
INSERT INTO public.ums_admin (id, username, password, icon, email, nick_name, note, create_time, login_time, status, id_card, phone, position, contact, sort, create_by, update_by, update_time) VALUES (1, 'admin', '$2a$10$ZbxmXAYqKPFKVPMppmGvo.abMtFALUMG1cZZcFI390nXbkzLO9piO', '2023-08-02/90f05356-1258-4d16-90f6-2181e25d3eb1.JPG', '', 'System Manager', 'System Manager', '2026-02-09 13:33:15.981657', '2026-02-09 13:33:15.981657', 1, '420621199409217789', null, null, null, null, null, null, '2026-02-19 13:27:19.213147');

select setval('ums_admin_id_seq',20)
