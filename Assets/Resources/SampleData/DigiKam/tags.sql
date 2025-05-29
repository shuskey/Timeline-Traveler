-- RootsMagic to digiKam tag import
-- Generated: Thu May 29 13:25:13 2025

--
-- INSTRUCTIONS FOR IMPORTING TAGS INTO DIGIKAM:
--
-- 1. Close digiKam completely
-- 2. Locate your digiKam database file:
--    - Windows: Usually in %LOCALAPPDATA%/digikam/digikam4.db
--    - Linux: Usually in ~/.local/share/digikam/digikam4.db
--    - macOS: Usually in ~/Library/Application Support/digikam/digikam4.db
-- 3. Make a backup of your digikam4.db file!
-- 4. Use the sqlite3 command line tool to import this file:
--    sqlite3 path/to/digikam4.db ".read tags.sql"
-- 5. Start digiKam and verify the tags were imported correctly
--
-- Note: If you get any errors during import, restore from your backup
--       and check for any special characters in tag names.
-- Note: All people will be imported as tags under a 'RootsMagic' parent tag
--       (unless a different parent tag name was specified).
-- Note: A helper script may have been deployed to your digikam folder that contains something like:
-- .\rootsmagic_utils.exe -d '..\RootMagic\Kennedy.rmtree'
-- sqlite3 ".\digikam4.db" ".read tags.sql"
--

BEGIN TRANSACTION;

INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) VALUES ('RootsMagic', 0, NULL, NULL);

-- Create tag for: Joseph Partick Kennedy 1888-1969
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Joseph Partick Kennedy 1888-1969', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '1' FROM Tags t WHERE t.name='Joseph Partick Kennedy 1888-1969' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Joseph Partick Kennedy 1888-1969' FROM Tags t WHERE t.name='Joseph Partick Kennedy 1888-1969' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Joseph Patrick Kennedy 1915-1944
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Joseph Patrick Kennedy 1915-1944', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '2' FROM Tags t WHERE t.name='Joseph Patrick Kennedy 1915-1944' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Joseph Patrick Kennedy 1915-1944' FROM Tags t WHERE t.name='Joseph Patrick Kennedy 1915-1944' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Rose Marie Kennedy 1918-2005
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Rose Marie Kennedy 1918-2005', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '3' FROM Tags t WHERE t.name='Rose Marie Kennedy 1918-2005' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Rose Marie Kennedy 1918-2005' FROM Tags t WHERE t.name='Rose Marie Kennedy 1918-2005' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Kathleen Agnes Kennedy 1920-1948
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Kathleen Agnes Kennedy 1920-1948', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '4' FROM Tags t WHERE t.name='Kathleen Agnes Kennedy 1920-1948' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Kathleen Agnes Kennedy 1920-1948' FROM Tags t WHERE t.name='Kathleen Agnes Kennedy 1920-1948' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Edward Moore (Ted) Kennedy 1932-2009
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Edward Moore (Ted) Kennedy 1932-2009', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '5' FROM Tags t WHERE t.name='Edward Moore (Ted) Kennedy 1932-2009' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Edward Moore (Ted) Kennedy 1932-2009' FROM Tags t WHERE t.name='Edward Moore (Ted) Kennedy 1932-2009' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Victoria Anne Reggie 1954-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Victoria Anne Reggie 1954-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '6' FROM Tags t WHERE t.name='Victoria Anne Reggie 1954-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Victoria Anne Reggie 1954-unknown' FROM Tags t WHERE t.name='Victoria Anne Reggie 1954-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Virginia Joan Bennett unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Virginia Joan Bennett unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '7' FROM Tags t WHERE t.name='Virginia Joan Bennett unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Virginia Joan Bennett unknown-unknown' FROM Tags t WHERE t.name='Virginia Joan Bennett unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Fitzgerald Kennedy 1917-1963
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Fitzgerald Kennedy 1917-1963', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '8' FROM Tags t WHERE t.name='John Fitzgerald Kennedy 1917-1963' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Fitzgerald Kennedy 1917-1963' FROM Tags t WHERE t.name='John Fitzgerald Kennedy 1917-1963' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: William John Robert Cavendish 1917-1944
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'William John Robert Cavendish 1917-1944', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '9' FROM Tags t WHERE t.name='William John Robert Cavendish 1917-1944' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'William John Robert Cavendish 1917-1944' FROM Tags t WHERE t.name='William John Robert Cavendish 1917-1944' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Arabella Kennedy 1956-1956
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Arabella Kennedy 1956-1956', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '10' FROM Tags t WHERE t.name='Arabella Kennedy 1956-1956' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Arabella Kennedy 1956-1956' FROM Tags t WHERE t.name='Arabella Kennedy 1956-1956' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Bouvier Kennedy 1963-1963
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Bouvier Kennedy 1963-1963', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '11' FROM Tags t WHERE t.name='Patrick Bouvier Kennedy 1963-1963' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Bouvier Kennedy 1963-1963' FROM Tags t WHERE t.name='Patrick Bouvier Kennedy 1963-1963' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Augusta Hickey 1857-1923
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Augusta Hickey 1857-1923', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '12' FROM Tags t WHERE t.name='Mary Augusta Hickey 1857-1923' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Augusta Hickey 1857-1923' FROM Tags t WHERE t.name='Mary Augusta Hickey 1857-1923' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John F Kennedy 1960-1999
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John F Kennedy 1960-1999', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '13' FROM Tags t WHERE t.name='John F Kennedy 1960-1999' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John F Kennedy 1960-1999' FROM Tags t WHERE t.name='John F Kennedy 1960-1999' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Joseph Kennedy 1858-1929
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Joseph Kennedy 1858-1929', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '14' FROM Tags t WHERE t.name='Patrick Joseph Kennedy 1858-1929' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Joseph Kennedy 1858-1929' FROM Tags t WHERE t.name='Patrick Joseph Kennedy 1858-1929' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Jean Ann Kennedy 1928-2020
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Jean Ann Kennedy 1928-2020', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '15' FROM Tags t WHERE t.name='Jean Ann Kennedy 1928-2020' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Jean Ann Kennedy 1928-2020' FROM Tags t WHERE t.name='Jean Ann Kennedy 1928-2020' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Jacqueline Lee Bouvier 1929-1994
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Jacqueline Lee Bouvier 1929-1994', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '16' FROM Tags t WHERE t.name='Jacqueline Lee Bouvier 1929-1994' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Jacqueline Lee Bouvier 1929-1994' FROM Tags t WHERE t.name='Jacqueline Lee Bouvier 1929-1994' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Francis Benedict Kennedy 1891-1892
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Francis Benedict Kennedy 1891-1892', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '17' FROM Tags t WHERE t.name='Francis Benedict Kennedy 1891-1892' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Francis Benedict Kennedy 1891-1892' FROM Tags t WHERE t.name='Francis Benedict Kennedy 1891-1892' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Amanda Mary Smith 1967-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Amanda Mary Smith 1967-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '18' FROM Tags t WHERE t.name='Amanda Mary Smith 1967-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Amanda Mary Smith 1967-unknown' FROM Tags t WHERE t.name='Amanda Mary Smith 1967-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Kara Anne Kennedy 1960-2011
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Kara Anne Kennedy 1960-2011', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '19' FROM Tags t WHERE t.name='Kara Anne Kennedy 1960-2011' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Kara Anne Kennedy 1960-2011' FROM Tags t WHERE t.name='Kara Anne Kennedy 1960-2011' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Stephen Edward Smith 1957-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Stephen Edward Smith 1957-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '20' FROM Tags t WHERE t.name='Stephen Edward Smith 1957-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Stephen Edward Smith 1957-unknown' FROM Tags t WHERE t.name='Stephen Edward Smith 1957-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Grace  unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Grace  unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '21' FROM Tags t WHERE t.name='Grace  unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Grace  unknown-unknown' FROM Tags t WHERE t.name='Grace  unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Max  unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Max  unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '22' FROM Tags t WHERE t.name='Max  unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Max  unknown-unknown' FROM Tags t WHERE t.name='Max  unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: William Kennedy Smith 1960-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'William Kennedy Smith 1960-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '23' FROM Tags t WHERE t.name='William Kennedy Smith 1960-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'William Kennedy Smith 1960-unknown' FROM Tags t WHERE t.name='William Kennedy Smith 1960-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Loretta Kennedy 1892-1972
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Loretta Kennedy 1892-1972', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '24' FROM Tags t WHERE t.name='Mary Loretta Kennedy 1892-1972' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Loretta Kennedy 1892-1972' FROM Tags t WHERE t.name='Mary Loretta Kennedy 1892-1972' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Kym Maria Smith 1972-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Kym Maria Smith 1972-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '25' FROM Tags t WHERE t.name='Kym Maria Smith 1972-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Kym Maria Smith 1972-unknown' FROM Tags t WHERE t.name='Kym Maria Smith 1972-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Louise Kennedy 1898-1974
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Louise Kennedy 1898-1974', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '26' FROM Tags t WHERE t.name='Margaret Louise Kennedy 1898-1974' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Louise Kennedy 1898-1974' FROM Tags t WHERE t.name='Margaret Louise Kennedy 1898-1974' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Robert Francis Kennedy 1925-1968
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Robert Francis Kennedy 1925-1968', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '27' FROM Tags t WHERE t.name='Robert Francis Kennedy 1925-1968' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Robert Francis Kennedy 1925-1968' FROM Tags t WHERE t.name='Robert Francis Kennedy 1925-1968' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patricia Helen Kennedy 1924-2006
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patricia Helen Kennedy 1924-2006', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '28' FROM Tags t WHERE t.name='Patricia Helen Kennedy 1924-2006' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patricia Helen Kennedy 1924-2006' FROM Tags t WHERE t.name='Patricia Helen Kennedy 1924-2006' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael Allen unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael Allen unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '29' FROM Tags t WHERE t.name='Michael Allen unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael Allen unknown-unknown' FROM Tags t WHERE t.name='Michael Allen unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: George William Connelly 1898-1971
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'George William Connelly 1898-1971', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '30' FROM Tags t WHERE t.name='George William Connelly 1898-1971' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'George William Connelly 1898-1971' FROM Tags t WHERE t.name='George William Connelly 1898-1971' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Ethel Skakel 1928-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Ethel Skakel 1928-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '31' FROM Tags t WHERE t.name='Ethel Skakel 1928-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Ethel Skakel 1928-unknown' FROM Tags t WHERE t.name='Ethel Skakel 1928-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Rose Elizabeth Fitzgerald 1890-1995
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Rose Elizabeth Fitzgerald 1890-1995', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '32' FROM Tags t WHERE t.name='Rose Elizabeth Fitzgerald 1890-1995' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Rose Elizabeth Fitzgerald 1890-1995' FROM Tags t WHERE t.name='Rose Elizabeth Fitzgerald 1890-1995' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Eunice Mary Kennedy 1921-2009
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Eunice Mary Kennedy 1921-2009', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '33' FROM Tags t WHERE t.name='Eunice Mary Kennedy 1921-2009' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Eunice Mary Kennedy 1921-2009' FROM Tags t WHERE t.name='Eunice Mary Kennedy 1921-2009' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Charles Joseph Burke 1899-1967
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Charles Joseph Burke 1899-1967', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '34' FROM Tags t WHERE t.name='Charles Joseph Burke 1899-1967' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Charles Joseph Burke 1899-1967' FROM Tags t WHERE t.name='Charles Joseph Burke 1899-1967' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Sydney Maleia Kennedy Lawford 1956-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Sydney Maleia Kennedy Lawford 1956-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '35' FROM Tags t WHERE t.name='Sydney Maleia Kennedy Lawford 1956-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Sydney Maleia Kennedy Lawford 1956-unknown' FROM Tags t WHERE t.name='Sydney Maleia Kennedy Lawford 1956-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Victoria Francis Lawford 1958-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Victoria Francis Lawford 1958-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '36' FROM Tags t WHERE t.name='Victoria Francis Lawford 1958-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Victoria Francis Lawford 1958-unknown' FROM Tags t WHERE t.name='Victoria Francis Lawford 1958-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Robin Elizabeth Lawford 1961-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Robin Elizabeth Lawford 1961-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '37' FROM Tags t WHERE t.name='Robin Elizabeth Lawford 1961-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Robin Elizabeth Lawford 1961-unknown' FROM Tags t WHERE t.name='Robin Elizabeth Lawford 1961-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Martha Field 1836-1911
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Martha Field 1836-1911', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '38' FROM Tags t WHERE t.name='Margaret Martha Field 1836-1911' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Martha Field 1836-1911' FROM Tags t WHERE t.name='Margaret Martha Field 1836-1911' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Carolyn Jeanne Bessette 1966-1999
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Carolyn Jeanne Bessette 1966-1999', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '39' FROM Tags t WHERE t.name='Carolyn Jeanne Bessette 1966-1999' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Carolyn Jeanne Bessette 1966-1999' FROM Tags t WHERE t.name='Carolyn Jeanne Bessette 1966-1999' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Kennedy 1823-1858
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Kennedy 1823-1858', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '40' FROM Tags t WHERE t.name='Patrick Kennedy 1823-1858' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Kennedy 1823-1858' FROM Tags t WHERE t.name='Patrick Kennedy 1823-1858' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Bridget Murphy 1821-1888
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Bridget Murphy 1821-1888', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '41' FROM Tags t WHERE t.name='Bridget Murphy 1821-1888' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Bridget Murphy 1821-1888' FROM Tags t WHERE t.name='Bridget Murphy 1821-1888' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Stephen Edward "Steve" Smith 1927-1990
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Stephen Edward "Steve" Smith 1927-1990', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '42' FROM Tags t WHERE t.name='Stephen Edward "Steve" Smith 1927-1990' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Stephen Edward "Steve" Smith 1927-1990' FROM Tags t WHERE t.name='Stephen Edward "Steve" Smith 1927-1990' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Christopher Kennedy Lawford 1955-2018
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Christopher Kennedy Lawford 1955-2018', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '43' FROM Tags t WHERE t.name='Christopher Kennedy Lawford 1955-2018' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Christopher Kennedy Lawford 1955-2018' FROM Tags t WHERE t.name='Christopher Kennedy Lawford 1955-2018' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Robert Sargent "Bobby" Shriver 1954-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Robert Sargent "Bobby" Shriver 1954-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '44' FROM Tags t WHERE t.name='Robert Sargent "Bobby" Shriver 1954-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Robert Sargent "Bobby" Shriver 1954-unknown' FROM Tags t WHERE t.name='Robert Sargent "Bobby" Shriver 1954-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Timothy Perry Shriver 1959-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Timothy Perry Shriver 1959-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '45' FROM Tags t WHERE t.name='Timothy Perry Shriver 1959-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Timothy Perry Shriver 1959-unknown' FROM Tags t WHERE t.name='Timothy Perry Shriver 1959-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mark Kennedy Shriver 1964-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mark Kennedy Shriver 1964-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '46' FROM Tags t WHERE t.name='Mark Kennedy Shriver 1964-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mark Kennedy Shriver 1964-unknown' FROM Tags t WHERE t.name='Mark Kennedy Shriver 1964-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Anthony Paul Kennedy Shriver 1965-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Anthony Paul Kennedy Shriver 1965-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '47' FROM Tags t WHERE t.name='Anthony Paul Kennedy Shriver 1965-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Anthony Paul Kennedy Shriver 1965-unknown' FROM Tags t WHERE t.name='Anthony Paul Kennedy Shriver 1965-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Hickey 1867-1867
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Hickey 1867-1867', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '48' FROM Tags t WHERE t.name='Margaret Hickey 1867-1867' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Hickey 1867-1867' FROM Tags t WHERE t.name='Margaret Hickey 1867-1867' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Hickey 1836-1900
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Hickey 1836-1900', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '49' FROM Tags t WHERE t.name='James Hickey 1836-1900' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Hickey 1836-1900' FROM Tags t WHERE t.name='James Hickey 1836-1900' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John A. Hickey 1866-1934
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John A. Hickey 1866-1934', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '50' FROM Tags t WHERE t.name='John A. Hickey 1866-1934' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John A. Hickey 1866-1934' FROM Tags t WHERE t.name='John A. Hickey 1866-1934' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Catharine Frances Hickey 1868-1947
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Catharine Frances Hickey 1868-1947', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '51' FROM Tags t WHERE t.name='Catharine Frances Hickey 1868-1947' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Catharine Frances Hickey 1868-1947' FROM Tags t WHERE t.name='Catharine Frances Hickey 1868-1947' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Martha Hickey 1870-1902
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Martha Hickey 1870-1902', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '52' FROM Tags t WHERE t.name='Margaret Martha Hickey 1870-1902' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Martha Hickey 1870-1902' FROM Tags t WHERE t.name='Margaret Martha Hickey 1870-1902' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Kennedy 1854-1855
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Kennedy 1854-1855', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '53' FROM Tags t WHERE t.name='John Kennedy 1854-1855' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Kennedy 1854-1855' FROM Tags t WHERE t.name='John Kennedy 1854-1855' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Barron 1800-1860
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Barron 1800-1860', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '54' FROM Tags t WHERE t.name='Mary Barron 1800-1860' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Barron 1800-1860' FROM Tags t WHERE t.name='Mary Barron 1800-1860' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Sheehy 1788-1880
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Sheehy 1788-1880', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '55' FROM Tags t WHERE t.name='Mary Sheehy 1788-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Sheehy 1788-1880' FROM Tags t WHERE t.name='Mary Sheehy 1788-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James F. Hickey 1861-1922
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James F. Hickey 1861-1922', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '56' FROM Tags t WHERE t.name='James F. Hickey 1861-1922' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James F. Hickey 1861-1922' FROM Tags t WHERE t.name='James F. Hickey 1861-1922' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Richard Murphy 1780-1845
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Richard Murphy 1780-1845', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '57' FROM Tags t WHERE t.name='Richard Murphy 1780-1845' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Richard Murphy 1780-1845' FROM Tags t WHERE t.name='Richard Murphy 1780-1845' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Field 1786-1857
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Field 1786-1857', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '58' FROM Tags t WHERE t.name='Patrick Field 1786-1857' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Field 1786-1857' FROM Tags t WHERE t.name='Patrick Field 1786-1857' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Kennedy 1770-1835
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Kennedy 1770-1835', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '59' FROM Tags t WHERE t.name='James Kennedy 1770-1835' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Kennedy 1770-1835' FROM Tags t WHERE t.name='James Kennedy 1770-1835' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael Fields 1835-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael Fields 1835-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '60' FROM Tags t WHERE t.name='Michael Fields 1835-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael Fields 1835-unknown' FROM Tags t WHERE t.name='Michael Fields 1835-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Julia Fields 1847-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Julia Fields 1847-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '61' FROM Tags t WHERE t.name='Julia Fields 1847-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Julia Fields 1847-unknown' FROM Tags t WHERE t.name='Julia Fields 1847-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Robert Sargent Shriver Jr 1915-2011
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Robert Sargent Shriver Jr 1915-2011', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '62' FROM Tags t WHERE t.name='Robert Sargent Shriver Jr 1915-2011' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Robert Sargent Shriver Jr 1915-2011' FROM Tags t WHERE t.name='Robert Sargent Shriver Jr 1915-2011' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Catherine Hasset 1792-1870
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Catherine Hasset 1792-1870', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '63' FROM Tags t WHERE t.name='Catherine Hasset 1792-1870' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Catherine Hasset 1792-1870' FROM Tags t WHERE t.name='Catherine Hasset 1792-1870' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Fields 1833-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Fields 1833-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '64' FROM Tags t WHERE t.name='Patrick Fields 1833-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Fields 1833-unknown' FROM Tags t WHERE t.name='Patrick Fields 1833-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Peter Sydney Ernest Lawford 1923-1984
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Peter Sydney Ernest Lawford 1923-1984', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '65' FROM Tags t WHERE t.name='Peter Sydney Ernest Lawford 1923-1984' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Peter Sydney Ernest Lawford 1923-1984' FROM Tags t WHERE t.name='Peter Sydney Ernest Lawford 1923-1984' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael John Hickey 1791-1870
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael John Hickey 1791-1870', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '66' FROM Tags t WHERE t.name='Michael John Hickey 1791-1870' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael John Hickey 1791-1870' FROM Tags t WHERE t.name='Michael John Hickey 1791-1870' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Murphy 1835-1880
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Murphy 1835-1880', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '67' FROM Tags t WHERE t.name='Margaret Murphy 1835-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Murphy 1835-1880' FROM Tags t WHERE t.name='Margaret Murphy 1835-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Joanna Davis 1779-1835
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Joanna Davis 1779-1835', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '68' FROM Tags t WHERE t.name='Mary Joanna Davis 1779-1835' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Joanna Davis 1779-1835' FROM Tags t WHERE t.name='Mary Joanna Davis 1779-1835' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Charles Michael Hickey 1859-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Charles Michael Hickey 1859-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '69' FROM Tags t WHERE t.name='Charles Michael Hickey 1859-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Charles Michael Hickey 1859-unknown' FROM Tags t WHERE t.name='Charles Michael Hickey 1859-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret M. Kennedy 1855-1929
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret M. Kennedy 1855-1929', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '70' FROM Tags t WHERE t.name='Margaret M. Kennedy 1855-1929' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret M. Kennedy 1855-1929' FROM Tags t WHERE t.name='Margaret M. Kennedy 1855-1929' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary L Kennedy 1851-1926
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary L Kennedy 1851-1926', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '71' FROM Tags t WHERE t.name='Mary L Kennedy 1851-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary L Kennedy 1851-1926' FROM Tags t WHERE t.name='Mary L Kennedy 1851-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Charles Hickey unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Charles Hickey unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '72' FROM Tags t WHERE t.name='Charles Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Charles Hickey unknown-unknown' FROM Tags t WHERE t.name='Charles Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Thomas Hickey unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Thomas Hickey unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '73' FROM Tags t WHERE t.name='Thomas Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Thomas Hickey unknown-unknown' FROM Tags t WHERE t.name='Thomas Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Sally Orpha Moore unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Sally Orpha Moore unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '74' FROM Tags t WHERE t.name='Sally Orpha Moore unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Sally Orpha Moore unknown-unknown' FROM Tags t WHERE t.name='Sally Orpha Moore unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Hickey unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Hickey unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '75' FROM Tags t WHERE t.name='John Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Hickey unknown-unknown' FROM Tags t WHERE t.name='John Hickey unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for:  Davis 1765-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT ' Davis 1765-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '76' FROM Tags t WHERE t.name=' Davis 1765-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', ' Davis 1765-unknown' FROM Tags t WHERE t.name=' Davis 1765-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Sheehan unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Sheehan unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '77' FROM Tags t WHERE t.name='Mary Sheehan unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Sheehan unknown-unknown' FROM Tags t WHERE t.name='Mary Sheehan unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Cleary 1832-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Cleary 1832-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '78' FROM Tags t WHERE t.name='Patrick Cleary 1832-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Cleary 1832-unknown' FROM Tags t WHERE t.name='Patrick Cleary 1832-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Katherine Hickey 1830-1905
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Katherine Hickey 1830-1905', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '79' FROM Tags t WHERE t.name='Katherine Hickey 1830-1905' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Katherine Hickey 1830-1905' FROM Tags t WHERE t.name='Katherine Hickey 1830-1905' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Alicia Sexton 1766-1821
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Alicia Sexton 1766-1821', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '80' FROM Tags t WHERE t.name='Alicia Sexton 1766-1821' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Alicia Sexton 1766-1821' FROM Tags t WHERE t.name='Alicia Sexton 1766-1821' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Charles Hickey 1760-1828
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Charles Hickey 1760-1828', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '81' FROM Tags t WHERE t.name='John Charles Hickey 1760-1828' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Charles Hickey 1760-1828' FROM Tags t WHERE t.name='John Charles Hickey 1760-1828' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Thomas Hickey 1800-1845
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Thomas Hickey 1800-1845', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '82' FROM Tags t WHERE t.name='Thomas Hickey 1800-1845' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Thomas Hickey 1800-1845' FROM Tags t WHERE t.name='Thomas Hickey 1800-1845' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Thomas Morony 1832-1907
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Thomas Morony 1832-1907', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '83' FROM Tags t WHERE t.name='Thomas Morony 1832-1907' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Thomas Morony 1832-1907' FROM Tags t WHERE t.name='Thomas Morony 1832-1907' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Ann Murphy 1834-1893
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Ann Murphy 1834-1893', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '84' FROM Tags t WHERE t.name='Ann Murphy 1834-1893' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Ann Murphy 1834-1893' FROM Tags t WHERE t.name='Ann Murphy 1834-1893' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Joanna L Kennedy 1852-1926
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Joanna L Kennedy 1852-1926', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '85' FROM Tags t WHERE t.name='Joanna L Kennedy 1852-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Joanna L Kennedy 1852-1926' FROM Tags t WHERE t.name='Joanna L Kennedy 1852-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Kennedy 1816-1881
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Kennedy 1816-1881', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '86' FROM Tags t WHERE t.name='James Kennedy 1816-1881' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Kennedy 1816-1881' FROM Tags t WHERE t.name='James Kennedy 1816-1881' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Hickey 1819-1894
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Hickey 1819-1894', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '87' FROM Tags t WHERE t.name='Patrick Hickey 1819-1894' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Hickey 1819-1894' FROM Tags t WHERE t.name='Patrick Hickey 1819-1894' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: William Kennedy 1826-1900
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'William Kennedy 1826-1900', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '88' FROM Tags t WHERE t.name='William Kennedy 1826-1900' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'William Kennedy 1826-1900' FROM Tags t WHERE t.name='William Kennedy 1826-1900' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Hannah Murphy 1858-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Hannah Murphy 1858-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '89' FROM Tags t WHERE t.name='Hannah Murphy 1858-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Hannah Murphy 1858-unknown' FROM Tags t WHERE t.name='Hannah Murphy 1858-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Fieldss 1845-1915
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Fieldss 1845-1915', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '90' FROM Tags t WHERE t.name='Mary Fieldss 1845-1915' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Fieldss 1845-1915' FROM Tags t WHERE t.name='Mary Fieldss 1845-1915' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Richard Kennedy 1706-1803
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Richard Kennedy 1706-1803', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '91' FROM Tags t WHERE t.name='Richard Kennedy 1706-1803' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Richard Kennedy 1706-1803' FROM Tags t WHERE t.name='Richard Kennedy 1706-1803' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Bridget Swallow 1744-1774
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Bridget Swallow 1744-1774', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '92' FROM Tags t WHERE t.name='Bridget Swallow 1744-1774' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Bridget Swallow 1744-1774' FROM Tags t WHERE t.name='Bridget Swallow 1744-1774' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Kennedy 1804-1864
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Kennedy 1804-1864', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '93' FROM Tags t WHERE t.name='John Kennedy 1804-1864' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Kennedy 1804-1864' FROM Tags t WHERE t.name='John Kennedy 1804-1864' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Kennedy 1820-1898
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Kennedy 1820-1898', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '94' FROM Tags t WHERE t.name='Mary Kennedy 1820-1898' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Kennedy 1820-1898' FROM Tags t WHERE t.name='Mary Kennedy 1820-1898' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael Hickey 1823-1893
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael Hickey 1823-1893', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '95' FROM Tags t WHERE t.name='Michael Hickey 1823-1893' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael Hickey 1823-1893' FROM Tags t WHERE t.name='Michael Hickey 1823-1893' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Lawrence M Kane 1858-1905
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Lawrence M Kane 1858-1905', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '96' FROM Tags t WHERE t.name='Lawrence M Kane 1858-1905' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Lawrence M Kane 1858-1905' FROM Tags t WHERE t.name='Lawrence M Kane 1858-1905' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Thomas Caufield 1861-1937
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Thomas Caufield 1861-1937', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '97' FROM Tags t WHERE t.name='John Thomas Caufield 1861-1937' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Thomas Caufield 1861-1937' FROM Tags t WHERE t.name='John Thomas Caufield 1861-1937' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Fanny Zillhart Owens Kennedy 1759-1854
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Fanny Zillhart Owens Kennedy 1759-1854', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '98' FROM Tags t WHERE t.name='Fanny Zillhart Owens Kennedy 1759-1854' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Fanny Zillhart Owens Kennedy 1759-1854' FROM Tags t WHERE t.name='Fanny Zillhart Owens Kennedy 1759-1854' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Kennedy 1766-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Kennedy 1766-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '99' FROM Tags t WHERE t.name='Mary Kennedy 1766-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Kennedy 1766-unknown' FROM Tags t WHERE t.name='Mary Kennedy 1766-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Catherine Colfer 1819-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Catherine Colfer 1819-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '100' FROM Tags t WHERE t.name='Catherine Colfer 1819-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Catherine Colfer 1819-unknown' FROM Tags t WHERE t.name='Catherine Colfer 1819-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Parden unknown-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Parden unknown-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '101' FROM Tags t WHERE t.name='Margaret Parden unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Parden unknown-unknown' FROM Tags t WHERE t.name='Margaret Parden unknown-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Catherine  1834-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Catherine  1834-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '102' FROM Tags t WHERE t.name='Catherine  1834-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Catherine  1834-unknown' FROM Tags t WHERE t.name='Catherine  1834-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Elizabeth Fields 1838-1926
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Elizabeth Fields 1838-1926', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '103' FROM Tags t WHERE t.name='Mary Elizabeth Fields 1838-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Elizabeth Fields 1838-1926' FROM Tags t WHERE t.name='Mary Elizabeth Fields 1838-1926' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Swallow Kennedy 1758-1823
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Swallow Kennedy 1758-1823', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '104' FROM Tags t WHERE t.name='John Swallow Kennedy 1758-1823' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Swallow Kennedy 1758-1823' FROM Tags t WHERE t.name='John Swallow Kennedy 1758-1823' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Kennedy 1794-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Kennedy 1794-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '105' FROM Tags t WHERE t.name='Patrick Kennedy 1794-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Kennedy 1794-unknown' FROM Tags t WHERE t.name='Patrick Kennedy 1794-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Julia O'Neil 1839-1910
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Julia O''Neil 1839-1910', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '106' FROM Tags t WHERE t.name='Julia O''Neil 1839-1910' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Julia O''Neil 1839-1910' FROM Tags t WHERE t.name='Julia O''Neil 1839-1910' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael Keys 1822-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael Keys 1822-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '107' FROM Tags t WHERE t.name='Michael Keys 1822-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael Keys 1822-unknown' FROM Tags t WHERE t.name='Michael Keys 1822-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Dennis Hickey 1802-1863
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Dennis Hickey 1802-1863', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '108' FROM Tags t WHERE t.name='Dennis Hickey 1802-1863' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Dennis Hickey 1802-1863' FROM Tags t WHERE t.name='Dennis Hickey 1802-1863' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Kennedy 1773-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Kennedy 1773-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '109' FROM Tags t WHERE t.name='Mary Kennedy 1773-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Kennedy 1773-unknown' FROM Tags t WHERE t.name='Mary Kennedy 1773-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Molloy 1912-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Molloy 1912-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '110' FROM Tags t WHERE t.name='James Molloy 1912-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Molloy 1912-unknown' FROM Tags t WHERE t.name='James Molloy 1912-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Cahill Collins 1775-1805
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Cahill Collins 1775-1805', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '111' FROM Tags t WHERE t.name='Margaret Cahill Collins 1775-1805' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Cahill Collins 1775-1805' FROM Tags t WHERE t.name='Margaret Cahill Collins 1775-1805' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Kennedy 1760-1809
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Kennedy 1760-1809', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '112' FROM Tags t WHERE t.name='James Kennedy 1760-1809' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Kennedy 1760-1809' FROM Tags t WHERE t.name='James Kennedy 1760-1809' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Susannah Fisher 1836-1910
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Susannah Fisher 1836-1910', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '113' FROM Tags t WHERE t.name='Susannah Fisher 1836-1910' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Susannah Fisher 1836-1910' FROM Tags t WHERE t.name='Susannah Fisher 1836-1910' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Kennedy 1769-1836
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Kennedy 1769-1836', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '114' FROM Tags t WHERE t.name='John Kennedy 1769-1836' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Kennedy 1769-1836' FROM Tags t WHERE t.name='John Kennedy 1769-1836' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Gunnip 1816-1881
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Gunnip 1816-1881', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '115' FROM Tags t WHERE t.name='Mary Gunnip 1816-1881' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Gunnip 1816-1881' FROM Tags t WHERE t.name='Mary Gunnip 1816-1881' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick I Kennedy 1760-1824
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick I Kennedy 1760-1824', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '116' FROM Tags t WHERE t.name='Patrick I Kennedy 1760-1824' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick I Kennedy 1760-1824' FROM Tags t WHERE t.name='Patrick I Kennedy 1760-1824' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Eliza O'Day 1823-1877
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Eliza O''Day 1823-1877', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '117' FROM Tags t WHERE t.name='Eliza O''Day 1823-1877' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Eliza O''Day 1823-1877' FROM Tags t WHERE t.name='Eliza O''Day 1823-1877' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Patrick Kennedy 1765-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Patrick Kennedy 1765-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '118' FROM Tags t WHERE t.name='Patrick Kennedy 1765-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Patrick Kennedy 1765-unknown' FROM Tags t WHERE t.name='Patrick Kennedy 1765-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Elizabeth Hamilton 1762-1822
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Elizabeth Hamilton 1762-1822', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '119' FROM Tags t WHERE t.name='Elizabeth Hamilton 1762-1822' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Elizabeth Hamilton 1762-1822' FROM Tags t WHERE t.name='Elizabeth Hamilton 1762-1822' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael Guerin 1773-1849
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael Guerin 1773-1849', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '120' FROM Tags t WHERE t.name='Michael Guerin 1773-1849' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael Guerin 1773-1849' FROM Tags t WHERE t.name='Michael Guerin 1773-1849' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Michael O'Brien 1832-1891
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Michael O''Brien 1832-1891', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '121' FROM Tags t WHERE t.name='Michael O''Brien 1832-1891' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Michael O''Brien 1832-1891' FROM Tags t WHERE t.name='Michael O''Brien 1832-1891' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Gilbert Kennedy 1766-1851
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Gilbert Kennedy 1766-1851', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '122' FROM Tags t WHERE t.name='Gilbert Kennedy 1766-1851' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Gilbert Kennedy 1766-1851' FROM Tags t WHERE t.name='Gilbert Kennedy 1766-1851' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Johanna 1785-1860
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Johanna 1785-1860', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '123' FROM Tags t WHERE t.name='Mary Johanna 1785-1860' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Johanna 1785-1860' FROM Tags t WHERE t.name='Mary Johanna 1785-1860' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Ann Newman Kennedy 1757-1851
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Ann Newman Kennedy 1757-1851', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '124' FROM Tags t WHERE t.name='Ann Newman Kennedy 1757-1851' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Ann Newman Kennedy 1757-1851' FROM Tags t WHERE t.name='Ann Newman Kennedy 1757-1851' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Ellen Barry 1805-1880
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Ellen Barry 1805-1880', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '125' FROM Tags t WHERE t.name='Ellen Barry 1805-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Ellen Barry 1805-1880' FROM Tags t WHERE t.name='Ellen Barry 1805-1880' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Ann Nancy Clayton Pendleton 1777-1854
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Ann Nancy Clayton Pendleton 1777-1854', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '126' FROM Tags t WHERE t.name='Ann Nancy Clayton Pendleton 1777-1854' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Ann Nancy Clayton Pendleton 1777-1854' FROM Tags t WHERE t.name='Ann Nancy Clayton Pendleton 1777-1854' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Humphrey Charles Mahoney 1847-1928
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Humphrey Charles Mahoney 1847-1928', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '127' FROM Tags t WHERE t.name='Humphrey Charles Mahoney 1847-1928' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Humphrey Charles Mahoney 1847-1928' FROM Tags t WHERE t.name='Humphrey Charles Mahoney 1847-1928' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Abram Elmendorf Schoonmaker 1828-1904
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Abram Elmendorf Schoonmaker 1828-1904', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '128' FROM Tags t WHERE t.name='Abram Elmendorf Schoonmaker 1828-1904' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Abram Elmendorf Schoonmaker 1828-1904' FROM Tags t WHERE t.name='Abram Elmendorf Schoonmaker 1828-1904' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Jane Appleby 1773-1859
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Jane Appleby 1773-1859', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '129' FROM Tags t WHERE t.name='Jane Appleby 1773-1859' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Jane Appleby 1773-1859' FROM Tags t WHERE t.name='Jane Appleby 1773-1859' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Dunkirk 1765-1850
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Dunkirk 1765-1850', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '130' FROM Tags t WHERE t.name='Mary Dunkirk 1765-1850' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Dunkirk 1765-1850' FROM Tags t WHERE t.name='Mary Dunkirk 1765-1850' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Herman Hostetter 1760-1812
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Herman Hostetter 1760-1812', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '131' FROM Tags t WHERE t.name='Herman Hostetter 1760-1812' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Herman Hostetter 1760-1812' FROM Tags t WHERE t.name='Herman Hostetter 1760-1812' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Caroline Bouvier Kennedy 1957-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Caroline Bouvier Kennedy 1957-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '132' FROM Tags t WHERE t.name='Caroline Bouvier Kennedy 1957-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Caroline Bouvier Kennedy 1957-unknown' FROM Tags t WHERE t.name='Caroline Bouvier Kennedy 1957-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Vernou Bouvier 1891-1957
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Vernou Bouvier 1891-1957', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '133' FROM Tags t WHERE t.name='John Vernou Bouvier 1891-1957' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Vernou Bouvier 1891-1957' FROM Tags t WHERE t.name='John Vernou Bouvier 1891-1957' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Janet Norton Lee 1907-1989
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Janet Norton Lee 1907-1989', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '134' FROM Tags t WHERE t.name='Janet Norton Lee 1907-1989' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Janet Norton Lee 1907-1989' FROM Tags t WHERE t.name='Janet Norton Lee 1907-1989' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Caroline Lee Bouvier 1933-2019
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Caroline Lee Bouvier 1933-2019', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '135' FROM Tags t WHERE t.name='Caroline Lee Bouvier 1933-2019' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Caroline Lee Bouvier 1933-2019' FROM Tags t WHERE t.name='Caroline Lee Bouvier 1933-2019' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Vernou Bouvier 1865-1948
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Vernou Bouvier 1865-1948', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '136' FROM Tags t WHERE t.name='John Vernou Bouvier 1865-1948' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Vernou Bouvier 1865-1948' FROM Tags t WHERE t.name='John Vernou Bouvier 1865-1948' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Maude Frances Sergeant 1869-1940
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Maude Frances Sergeant 1869-1940', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '137' FROM Tags t WHERE t.name='Maude Frances Sergeant 1869-1940' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Maude Frances Sergeant 1869-1940' FROM Tags t WHERE t.name='Maude Frances Sergeant 1869-1940' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: James Thomas Aloysius Lee 1877-1968
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'James Thomas Aloysius Lee 1877-1968', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '138' FROM Tags t WHERE t.name='James Thomas Aloysius Lee 1877-1968' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'James Thomas Aloysius Lee 1877-1968' FROM Tags t WHERE t.name='James Thomas Aloysius Lee 1877-1968' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Margaret Ann Merrit 1877-1943
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Margaret Ann Merrit 1877-1943', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '139' FROM Tags t WHERE t.name='Margaret Ann Merrit 1877-1943' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Margaret Ann Merrit 1877-1943' FROM Tags t WHERE t.name='Margaret Ann Merrit 1877-1943' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: John Francis Fitzgerald 1863-1950
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'John Francis Fitzgerald 1863-1950', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '140' FROM Tags t WHERE t.name='John Francis Fitzgerald 1863-1950' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'John Francis Fitzgerald 1863-1950' FROM Tags t WHERE t.name='John Francis Fitzgerald 1863-1950' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Mary Josephine Hannon 1865-1964
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Mary Josephine Hannon 1865-1964', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '141' FROM Tags t WHERE t.name='Mary Josephine Hannon 1865-1964' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Mary Josephine Hannon 1865-1964' FROM Tags t WHERE t.name='Mary Josephine Hannon 1865-1964' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Edwin Arthur Schlossberg 1945-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Edwin Arthur Schlossberg 1945-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '142' FROM Tags t WHERE t.name='Edwin Arthur Schlossberg 1945-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Edwin Arthur Schlossberg 1945-unknown' FROM Tags t WHERE t.name='Edwin Arthur Schlossberg 1945-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Rose Schlossberg 1988-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Rose Schlossberg 1988-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '143' FROM Tags t WHERE t.name='Rose Schlossberg 1988-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Rose Schlossberg 1988-unknown' FROM Tags t WHERE t.name='Rose Schlossberg 1988-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Tatiana Celia Kennedy Schlossberg 1990-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Tatiana Celia Kennedy Schlossberg 1990-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '144' FROM Tags t WHERE t.name='Tatiana Celia Kennedy Schlossberg 1990-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Tatiana Celia Kennedy Schlossberg 1990-unknown' FROM Tags t WHERE t.name='Tatiana Celia Kennedy Schlossberg 1990-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Jack Schlossberg 1993-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Jack Schlossberg 1993-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '145' FROM Tags t WHERE t.name='Jack Schlossberg 1993-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Jack Schlossberg 1993-unknown' FROM Tags t WHERE t.name='Jack Schlossberg 1993-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: George Winchester Moran 1989-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'George Winchester Moran 1989-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '146' FROM Tags t WHERE t.name='George Winchester Moran 1989-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'George Winchester Moran 1989-unknown' FROM Tags t WHERE t.name='George Winchester Moran 1989-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Exif Sample Test 1980-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Exif Sample Test 1980-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '147' FROM Tags t WHERE t.name='Exif Sample Test 1980-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Exif Sample Test 1980-unknown' FROM Tags t WHERE t.name='Exif Sample Test 1980-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF1 Top-left Test 2001-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF1 Top-left Test 2001-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '148' FROM Tags t WHERE t.name='EXIF1 Top-left Test 2001-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF1 Top-left Test 2001-unknown' FROM Tags t WHERE t.name='EXIF1 Top-left Test 2001-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF2 Top-right Test 2002-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF2 Top-right Test 2002-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '149' FROM Tags t WHERE t.name='EXIF2 Top-right Test 2002-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF2 Top-right Test 2002-unknown' FROM Tags t WHERE t.name='EXIF2 Top-right Test 2002-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF3 Bottom-right Test 2003-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF3 Bottom-right Test 2003-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '150' FROM Tags t WHERE t.name='EXIF3 Bottom-right Test 2003-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF3 Bottom-right Test 2003-unknown' FROM Tags t WHERE t.name='EXIF3 Bottom-right Test 2003-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF4 Bottom-left Test 2004-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF4 Bottom-left Test 2004-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '151' FROM Tags t WHERE t.name='EXIF4 Bottom-left Test 2004-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF4 Bottom-left Test 2004-unknown' FROM Tags t WHERE t.name='EXIF4 Bottom-left Test 2004-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF5 Left-top Test 2005-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF5 Left-top Test 2005-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '152' FROM Tags t WHERE t.name='EXIF5 Left-top Test 2005-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF5 Left-top Test 2005-unknown' FROM Tags t WHERE t.name='EXIF5 Left-top Test 2005-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF6 Right-top Test 2006-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF6 Right-top Test 2006-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '153' FROM Tags t WHERE t.name='EXIF6 Right-top Test 2006-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF6 Right-top Test 2006-unknown' FROM Tags t WHERE t.name='EXIF6 Right-top Test 2006-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF7 Right-bottom Test 2007-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF7 Right-bottom Test 2007-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '154' FROM Tags t WHERE t.name='EXIF7 Right-bottom Test 2007-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF7 Right-bottom Test 2007-unknown' FROM Tags t WHERE t.name='EXIF7 Right-bottom Test 2007-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: EXIF8  Left-bottom Test 2008-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'EXIF8  Left-bottom Test 2008-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '155' FROM Tags t WHERE t.name='EXIF8  Left-bottom Test 2008-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'EXIF8  Left-bottom Test 2008-unknown' FROM Tags t WHERE t.name='EXIF8  Left-bottom Test 2008-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Exif Mom Test 1981-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Exif Mom Test 1981-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '156' FROM Tags t WHERE t.name='Exif Mom Test 1981-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Exif Mom Test 1981-unknown' FROM Tags t WHERE t.name='Exif Mom Test 1981-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: File Formats Test 1950-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'File Formats Test 1950-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '157' FROM Tags t WHERE t.name='File Formats Test 1950-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'File Formats Test 1950-unknown' FROM Tags t WHERE t.name='File Formats Test 1950-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Bad Formats Dad Test 1983-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Bad Formats Dad Test 1983-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '158' FROM Tags t WHERE t.name='Bad Formats Dad Test 1983-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Bad Formats Dad Test 1983-unknown' FROM Tags t WHERE t.name='Bad Formats Dad Test 1983-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Bad Formats Mom Testing 1983-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Bad Formats Mom Testing 1983-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '159' FROM Tags t WHERE t.name='Bad Formats Mom Testing 1983-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Bad Formats Mom Testing 1983-unknown' FROM Tags t WHERE t.name='Bad Formats Mom Testing 1983-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Format 1 Test 2000-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Format 1 Test 2000-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '160' FROM Tags t WHERE t.name='Format 1 Test 2000-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Format 1 Test 2000-unknown' FROM Tags t WHERE t.name='Format 1 Test 2000-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Format 2 Test 2003-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Format 2 Test 2003-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '161' FROM Tags t WHERE t.name='Format 2 Test 2003-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Format 2 Test 2003-unknown' FROM Tags t WHERE t.name='Format 2 Test 2003-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Format 3 Test 2006-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Format 3 Test 2006-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '162' FROM Tags t WHERE t.name='Format 3 Test 2006-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Format 3 Test 2006-unknown' FROM Tags t WHERE t.name='Format 3 Test 2006-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

-- Create tag for: Format 4 Test 2009-unknown
INSERT OR IGNORE INTO Tags (name, pid, icon, iconkde) SELECT 'Format 4 Test 2009-unknown', id, NULL, 'user' FROM Tags WHERE name='RootsMagic';
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'rootsmagic_owner_id', '163' FROM Tags t WHERE t.name='Format 4 Test 2009-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'rootsmagic_owner_id');
INSERT OR IGNORE INTO TagProperties (tagid, property, value) SELECT t.id, 'person', 'Format 4 Test 2009-unknown' FROM Tags t WHERE t.name='Format 4 Test 2009-unknown' AND NOT EXISTS (SELECT 1 FROM TagProperties tp                WHERE tp.tagid = t.id                AND tp.property = 'person');

COMMIT;
