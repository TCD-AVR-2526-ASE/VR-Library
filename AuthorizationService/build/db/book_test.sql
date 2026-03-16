create table book
(
    id          integer not null
        primary key,
    title       text,
    authors     jsonb,
    subjects    jsonb,
    bookshelves jsonb
);

CREATE EXTENSION IF NOT EXISTS pg_trgm;

CREATE INDEX idx_book_subjects_trgm ON book USING gin ((subjects::text) gin_trgm_ops);

CREATE INDEX idx_book_shelves_trgm ON book USING gin ((bookshelves::text) gin_trgm_ops);


SELECT id, title, authors, subjects
FROM book
WHERE subjects::text ILIKE '%Fiction%';


SELECT id, title, authors, subjects,bookshelves
FROM book
WHERE bookshelves::text ILIKE '%War%';



SELECT id, title, authors
FROM book
WHERE EXISTS (
    SELECT 1
    FROM jsonb_array_elements(authors) AS author
    WHERE author->>'name' ILIKE '%jackson%'
);

