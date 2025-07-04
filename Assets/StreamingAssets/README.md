# StreamingAssets Directory

This directory contains files that need to be included in the Unity build and accessible at runtime.

## Required Files for Hall of History Feature

### TopEvents.db
This SQLite database file is required for the Hall of History feature to display historical events alongside family photos in the timeline.

**File Location:** `Assets/StreamingAssets/TopEvents.db`

**Database Schema:**
The database should contain a table called `topEvents` with the following columns:

```sql
CREATE TABLE topEvents (
    id INTEGER PRIMARY KEY,
    year TEXT,
    linkCount INTEGER,
    item TEXT,
    itemLabel TEXT,
    picture TEXT,
    wikiLink TEXT,
    description TEXT,
    aliases TEXT,
    locations TEXT,
    countries TEXT,
    pointInTime TEXT,
    eventStartDate TEXT,
    eventEndDate TEXT
);
```

**Data Requirements:**
- Events should have non-empty `description`, `itemLabel`, and `wikiLink` fields
- `linkCount` should be greater than 5 for quality filtering
- `year` field is used to match events to person's age/timeline
- `picture` field should contain URLs to historical images (optional)

**How to Obtain:**
1. **Wikidata Export**: You can create this database by exporting historical events from Wikidata
2. **Manual Creation**: Create your own database with historical events of interest
3. **Sample Data**: A sample database with major world events can be created for testing

**Example Data:**
```sql
INSERT INTO topEvents VALUES (
    1, '1969', 50, 'Q182436', 'Apollo 11', 
    'https://upload.wikimedia.org/wikipedia/commons/thumb/9/98/Aldrin_Apollo_11_original.jpg/256px-Aldrin_Apollo_11_original.jpg',
    'https://en.wikipedia.org/wiki/Apollo_11',
    'First crewed mission to land on the Moon',
    'Moon landing', 'Kennedy Space Center,Moon', 'United States',
    '1969-07-20T20:17:40Z', '1969-07-16', '1969-07-24'
);
```

## Feature Behavior

When the `TopEvents.db` file is missing:
- The Hall of History will still function but display empty panels
- A warning message will be logged: "TopEvents database file not found"
- No historical events will be shown alongside family photos

When the database is present:
- Historical events will be displayed on panels positioned to the right of the person's timeline
- Events are filtered by year to match the person's age
- Users can interact with event panels to view details and visit Wikipedia links

## Directory Structure

```
StreamingAssets/
├── README.md (this file)
└── TopEvents.db (required for historical events - not included)
```

## Notes

- The StreamingAssets folder is special in Unity - files here are copied to the build without processing
- Database files in this directory are accessible via `Application.streamingAssetsPath` at runtime
- This approach allows the application to work with different historical databases without recompiling 