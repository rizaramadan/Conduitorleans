CREATE INDEX user_details ON orleansstorage ((payloadjson->>'Email')) 
WHERE graintypestring = 'Grains.Security.UserGrain,Grains.UserGrain';

