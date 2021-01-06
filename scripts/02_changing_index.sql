DROP INDEX user_details;
CREATE INDEX user_details ON orleansstorage ((payloadjson->>'Email')) 
WHERE graintypestring = 'Grains.Users.UserGrain,Grains.UserGrain';