# Conduitorleans Database

conduitorleans.sql contains script to fill a postgre sql database to be used 
as cluster management and grain persistence. 
**It is mandatory to prepare the database or the software wont work.**

To make the database ready:
- install postgresql version 12 or newer on localhost
- have the database listen at ip localhost and port 5432
- create database with name **conduitorleans** (lowercase)
- execute conduitorleans.sql for that database
- done

#### footnote
The database structure here is slightly different compare to the Orleans default,
and that is of the sake of changing the payloadjson column type. In the original, 
coloum type is text.In this version, the column type is jsonb. 

