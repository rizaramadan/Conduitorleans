# Conduitorleans Database

conduitorleans.sql contains script to fill a postgre sql database to be used 
as cluster management and grain persistence. 
**It is mandatory to prepare the database or the software wont work.**

To make the database ready:
- install postgresql version 12 or newer on localhost
- have the database listen at ip localhost and port 5432
- create database with name **conduitorleans** (lowercase)
- execute [conduitorleans_all.sql](https://github.com/rizaramadan/Conduitorleans/blob/main/scripts/conduitorleans_all.sql) for that database, this will create all items in database
- done


#### Update Database
Since this repo is still work in progress, people might have restore db from
script conduitorleans_all.sql (or previously conduitorleans.sql). This might impose
a problem when there's an database update in the future.

In absence of more elegant solution, this folder will have two type of script:
1. incremental scripts, a file scripts with start with 2 digit numbers, for example like [00_conduitorleans.sql](https://github.com/rizaramadan/Conduitorleans/blob/main/scripts/00_conduitorleans.sql)
2. all at once script, and that is [conduitorleans_all.sql](https://github.com/rizaramadan/Conduitorleans/blob/main/scripts/conduitorleans_all.sql) for that database, this will create all items in database

Use the conduitorleans_all.sql from empty database all the way to latest version.
Use the numbered one if a new script exist in the future. 


#### footnote
The database structure here is slightly different compare to the Orleans default,
and that is of the sake of changing the payloadjson column type. In the original, 
coloum type is text.In this version, the column type is jsonb. 

