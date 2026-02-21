create table ums_admin_role_relation
(
    id       bigserial
        primary key,
    admin_id bigint,
    role_id  bigint
);

alter table ums_admin_role_relation
    owner to root;

INSERT INTO public.ums_admin_role_relation (id, admin_id, role_id) VALUES (11, 9, 2);
INSERT INTO public.ums_admin_role_relation (id, admin_id, role_id) VALUES (16, 13, 4);
INSERT INTO public.ums_admin_role_relation (id, admin_id, role_id) VALUES (29, 1, 1);

select setval('ums_admin_id_seq',30)
