# ANSWERS.md

---

## 1. Schema Question

### Tables

```sql
-- applications
CREATE TABLE applications (
    id               UUID PRIMARY KEY,
    job_id           UUID NOT NULL REFERENCES jobs(id) ON DELETE CASCADE,
    candidate_name   VARCHAR(200) NOT NULL,
    candidate_email  VARCHAR(200) NOT NULL,
    cover_letter     TEXT,
    stage            VARCHAR(50)  NOT NULL DEFAULT 'Applied',
    applied_at       TIMESTAMPTZ  NOT NULL,

    CONSTRAINT uq_application_job_email UNIQUE (job_id, candidate_email)
);

CREATE INDEX ix_applications_job_stage ON applications (job_id, stage);


-- application_notes
CREATE TABLE application_notes (
    id               UUID PRIMARY KEY,
    application_id   UUID        NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    type             VARCHAR(50) NOT NULL,
    description      TEXT        NOT NULL,
    created_by       UUID        NOT NULL REFERENCES team_members(id),
    created_at       TIMESTAMPTZ NOT NULL
);

-- Loaded ordered by date on the profile endpoint
CREATE INDEX ix_notes_application_created ON application_notes (application_id, created_at DESC);


-- application_scores
-- One row per (application, dimension). Re-submitting overwrites the row.
CREATE TABLE application_scores (
    id               UUID PRIMARY KEY,
    application_id   UUID        NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    dimension        VARCHAR(50) NOT NULL,  -- CultureFit | Interview | Assessment
    score            INT         NOT NULL CHECK (score BETWEEN 1 AND 5),
    comment          TEXT,
    scored_by        UUID        NOT NULL REFERENCES team_members(id),
    scored_at        TIMESTAMPTZ NOT NULL,

    CONSTRAINT uq_score_application_dimension UNIQUE (application_id, dimension)
);


-- stage_history
CREATE TABLE stage_history (
    id               UUID PRIMARY KEY,
    application_id   UUID        NOT NULL REFERENCES applications(id) ON DELETE CASCADE,
    from_stage       VARCHAR(50) NOT NULL,
    to_stage         VARCHAR(50) NOT NULL,
    changed_by       UUID        NOT NULL REFERENCES team_members(id),
    changed_at       TIMESTAMPTZ NOT NULL,
    reason           TEXT
);

CREATE INDEX ix_stage_history_application ON stage_history (application_id, changed_at ASC);
```

### Why these indexes?

| Index                                               | Reason                                                                     |
| --------------------------------------------------- | -------------------------------------------------------------------------- |
| `(job_id, candidate_email)` UNIQUE                  | Enforces the duplicate-application rule at the DB level — not just in code |
| `(job_id, stage)`                                   | `GET /api/jobs/{id}/applications?stage=screening` filters on both columns  |
| `(application_id, created_at DESC)` on notes        | Profile endpoint reads notes newest-first; this avoids a sort              |
| `(application_id, changed_at ASC)` on stage_history | Profile endpoint reads history oldest-first; same reason                   |
| `(application_id, dimension)` UNIQUE on scores      | Enforces one score per dimension per candidate at the DB level             |

### GET /api/applications/{id} — how many round-trips?

**One.** EF Core translates the `Include().ThenInclude()` chain into a single SQL query with LEFT JOINs:

```sql
SELECT a.*, j.title,
       n.*, nm.name  AS author_name,
       s.*, sm.name  AS scorer_name,
       h.*, hm.name  AS changer_name
FROM   applications a
JOIN   jobs j          ON j.id = a.job_id
LEFT JOIN application_notes n  ON n.application_id = a.id
LEFT JOIN team_members nm      ON nm.id = n.created_by
LEFT JOIN application_scores s ON s.application_id = a.id
LEFT JOIN team_members sm      ON sm.id = s.scored_by
LEFT JOIN stage_history h      ON h.application_id = a.id
LEFT JOIN team_members hm      ON hm.id = h.changed_by
WHERE  a.id = $1;
```

All related data — job title, notes with author names, scores with scorer names, stage history with changer names — comes back in one network round-trip. EF then maps the flat rows into the nested object graph in memory.

---

## 2. Scoring Design Trade-off

### (a) Three separate endpoints vs. one generic endpoint

**Why three separate endpoints is better:**

- **Granular permissions.** In a real ATS you might only allow the interviewer to score the Interview dimension. Three endpoints make it easy to add that rule per endpoint without a conditional inside a shared handler.
- **Clearer intent.** `PUT /scores/culture-fit` is self-documenting. A client can't accidentally submit the wrong dimension key.
- **Independent validation.** Each dimension could have different rules in future (e.g. assessment score requires a comment, culture-fit does not). Three endpoints makes that extension straightforward.
- **Audit clarity.** Logs and error messages reference a specific dimension by URL, not a field inside a body.

**When one generic endpoint would be better:**

- When a recruiter scores all three dimensions in a single review session and you want to save them atomically — one request, one DB transaction, all-or-nothing.
- When minimising HTTP round-trips matters (e.g. mobile clients on slow connections).
- When all three dimensions will always have the same validation rules and no per-dimension logic is ever expected.

### (b) If product said "we need score history"

**Schema change — stop overwriting, start appending:**

```sql
-- Rename the unique constraint to allow multiple rows per (application, dimension)
-- Add a superseded_at column so we know which row is current

ALTER TABLE application_scores
    DROP CONSTRAINT uq_score_application_dimension;

ALTER TABLE application_scores
    ADD COLUMN superseded_at TIMESTAMPTZ NULL;

-- The current score is the one where superseded_at IS NULL
CREATE INDEX ix_scores_current ON application_scores (application_id, dimension)
    WHERE superseded_at IS NULL;
```

On re-submit: mark the existing row `superseded_at = NOW()` then insert a new row. The "current" score is always `WHERE superseded_at IS NULL`.

**Endpoint change — minimal.** The three PUT endpoints keep the same URL and request shape. Internally `ScoreService.UpsertAsync` changes from overwrite to append. The GET profile endpoint adds a filter `WHERE superseded_at IS NULL` to return only current scores. A new optional endpoint `GET /api/applications/{id}/scores/{dimension}/history` could expose the full history if the UI needs it — but the existing endpoints don't need to change.

---

## 3. Debugging Question

A recruiter says: _"I moved a candidate to Interview yesterday and today the system still shows Screening."_

- **Check the stage_history table first.** Query `SELECT * FROM stage_history WHERE application_id = $id ORDER BY changed_at DESC`. If a row for `Screening → Interview` exists with yesterday's timestamp, the move was saved — the problem is in the read path or the UI cache.

- **If no history row exists**, the PATCH request never completed successfully. Check the API logs around yesterday's timestamp for that application ID — look for any `4xx` or `5xx` response, or a timeout.

- **Check the application row itself.** `SELECT stage FROM applications WHERE id = $id`. This is the ground truth. If it says `Interview`, the DB is correct and the bug is in the frontend.

- **Check the browser's network tab** (ask the recruiter to reproduce or check session history). Was the PATCH request sent? What HTTP status came back? A `400` means the transition was rejected; a network failure means it never reached the server.

- **Check the X-Team-Member-Id header** on the request. If the header was missing or the GUID was invalid, the filter would have returned `400` before the move happened. The recruiter might have seen the page appear to succeed if error handling on the frontend was swallowing the response.

- **Check for a race condition.** Was there another PATCH that moved the application back? Query stage_history for all rows on that application — a `Interview → Screening` row would explain the current state.

- **Check browser/CDN cache.** If the frontend cached the profile response, the GET may be returning a stale snapshot. Ask the recruiter to hard-refresh or check if the API response has cache headers that shouldn't be there.

- **Confirm timezone.** The recruiter said "yesterday." Confirm they're reading `changed_at` in their local timezone — UTC timestamps displayed without conversion can make a same-day change appear to be from the previous day.

- **If nothing explains it**, check if the migration ran cleanly on the deployed environment — it's possible the deployed schema differs from local (e.g. the stage column didn't get the right default, or an old migration is still pending).

---

## 4. Honest Self-Assessment

| Skill               | Rating | Note                                                                                                                                                                               |
| ------------------- | ------ | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **C#**              | 3/5    | Two years of Java/Spring Boot; .NET idioms are new to me — this project is my first real exposure.                                                                                 |
| **SQL**             | 3/5    | Comfortable writing queries, designing schemas, and reasoning about indexes; less experienced with advanced features like window functions or query plan analysis.                 |
| **Git**             | 4/5    | Use it daily — branching, rebasing, PRs, resolving conflicts — but haven't used it in large monorepo or complex release workflows.                                                 |
| **REST API design** | 4/5    | Solid understanding of resource modelling, HTTP semantics, status codes, and pagination; this is where most of my Spring Boot experience transfers directly.                       |
| **Writing tests**   | 3/5    | Comfortable with unit tests and the thinking behind what makes a test meaningful; integration testing with real containers is newer territory that I learned through this project. |
